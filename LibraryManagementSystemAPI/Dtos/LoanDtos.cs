using LibraryManagementSystemAPI.Entities;

namespace LibraryManagementSystemAPI.Dtos
{
	// What the librarian scans at the desk
	public record BorrowBookRequest(
		string CardNumber,
		string Barcode
	);

	// Optional: the librarian noticed damage when the book came back
	public record ReturnLoanRequest(bool NeedsMaintenance);

	public record LoanMemberSummary(Guid Id, string CardNumber, string FullName);

	public record LoanCopySummary(Guid Id, string Barcode, Guid BookId, string BookTitle);

	public record LoanResponse(
		Guid Id,
		LoanMemberSummary Member,
		LoanCopySummary Copy,
		DateOnly BorrowedOn,
		DateOnly DueOn,
		DateOnly? ReturnedOn,
		int RenewalCount,
		LoanStatus Status,
		int DaysOverdue,
		decimal FineAmount,
		DateTime CreatedAt,
		DateTime? UpdatedAt
	);

	public class LoanQueryParameters
	{
		public Guid? MemberId { get; set; }
		public Guid? BookId { get; set; }

		// "Active", "Overdue", or "Returned"
		public string? Status { get; set; }

		public int? Page { get; set; }
		public int? PageSize { get; set; }
	}
}