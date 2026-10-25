using System.ComponentModel.DataAnnotations;

namespace LibraryManagementSystemAPI.Entities
{
	public class Category : BaseEntity
	{
		public required string Name { get; set; } 
		public string? Description { get; set; }
	}
}
