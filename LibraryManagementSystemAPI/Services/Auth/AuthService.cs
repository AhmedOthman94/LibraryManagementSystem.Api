using LibraryManagementSystemAPI.Auth;
using LibraryManagementSystemAPI.Common;
using LibraryManagementSystemAPI.DataAccess;
using LibraryManagementSystemAPI.Dtos;
using LibraryManagementSystemAPI.Entities;
using LibraryManagementSystemAPI.Mappings;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystemAPI.Services.Auth
{
	public class AuthService(
		LibraryDbContext db,
		IPasswordHasher<User> passwordHasher,
		ITokenService tokenService) : IAuthService
	{
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

			// The password is right, but was hashed with older, weaker settings: upgrade it now
			if (verification == PasswordVerificationResult.SuccessRehashNeeded)
			{
				user.PasswordHash = passwordHasher.HashPassword(user, request.Password);
				await db.SaveChangesAsync(ct);
			}

			return CreateAuthResponse(user);
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

			// Card AND email must belong to the same member
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
			await db.SaveChangesAsync(ct);

			// Log the member in right away
			return CreateAuthResponse(user);
		}

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

		private AuthResponse CreateAuthResponse(User user)
		{
			var token = tokenService.CreateAccessToken(user);
			return new AuthResponse(token.Token, token.ExpiresAt, user.ToResponse());
		}
	}
}