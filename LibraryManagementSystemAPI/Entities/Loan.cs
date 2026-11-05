namespace LibraryManagementSystemAPI.Entities
{
	// A record of "who had which copy, and when". Never edited or deleted, only completed.
	public class Loan : BaseEntity
	{
		public Guid MemberId { get; set; }
		public Member Member { get; set; } = null!;

		public Guid BookCopyId { get; set; }
		public BookCopy BookCopy { get; set; } = null!;

		// Facts
		public DateOnly BorrowedOn { get; set; }
		public DateOnly DueOn { get; set; }
		public DateOnly? ReturnedOn { get; set; }
		public int RenewalCount { get; set; }

		// Snapshot: saved at return time, so later policy changes can't alter it
		public decimal? FineAmount { get; set; }

		// ---------- Questions about this loan ----------

		public bool IsOverdue(DateOnly today) =>
			ReturnedOn is null && DueOn < today;

		// Returned loan: counts until the return date. Open loan: until today.
		public int DaysOverdue(DateOnly today)
		{
			var endDate = ReturnedOn ?? today;
			return Math.Max(0, endDate.DayNumber - DueOn.DayNumber);
		}

		public LoanStatus GetStatus(DateOnly today) =>
			ReturnedOn is not null ? LoanStatus.Returned
			: IsOverdue(today) ? LoanStatus.Overdue
			: LoanStatus.Active;
	}
}