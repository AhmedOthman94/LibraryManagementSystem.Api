namespace LibraryManagementSystemAPI.Entities
{
	public class Author : BaseEntity
	{
		public required string FirstName {  get; set; }
		public required string LastName { get; set; }

		public string? Biography { get; set; }
		public string? Nationality { get; set; }

		public DateOnly? DateOfBirth { get; set; }
		public DateOnly? DateOfDeath { get; set; }

		public string? ImageUrl { get; set; }
	}
}
