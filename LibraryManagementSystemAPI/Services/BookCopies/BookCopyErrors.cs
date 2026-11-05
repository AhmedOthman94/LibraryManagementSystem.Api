using LibraryManagementSystemAPI.Common;

namespace LibraryManagementSystemAPI.Services.BookCopies
{
	public static class BookCopyErrors
	{
		public static Error NotFound(Guid copyId) =>
			Error.NotFound($"Copy with Id: {copyId} was not found for this book.");

		public static Error BarcodeAlreadyExists(string barcode) =>
			Error.Conflict($"A copy with barcode: {barcode} already exists.");

		public static Error CopyIsOnLoan =>
			Error.Conflict("This copy is currently on loan. It must be returned first.");
	}
}