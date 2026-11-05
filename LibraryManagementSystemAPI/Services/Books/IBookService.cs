using LibraryManagementSystemAPI.Common;
using LibraryManagementSystemAPI.Dtos;

namespace LibraryManagementSystemAPI.Services.Books
{
	public interface IBookService
	{
		Task<PagedResponse<BookResponse>> GetAllAsync(BookQueryParameters parameters, CancellationToken ct); Task<Result<BookResponse>> GetByIdAsync(Guid id, CancellationToken ct);
		Task<Result<BookResponse>> CreateAsync(CreateBookRequest request, CancellationToken ct);
		Task<Result<BookResponse>> UpdateAsync(Guid id, UpdateBookRequest request, CancellationToken ct);
		Task<Result> DeleteAsync(Guid id, CancellationToken ct);
	}
}