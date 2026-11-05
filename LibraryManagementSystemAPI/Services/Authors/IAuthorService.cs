using LibraryManagementSystemAPI.Common;
using LibraryManagementSystemAPI.Dtos;

namespace LibraryManagementSystemAPI.Services.Authors
{
	public interface IAuthorService
	{
		Task<List<AuthorResponse>> GetAllAsync(CancellationToken ct);
		Task<Result<AuthorResponse>> GetByIdAsync(Guid id, CancellationToken ct);
		Task<AuthorResponse> CreateAsync(CreateAuthorRequest request, CancellationToken ct);
		Task<Result<AuthorResponse>> UpdateAsync(Guid id, UpdateAuthorRequest request, CancellationToken ct);
		Task<Result> DeleteAsync(Guid id, CancellationToken ct);
	}
}