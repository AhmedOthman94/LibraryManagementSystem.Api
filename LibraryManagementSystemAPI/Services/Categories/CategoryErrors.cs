using LibraryManagementSystemAPI.Common;

namespace LibraryManagementSystemAPI.Services.Categories
{
	public static class CategoryErrors
	{
		public static Error NotFound(Guid id) =>
			Error.NotFound($"Category with Id: {id} was not found.");

		public static Error NameAlreadyExists(string name) =>
			Error.Conflict($"Category with name: {name} already exists.");

		public static Error HasBooks =>
			Error.Conflict("This category still has books. Move or delete them first.");
	}
}