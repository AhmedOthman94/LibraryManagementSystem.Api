using LibraryManagementSystemAPI.Common;

namespace LibraryManagementSystemAPI.Services.Books
{
	public static class BookErrors
	{
		public static Error NotFound(Guid id) =>
			Error.NotFound($"Book with Id: {id} was not found.");

		public static Error IsbnAlreadyExists(string isbn) =>
			Error.Conflict($"A book with ISBN: {isbn} already exists.");

		public static Error CategoryDoesNotExist(Guid categoryId) =>
			Error.Validation($"Category with Id: {categoryId} does not exist.");

		public static Error AuthorsDoNotExist(IEnumerable<Guid> authorIds) =>
			Error.Validation($"These authors do not exist: {string.Join(", ", authorIds)}");

		public static Error HasCopies =>
			Error.Conflict("This book still has copies. Delete its copies first.");
	}
}