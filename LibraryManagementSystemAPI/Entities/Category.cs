using System.ComponentModel.DataAnnotations;

namespace LibraryManagementSystemAPI.Entities
{
	public class Category
	{
		public Guid Id { get; set; }
		[Required]
		public required string Name { get; set; } 
	}
}
