namespace LibraryManagementSystemAPI.Entities
{
	public class BookCopy : BaseEntity
	{
		public Guid BookId { get; set; }
		public Book Book { get; set; } = null!;

		public required string Barcode { get; set; }

		public CopyStatus Status { get; set; } = CopyStatus.Available;

		public string? ShelfLocation { get; set; }
		public DateOnly? AcquiredOn { get; set; }
	}
}