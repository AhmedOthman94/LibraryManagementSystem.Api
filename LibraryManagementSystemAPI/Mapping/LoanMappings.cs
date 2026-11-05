using LibraryManagementSystemAPI.Common;
using LibraryManagementSystemAPI.Dtos;
using LibraryManagementSystemAPI.Entities;

namespace LibraryManagementSystemAPI.Mappings
{
	public static class LoanMappings
	{
		// Needs Member and BookCopy.Book to be loaded (Include)
		public static LoanResponse ToResponse(
			this Loan loan,
			DateOnly today,
			LibraryPolicyOptions policy)
		{
			var daysOverdue = loan.DaysOverdue(today);

			// Returned → the saved snapshot. Open → the fine so far.
			var fineAmount = loan.ReturnedOn is null
				? FineCalculator.Calculate(daysOverdue, policy)
				: loan.FineAmount ?? 0m;

			return new LoanResponse(
				loan.Id,
				new LoanMemberSummary(
					loan.Member.Id,
					MembershipCard.Format(loan.Member.MembershipNumber),
					$"{loan.Member.FirstName} {loan.Member.LastName}"),
				new LoanCopySummary(
					loan.BookCopy.Id,
					loan.BookCopy.Barcode,
					loan.BookCopy.BookId,
					loan.BookCopy.Book.Title),
				loan.BorrowedOn,
				loan.DueOn,
				loan.ReturnedOn,
				loan.RenewalCount,
				loan.GetStatus(today),
				daysOverdue,
				fineAmount,
				loan.CreatedAt,
				loan.UpdatedAt
			);
		}
	}
}