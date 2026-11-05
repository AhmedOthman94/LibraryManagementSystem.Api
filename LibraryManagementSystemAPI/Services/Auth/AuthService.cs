using LibraryManagementSystemAPI.Auth;
using LibraryManagementSystemAPI.Common;
using LibraryManagementSystemAPI.DataAccess;
using LibraryManagementSystemAPI.Dtos;
using LibraryManagementSystemAPI.Entities;
using LibraryManagementSystemAPI.Mappings;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace LibraryManagementSystemAPI.Services.Auth
{
	public class AuthService(
		LibraryDbContext db,
		IPasswordHasher<User> passwordHasher,
		ITokenService tokenService,
		TimeProvider timeProvider,
		IOptions<JwtOptions> jwtOptions,
		ILogger<AuthService> logger) : IAuthService
	{
		private readonly JwtOptions _jwt = jwtOptions.Value;

		private DateTime Now => timeProvider.GetUtcNow().UtcDateTime;

		// ---------- Login and registration ----------

		public async Task<Result<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken ct)
		{
			var email = EmailHelper.Normalize(request.Email);

			var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email, ct);

			if (user is null)
			{
				return AuthErrors.InvalidCredentials;
			}

			var verification = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);

			if (verification == PasswordVerificationResult.Failed)
			{
				return AuthErrors.InvalidCredentials;
			}

			if (verification == PasswordVerificationResult.SuccessRehashNeeded)
			{
				user.PasswordHash = passwordHasher.HashPassword(user, request.Password);
			}

			// Every login starts a NEW session (a new family)
			var (response, _) = IssueTokens(user, familyId: Guid.CreateVersion7());

			// One save: the new refresh token (and maybe the upgraded hash)
			await db.SaveChangesAsync(ct);

			return response;
		}

		public async Task<Result<AuthResponse>> RegisterMemberAccountAsync(
			RegisterMemberAccountRequest request,
			CancellationToken ct)
		{
			if (!MembershipCard.TryParse(request.CardNumber, out var cardNumber))
			{
				return AuthErrors.MemberNotMatched;
			}

			var email = EmailHelper.Normalize(request.Email);

			var member = await db.Members.FirstOrDefaultAsync(
				m => m.MembershipNumber == cardNumber && m.Email == email,
				ct);

			if (member is null)
			{
				return AuthErrors.MemberNotMatched;
			}

			if (await db.Users.AnyAsync(u => u.MemberId == member.Id, ct))
			{
				return AuthErrors.MemberAlreadyHasAccount;
			}

			if (await db.Users.AnyAsync(u => u.Email == email, ct))
			{
				return AuthErrors.EmailAlreadyUsed;
			}

			var user = new User
			{
				Email = email,
				PasswordHash = string.Empty,
				Role = UserRole.Member,
				MemberId = member.Id
			};

			user.PasswordHash = passwordHasher.HashPassword(user, request.Password);
			db.Users.Add(user);

			var (response, _) = IssueTokens(user, familyId: Guid.CreateVersion7());

			// One save: the new user AND their first refresh token, together
			await db.SaveChangesAsync(ct);

			return response;
		}

		// ---------- Sessions ----------

		public async Task<Result<AuthResponse>> RefreshAsync(RefreshTokenRequest request, CancellationToken ct)
		{
			var hash = RefreshTokenGenerator.Hash(request.RefreshToken);

			var current = await db.RefreshTokens
				.Include(t => t.User)
				.FirstOrDefaultAsync(t => t.TokenHash == hash, ct);

			if (current is null)
			{
				return AuthErrors.InvalidRefreshToken;
			}

			var now = Now;

			if (current.RevokedAt is not null)
			{
				// Used normally before (it has a replacement), and now used AGAIN:
				// someone has a copy. End the whole session for everyone.
				if (current.ReplacedByTokenId is not null)
				{
					await RevokeActiveAsync(
						db.RefreshTokens.Where(t => t.FamilyId == current.FamilyId),
						now, "Reuse detected", ct);

					logger.LogWarning(
						"Refresh token reuse detected for user {UserId}. Session {FamilyId} was revoked.",
						current.UserId, current.FamilyId);
				}

				return AuthErrors.InvalidRefreshToken;
			}

			if (current.ExpiresAt <= now)
			{
				return AuthErrors.InvalidRefreshToken;
			}

			// Rotation: same session (family), new voucher
			var (response, next) = IssueTokens(current.User, current.FamilyId);

			current.RevokedAt = now;
			current.RevokedReason = "Rotated";
			current.ReplacedByTokenId = next.Id;

			// One save: old token used + new token created, together
			await db.SaveChangesAsync(ct);

			return response;
		}

		public async Task LogoutAsync(LogoutRequest request, CancellationToken ct)
		{
			var hash = RefreshTokenGenerator.Hash(request.RefreshToken);

			var token = await db.RefreshTokens
				.AsNoTracking()
				.FirstOrDefaultAsync(t => t.TokenHash == hash, ct);

			if (token is null)
			{
				return; // nothing to do, and nothing to reveal
			}

			await RevokeActiveAsync(
				db.RefreshTokens.Where(t => t.FamilyId == token.FamilyId),
				Now, "Logged out", ct);
		}

		public async Task LogoutAllAsync(Guid userId, CancellationToken ct)
		{
			await RevokeActiveAsync(
				db.RefreshTokens.Where(t => t.UserId == userId),
				Now, "Logged out everywhere", ct);
		}

		public async Task<Result<AuthResponse>> ChangePasswordAsync(
			Guid userId,
			ChangePasswordRequest request,
			CancellationToken ct)
		{
			var user = await db.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);

			if (user is null)
			{
				return AuthErrors.UserNotFound(userId);
			}

			var verification = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.CurrentPassword);

			if (verification == PasswordVerificationResult.Failed)
			{
				return AuthErrors.CurrentPasswordIncorrect;
			}

			// Two separate database trips (bulk UPDATE + SaveChanges) → explicit transaction
			await using var transaction = await db.Database.BeginTransactionAsync(ct);

			// 1) End every existing session (one SQL UPDATE, runs immediately)
			await RevokeActiveAsync(
				db.RefreshTokens.Where(t => t.UserId == user.Id),
				Now, "Password changed", ct);

			// 2) New password + a fresh session for THIS device
			user.PasswordHash = passwordHasher.HashPassword(user, request.NewPassword);
			var (response, _) = IssueTokens(user, familyId: Guid.CreateVersion7());

			await db.SaveChangesAsync(ct);

			// Only now do both changes become permanent
			await transaction.CommitAsync(ct);

			return response;
		}

		// ---------- Staff and profile ----------

		public async Task<Result<UserResponse>> CreateStaffUserAsync(
			CreateStaffUserRequest request,
			CancellationToken ct)
		{
			var email = EmailHelper.Normalize(request.Email);

			if (await db.Users.AnyAsync(u => u.Email == email, ct))
			{
				return AuthErrors.EmailAlreadyUsed;
			}

			var user = new User
			{
				Email = email,
				PasswordHash = string.Empty,
				Role = request.Role
			};

			user.PasswordHash = passwordHasher.HashPassword(user, request.Password);

			db.Users.Add(user);
			await db.SaveChangesAsync(ct);

			return user.ToResponse();
		}

		public async Task<Result<UserResponse>> GetUserAsync(Guid id, CancellationToken ct)
		{
			var user = await db.Users
				.AsNoTracking()
				.FirstOrDefaultAsync(u => u.Id == id, ct);

			if (user is null)
			{
				return AuthErrors.UserNotFound(id);
			}

			return user.ToResponse();
		}

		// ---------- Private helpers ----------

		// Creates an access token + a refresh token, and ADDS the refresh token to the
		// context. It does NOT save: the caller saves once, with its other changes.
		private (AuthResponse Response, RefreshToken Entity) IssueTokens(User user, Guid familyId)
		{
			var accessToken = tokenService.CreateAccessToken(user);

			var now = Now;
			var plainRefreshToken = RefreshTokenGenerator.Generate();

			var refreshToken = new RefreshToken
			{
				UserId = user.Id,
				TokenHash = RefreshTokenGenerator.Hash(plainRefreshToken),
				FamilyId = familyId,
				CreatedAt = now,
				ExpiresAt = now.AddDays(_jwt.RefreshTokenDays)
			};

			db.RefreshTokens.Add(refreshToken);

			// The plain token goes to the client ONCE and is never stored
			var response = new AuthResponse(
				accessToken.Token,
				accessToken.ExpiresAt,
				plainRefreshToken,
				refreshToken.ExpiresAt,
				user.ToResponse());

			return (response, refreshToken);
		}

		// One SQL UPDATE for all matching active tokens, without loading them
		private static Task<int> RevokeActiveAsync(
			IQueryable<RefreshToken> tokens,
			DateTime now,
			string reason,
			CancellationToken ct) =>
			tokens
				.Where(t => t.RevokedAt == null)
				.ExecuteUpdateAsync(setters => setters
					.SetProperty(t => t.RevokedAt, (DateTime?)now)
					.SetProperty(t => t.RevokedReason, reason), ct);
	}
}