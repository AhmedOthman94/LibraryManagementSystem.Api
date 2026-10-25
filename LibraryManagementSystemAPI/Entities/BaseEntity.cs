namespace LibraryManagementSystemAPI.Entities
{
	public abstract class BaseEntity
	{
		public Guid Id { get; set; } = Guid.CreateVersion7();

		public DateTime CreatedAt { get; set; }
		public DateTime? UpdatedAt { get; set; }

		public bool IsDeleted { get; set; }
		public DateTime? DeletedAt { get; set; }
	}
}
