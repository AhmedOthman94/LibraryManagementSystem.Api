namespace LibraryManagementSystemAPI.Dtos
{
	public interface ICategoryRequest
	{
		public string Name { get; }
		public string? Description { get; }
	}

	public record CreateCategoryRequest(
		string Name,
		string? Description
	) : ICategoryRequest;

	public record UpdateCategoryRequest(
		string Name,
		string? Description
	) : ICategoryRequest;

	public record CategoryResponse(
		Guid Id,
		string Name,
		string? Description,
		DateTime CreatedAt,
		DateTime? UpdatedAt
	);
}