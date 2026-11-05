using LibraryManagementSystemAPI.Common;
using LibraryManagementSystemAPI.Dtos;

namespace LibraryManagementSystemAPI.Services.Authors
{
	public interface IAuthorService
	{
		Task<PagedResponse<AuthorResponse>> GetAllAsync(AuthorQueryParameters parameters, CancellationToken ct); Task<Result<AuthorResponse>> GetByIdAsync(Guid id, CancellationToken ct);
		Task<AuthorResponse> CreateAsync(CreateAuthorRequest request, CancellationToken ct);
		Task<Result<AuthorResponse>> UpdateAsync(Guid id, UpdateAuthorRequest request, CancellationToken ct);
		Task<Result> DeleteAsync(Guid id, CancellationToken ct);
	}
}