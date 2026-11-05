using LibraryManagementSystemAPI.Common;
using LibraryManagementSystemAPI.Dtos;

namespace LibraryManagementSystemAPI.Services.Categories
{
	public interface ICategoryService
	{
		Task<List<CategoryResponse>> GetAllAsync(CancellationToken ct);
		Task<Result<CategoryResponse>> GetByIdAsync(Guid id, CancellationToken ct);
		Task<Result<CategoryResponse>> CreateAsync(CreateCategoryRequest request, CancellationToken ct);
		Task<Result<CategoryResponse>> UpdateAsync(Guid id, UpdateCategoryRequest request, CancellationToken ct);
		Task<Result> DeleteAsync(Guid id, CancellationToken ct);
	}
}