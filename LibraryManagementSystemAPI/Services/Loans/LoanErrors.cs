using LibraryManagementSystemAPI.Common;
using LibraryManagementSystemAPI.Entities;

namespace LibraryManagementSystemAPI.Services.Loans
{
	public static class LoanErrors
	{
		// 404: the loan in the URL
		public static Error NotFound(Guid id) =>
			Error.NotFound($"Loan with Id: {id} was not found.");

		// 400: things in the BODY that don't exist
		public static Error MemberCardNotFound(string cardNumber) =>
			Error.Validation($"No member found with card number: {cardNumber}.");

		public static Error CopyNotFound(string barcode) =>
			Error.Validation($"No copy found with barcode: {barcode}.");

		// 409: business rules
		public static Error MemberSuspended =>
			Error.Conflict("This member is suspended and can't borrow books.");

		public static Error MembershipExpired =>
			Error.Conflict("This membership has expired. Renew it before borrowing.");

		public static Error MemberHasOverdueLoans =>
			Error.Conflict("This member has overdue books. They must be returned first.");

		public static Error LoanLimitReached(int max) =>
			Error.Conflict($"This member already has the maximum of {max} books on loan.");

		public static Error CopyNotAvailable(string barcode, CopyStatus status) =>
			Error.Conflict($"Copy {barcode} is not available. Its current status is {status}.");

		public static Error AlreadyReturned =>
			Error.Conflict("This loan was already returned.");

		public static Error CannotRenewOverdue =>
			Error.Conflict("An overdue loan can't be renewed. It must be returned first.");

		public static Error RenewalLimitReached(int max) =>
			Error.Conflict($"This loan was already renewed the maximum of {max} times.");
	}
}