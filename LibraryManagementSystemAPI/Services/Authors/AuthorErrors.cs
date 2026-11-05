using LibraryManagementSystemAPI.Common;

namespace LibraryManagementSystemAPI.Services.Authors
{
	public static class AuthorErrors
	{
		public static Error NotFound(Guid id) =>
			Error.NotFound($"Author with Id: {id} was not found.");

		public static Error HasBooks =>
			Error.Conflict("This author still has books. Remove the author from them first.");
	}
}