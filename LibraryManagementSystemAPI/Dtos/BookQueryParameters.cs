namespace LibraryManagementSystemAPI.Dtos
{
	public class BookQueryParameters
	{
		public static readonly string[] AllowedSortFields = ["title", "publicationYear", "createdAt"];
		public static readonly string[] AllowedSortDirections = ["asc", "desc"];

		public string? Search { get; set; }

		public Guid? CategoryId { get; set; }
		public Guid? AuthorId { get; set; }
		public string? Language { get; set; }
		public int? PublishedFrom { get; set; }
		public int? PublishedTo { get; set; }
		public bool? AvailableOnly { get; set; }
		public string? SortBy { get; set; }
		public string? SortDirection { get; set; }

		public int? Page { get; set; }
		public int? PageSize { get; set; }
	}
}