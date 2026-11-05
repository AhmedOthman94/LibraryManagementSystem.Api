using LibraryManagementSystemAPI.Common;
using LibraryManagementSystemAPI.Dtos;
using LibraryManagementSystemAPI.Entities;

namespace LibraryManagementSystemAPI.Mappings
{
	public static class BookCopyMappings
	{
		public static BookCopyResponse ToResponse(this BookCopy copy) =>
			new(
				copy.Id,
				copy.BookId,
				copy.Barcode,
				copy.Status,
				copy.ShelfLocation,
				copy.AcquiredOn,
				copy.CreatedAt,
				copy.UpdatedAt
			);

		// bookId comes from the URL, not from the body
		public static BookCopy ToEntity(this CreateBookCopyRequest request, Guid bookId) =>
			new()
			{
				BookId = bookId,
				Barcode = BarcodeHelper.Normalize(request.Barcode),
				ShelfLocation = request.ShelfLocation?.Trim(),
				AcquiredOn = request.AcquiredOn
			};

		public static void ApplyTo(this UpdateBookCopyRequest request, BookCopy copy)
		{
			copy.Barcode = BarcodeHelper.Normalize(request.Barcode);
			copy.ShelfLocation = request.ShelfLocation?.Trim();
			copy.AcquiredOn = request.AcquiredOn;
		}
	}
}