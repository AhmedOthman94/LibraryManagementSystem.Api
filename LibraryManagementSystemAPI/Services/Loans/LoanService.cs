using LibraryManagementSystemAPI.Common;
using LibraryManagementSystemAPI.DataAccess;
using LibraryManagementSystemAPI.Dtos;
using LibraryManagementSystemAPI.Entities;
using LibraryManagementSystemAPI.Mappings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace LibraryManagementSystemAPI.Services.Loans
{
	public class LoanService(
		LibraryDbContext db,
		TimeProvider timeProvider,
		IOptions<LibraryPolicyOptions> policyOptions) : ILoanService
	{
		private readonly LibraryPolicyOptions _policy = policyOptions.Value;

		private DateOnly Today =>
			DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);

		public async Task<PagedResponse<LoanResponse>> GetAllAsync(
			LoanQueryParameters parameters,
			CancellationToken ct)
		{
			var today = Today;
			var query = LoansWithDetails().AsNoTracking();

			if (parameters.MemberId.HasValue)
			{
				query = query.Where(l => l.MemberId == parameters.MemberId.Value);
			}

			if (parameters.BookId.HasValue)
			{
				query = query.Where(l => l.BookCopy.BookId == parameters.BookId.Value);
			}

			// The status is calculated, so we filter by the dates behind it
			query = parameters.Status?.ToLowerInvariant() switch
			{
				"active" => query.Where(l => l.ReturnedOn == null && l.DueOn >= today),
				"overdue" => query.Where(l => l.ReturnedOn == null && l.DueOn < today),
				"returned" => query.Where(l => l.ReturnedOn != null),
				_ => query
			};

			query = query
				.OrderByDescending(l => l.BorrowedOn)
				.ThenBy(l => l.Id);

			var page = parameters.Page ?? PaginationDefaults.Page;
			var pageSize = parameters.PageSize ?? PaginationDefaults.PageSize;

			return await query.ToPagedResponseAsync(page, pageSize, l => l.ToResponse(today, _policy), ct);
		}

		public async Task<Result<LoanResponse>> GetByIdAsync(Guid id, CancellationToken ct)
		{
			var loan = await LoansWithDetails()
				.AsNoTracking()
				.FirstOrDefaultAsync(l => l.Id == id, ct);

			if (loan is null)
			{
				return LoanErrors.NotFound(id);
			}

			return loan.ToResponse(Today, _policy);
		}

		public async Task<Result<LoanResponse>> BorrowAsync(
			BorrowBookRequest request,
			CancellationToken ct)
		{
			var today = Today;

			// 1) Who is borrowing?
			if (!MembershipCard.TryParse(request.CardNumber, out var cardNumber))
			{
				return LoanErrors.MemberCardNotFound(request.CardNumber);
			}

			var member = await db.Members
				.FirstOrDefaultAsync(m => m.MembershipNumber == cardNumber, ct);

			if (member is null)
			{
				return LoanErrors.MemberCardNotFound(request.CardNumber);
			}

			// 2) Is the member allowed to borrow? (the rule lives on the entity)
			if (!member.CanBorrow(today))
			{
				return member.Status == MemberStatus.Suspended
					? LoanErrors.MemberSuspended
					: LoanErrors.MembershipExpired;
			}

			// 3) No overdue books
			var hasOverdueLoans = await db.Loans.AnyAsync(
				l => l.MemberId == member.Id && l.ReturnedOn == null && l.DueOn < today,
				ct);

			if (hasOverdueLoans)
			{
				return LoanErrors.MemberHasOverdueLoans;
			}

			// 4) Under the limit
			var activeLoanCount = await db.Loans.CountAsync(
				l => l.MemberId == member.Id && l.ReturnedOn == null,
				ct);

			if (activeLoanCount >= _policy.MaxActiveLoansPerMember)
			{
				return LoanErrors.LoanLimitReached(_policy.MaxActiveLoansPerMember);
			}

			// 5) Which copy?
			var barcode = BarcodeHelper.Normalize(request.Barcode);

			var copy = await db.BookCopies
				.FirstOrDefaultAsync(c => c.Barcode == barcode, ct);

			if (copy is null)
			{
				return LoanErrors.CopyNotFound(barcode);
			}

			// 6) Is it on the shelf?
			if (copy.Status != CopyStatus.Available)
			{
				return LoanErrors.CopyNotAvailable(barcode, copy.Status);
			}

			// All checks passed: create the loan AND mark the copy...
			var loan = new Loan
			{
				MemberId = member.Id,
				BookCopyId = copy.Id,
				BorrowedOn = today,
				DueOn = today.AddDays(_policy.LoanPeriodDays)
			};

			db.Loans.Add(loan);
			copy.Status = CopyStatus.OnLoan;

			// ...then save ONCE: one transaction, both changes or neither.
			// If another desk changed this copy meanwhile, the version check
			// throws DbUpdateConcurrencyException and nothing is saved.
			await db.SaveChangesAsync(ct);

			return await GetResponseAsync(loan.Id, today, ct);
		}

		public async Task<Result<LoanResponse>> ReturnAsync(
			Guid id,
			ReturnLoanRequest? request,
			CancellationToken ct)
		{
			var loan = await db.Loans
				.Include(l => l.BookCopy)
				.FirstOrDefaultAsync(l => l.Id == id, ct);

			if (loan is null)
			{
				return LoanErrors.NotFound(id);
			}

			if (loan.ReturnedOn is not null)
			{
				return LoanErrors.AlreadyReturned;
			}

			var today = Today;

			loan.ReturnedOn = today;

			// Snapshot: freeze the fine with TODAY's policy
			loan.FineAmount = FineCalculator.Calculate(loan.DaysOverdue(today), _policy);

			loan.BookCopy.Status = request?.NeedsMaintenance == true
				? CopyStatus.Maintenance
				: CopyStatus.Available;

			// One save: loan closed + copy back on the shelf, together
			await db.SaveChangesAsync(ct);

			return await GetResponseAsync(id, today, ct);
		}

		public async Task<Result<LoanResponse>> RenewAsync(Guid id, CancellationToken ct)
		{
			var loan = await db.Loans
				.Include(l => l.Member)
				.FirstOrDefaultAsync(l => l.Id == id, ct);

			if (loan is null)
			{
				return LoanErrors.NotFound(id);
			}

			var today = Today;

			if (loan.ReturnedOn is not null)
			{
				return LoanErrors.AlreadyReturned;
			}

			if (loan.IsOverdue(today))
			{
				return LoanErrors.CannotRenewOverdue;
			}

			if (loan.RenewalCount >= _policy.MaxRenewals)
			{
				return LoanErrors.RenewalLimitReached(_policy.MaxRenewals);
			}

			if (!loan.Member.CanBorrow(today))
			{
				return loan.Member.Status == MemberStatus.Suspended
					? LoanErrors.MemberSuspended
					: LoanErrors.MembershipExpired;
			}

			// Extend from the current due date, so no remaining days are lost
			loan.DueOn = loan.DueOn.AddDays(_policy.LoanPeriodDays);
			loan.RenewalCount++;

			await db.SaveChangesAsync(ct);

			return await GetResponseAsync(id, today, ct);
		}

		// ---------- Private helpers ----------

		// History must still show members, copies, and books that were deleted later
		private IQueryable<Loan> LoansWithDetails() =>
			db.Loans
				.IgnoreQueryFilters()       // turns off ALL soft-delete filters in this query...
				.Where(l => !l.IsDeleted)   // ...so we re-apply the loan's own filter by hand
				.Include(l => l.Member)
				.Include(l => l.BookCopy)
					.ThenInclude(c => c.Book);

		private async Task<LoanResponse> GetResponseAsync(Guid id, DateOnly today, CancellationToken ct)
		{
			var loan = await LoansWithDetails()
				.AsNoTracking()
				.FirstAsync(l => l.Id == id, ct);

			return loan.ToResponse(today, _policy);
		}
	}
}