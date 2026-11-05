namespace LibraryManagementSystemAPI.Entities
{
	// A login account. Different from Member (a library customer).
	public class User : BaseEntity
	{
		// Stored trimmed and lowercase
		public required string Email { get; set; }

		// NEVER the password itself: a salted, slow, one-way hash
		public required string PasswordHash { get; set; }

		public UserRole Role { get; set; }

		// Only for role Member: which library member this account belongs to
		public Guid? MemberId { get; set; }
		public Member? Member { get; set; }
	}
}