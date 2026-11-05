namespace LibraryManagementSystemAPI.Entities
{
	// One "voucher". Used exactly once, then replaced (rotation).
	public class RefreshToken
	{
		public Guid Id { get; set; } = Guid.CreateVersion7();

		public Guid UserId { get; set; }
		public User User { get; set; } = null!;

		// SHA-256 of the real token. The real token is never stored.
		public required string TokenHash { get; set; }

		// All tokens from one login share a FamilyId (one "session")
		public Guid FamilyId { get; set; }

		public DateTime CreatedAt { get; set; }
		public DateTime ExpiresAt { get; set; }

		public DateTime? RevokedAt { get; set; }
		public string? RevokedReason { get; set; }

		// Set when this token was used normally and exchanged for a new one
		public Guid? ReplacedByTokenId { get; set; }

		// Concurrency token (PostgreSQL xmin), like BookCopy
		public uint Version { get; set; }
	}
}