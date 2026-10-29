namespace LibraryManagementSystemAPI.Entities
{
	public class Book : BaseEntity
	{
		public required string Title { get; set; }
		public string? Description { get; set; }
		public required string Isbn { get; set; }
		public string? Publisher { get; set; }
		public int? PublicationYear { get; set; }
		public int? PageCount { get; set; }
		public string? Language { get; set; }
		public string? CoverImageUrl { get; set; }

		// One-to-many: every book belongs to one category
		public Guid CategoryId { get; set; }
		public Category Category { get; set; } = null!;

		// Many-to-many: a book has one or more authors
		public ICollection<BookAuthor> BookAuthors { get; set; } = [];
	}
}