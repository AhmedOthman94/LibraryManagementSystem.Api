using LibraryManagementSystemAPI.Common;
using LibraryManagementSystemAPI.DataAccess;
using LibraryManagementSystemAPI.Dtos;
using LibraryManagementSystemAPI.Entities;
using LibraryManagementSystemAPI.Mappings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace LibraryManagementSystemAPI.Services.Members
{
	public class MemberService(
		LibraryDbContext db,
		TimeProvider timeProvider,
		IOptions<LibraryPolicyOptions> policyOptions) : IMemberService
	{
		private readonly LibraryPolicyOptions _policy = policyOptions.Value;

		// One place that decides what "today" is
		private DateOnly Today =>
			DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);

		public async Task<PagedResponse<MemberResponse>> GetAllAsync(
			MemberQueryParameters parameters,
			CancellationToken ct)
		{
			var today = Today;
			var query = db.Members.AsNoTracking();

			if (!string.IsNullOrWhiteSpace(parameters.Search))
			{
				var pattern = LikePattern.Contains(parameters.Search);
				var isCardNumber = MembershipCard.TryParse(parameters.Search, out var cardNumber);

				query = query.Where(m =>
					EF.Functions.ILike(m.FirstName + " " + m.LastName, pattern)
					|| EF.Functions.ILike(m.Email, pattern)
					|| (isCardNumber && m.MembershipNumber == cardNumber));
			}

			if (!string.IsNullOrWhiteSpace(parameters.Status))
			{
				var status = Enum.Parse<MemberStatus>(parameters.Status, ignoreCase: true);
				query = query.Where(m => m.Status == status);
			}

			if (parameters.ExpiredOnly == true)
			{
				// Not m.IsMembershipExpired(today): EF can't translate our own methods to SQL
				query = query.Where(m => m.MembershipExpiresOn < today);
			}

			query = query
				.OrderBy(m => m.LastName)
				.ThenBy(m => m.FirstName)
				.ThenBy(m => m.Id);

			var page = parameters.Page ?? PaginationDefaults.Page;
			var pageSize = parameters.PageSize ?? PaginationDefaults.PageSize;

			return await query.ToPagedResponseAsync(page, pageSize, m => m.ToResponse(today), ct);
		}

		public async Task<Result<MemberResponse>> GetByIdAsync(Guid id, CancellationToken ct)
		{
			var member = await db.Members
				.AsNoTracking()
				.FirstOrDefaultAsync(m => m.Id == id, ct);

			if (member is null)
			{
				return MemberErrors.NotFound(id);
			}

			return member.ToResponse(Today);
		}

		public async Task<Result<MemberResponse>> CreateAsync(
			CreateMemberRequest request,
			CancellationToken ct)
		{
			var email = EmailHelper.Normalize(request.Email);

			if (await EmailExistsAsync(email, excludeId: null, ct))
			{
				return MemberErrors.EmailAlreadyExists(email);
			}

			var today = Today;
			var expiresOn = today.AddMonths(_policy.MembershipDurationMonths);

			var member = request.ToEntity(expiresOn);

			db.Members.Add(member);
			await db.SaveChangesAsync(ct);

			// MembershipNumber was filled by PostgreSQL during SaveChanges
			return member.ToResponse(today);
		}

		public async Task<Result<MemberResponse>> UpdateAsync(
			Guid id,
			UpdateMemberRequest request,
			CancellationToken ct)
		{
			var member = await db.Members.FirstOrDefaultAsync(m => m.Id == id, ct);

			if (member is null)
			{
				return MemberErrors.NotFound(id);
			}

			var email = EmailHelper.Normalize(request.Email);

			if (await EmailExistsAsync(email, excludeId: id, ct))
			{
				return MemberErrors.EmailAlreadyExists(email);
			}

			request.ApplyTo(member);
			await db.SaveChangesAsync(ct);

			return member.ToResponse(Today);
		}

		public async Task<Result<MemberResponse>> SuspendAsync(
			Guid id,
			SuspendMemberRequest request,
			CancellationToken ct)
		{
			var member = await db.Members.FirstOrDefaultAsync(m => m.Id == id, ct);

			if (member is null)
			{
				return MemberErrors.NotFound(id);
			}

			if (member.Status == MemberStatus.Suspended)
			{
				return MemberErrors.AlreadySuspended;
			}

			member.Status = MemberStatus.Suspended;
			member.SuspensionReason = request.Reason.Trim();
			await db.SaveChangesAsync(ct);

			return member.ToResponse(Today);
		}

		public async Task<Result<MemberResponse>> ReactivateAsync(Guid id, CancellationToken ct)
		{
			var member = await db.Members.FirstOrDefaultAsync(m => m.Id == id, ct);

			if (member is null)
			{
				return MemberErrors.NotFound(id);
			}

			if (member.Status != MemberStatus.Suspended)
			{
				return MemberErrors.NotSuspended;
			}

			member.Status = MemberStatus.Active;
			member.SuspensionReason = null;
			await db.SaveChangesAsync(ct);

			return member.ToResponse(Today);
		}

		public async Task<Result<MemberResponse>> RenewAsync(Guid id, CancellationToken ct)
		{
			var member = await db.Members.FirstOrDefaultAsync(m => m.Id == id, ct);

			if (member is null)
			{
				return MemberErrors.NotFound(id);
			}

			if (member.Status == MemberStatus.Suspended)
			{
				return MemberErrors.SuspendedCannotRenew;
			}

			var today = Today;

			// Still valid → extend from the expiry date (keep remaining days)
			// Already expired → start again from today
			var startFrom = member.MembershipExpiresOn > today
				? member.MembershipExpiresOn
				: today;

			member.MembershipExpiresOn = startFrom.AddMonths(_policy.MembershipDurationMonths);
			await db.SaveChangesAsync(ct);

			return member.ToResponse(today);
		}

		public async Task<Result> DeleteAsync(Guid id, CancellationToken ct)
		{
			var member = await db.Members.FirstOrDefaultAsync(m => m.Id == id, ct);

			if (member is null)
			{
				return MemberErrors.NotFound(id);
			}

			if (await db.Loans.AnyAsync(l => l.MemberId == id && l.ReturnedOn == null, ct))
			{
				return MemberErrors.HasActiveLoans;
			}
			// The member's login account goes with them (same SaveChanges = same transaction)
			var account = await db.Users.FirstOrDefaultAsync(u => u.MemberId == id, ct);

			if (account is not null)
			{
				db.Users.Remove(account);
			}

			db.Members.Remove(member); // becomes a soft delete
			await db.SaveChangesAsync(ct);

			return Result.Success();
		}

		private Task<bool> EmailExistsAsync(string normalizedEmail, Guid? excludeId, CancellationToken ct) =>
			db.Members.AnyAsync(
				m => m.Id != excludeId && m.Email == normalizedEmail,
				ct);
	}
}