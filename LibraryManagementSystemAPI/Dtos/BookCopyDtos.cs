using LibraryManagementSystemAPI.Entities;

namespace LibraryManagementSystemAPI.Dtos
{
	public interface IBookCopyRequest
	{
		string Barcode { get; }
		string? ShelfLocation { get; }
		DateOnly? AcquiredOn { get; }
	}

	public record CreateBookCopyRequest(
		string Barcode,
		string? ShelfLocation,
		DateOnly? AcquiredOn
	) : IBookCopyRequest;

	// Details only. Status has its own request below.
	public record UpdateBookCopyRequest(
		string Barcode,
		string? ShelfLocation,
		DateOnly? AcquiredOn
	) : IBookCopyRequest;

	public record ChangeCopyStatusRequest(CopyStatus Status);

	public record BookCopyResponse(
		Guid Id,
		Guid BookId,
		string Barcode,
		CopyStatus Status,
		string? ShelfLocation,
		DateOnly? AcquiredOn,
		DateTime CreatedAt,
		DateTime? UpdatedAt
	);
}