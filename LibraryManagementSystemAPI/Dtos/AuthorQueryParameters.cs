namespace LibraryManagementSystemAPI.Dtos
{
	public class AuthorQueryParameters
	{
		public string? Search { get; set; }

		public string? Nationality { get; set; }

		public int? Page { get; set; }
		public int? PageSize { get; set; }
	}
}