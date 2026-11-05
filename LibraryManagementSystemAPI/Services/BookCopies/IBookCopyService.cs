using LibraryManagementSystemAPI.Common;
using LibraryManagementSystemAPI.Dtos;

namespace LibraryManagementSystemAPI.Services.BookCopies
{
	public interface IBookCopyService
	{
		Task<Result<List<BookCopyResponse>>> GetAllForBookAsync(Guid bookId, CancellationToken ct);
		Task<Result<BookCopyResponse>> GetByIdAsync(Guid bookId, Guid copyId, CancellationToken ct);
		Task<Result<BookCopyResponse>> CreateAsync(Guid bookId, CreateBookCopyRequest request, CancellationToken ct);
		Task<Result<BookCopyResponse>> UpdateAsync(Guid bookId, Guid copyId, UpdateBookCopyRequest request, CancellationToken ct);
		Task<Result<BookCopyResponse>> ChangeStatusAsync(Guid bookId, Guid copyId, ChangeCopyStatusRequest request, CancellationToken ct);
		Task<Result> DeleteAsync(Guid bookId, Guid copyId, CancellationToken ct);
	}
}