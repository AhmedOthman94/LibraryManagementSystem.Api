namespace LibraryManagementSystemAPI.Entities
{
	public class Author
	{
		public Guid Id { get; set; }
		public required string FirstName {  get; set; }
		public required string LastName { get; set; }
		public int? YearOfbirth { get; set; }
	}
}
