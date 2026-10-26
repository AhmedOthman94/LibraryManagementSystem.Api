using LibraryManagementSystemAPI.Dtos;
using LibraryManagementSystemAPI.Entities;

namespace LibraryManagementSystemAPI.Mappings
{
	public static class CategoryMappings
	{
		public static CategoryResponse ToResponse(this Category category) =>
			new(
				category.Id,
				category.Name,
				category.Description,
				category.CreatedAt,
				category.UpdatedAt
			);


		public static Category ToEntity(this CreateCategoryRequest request) =>
			new()
			{
				Name = request.Name.Trim(),
				Description = request.Description?.Trim()
			};

		public static void ApplyTo(this UpdateCategoryRequest request, Category category)
		{
			category.Name = request.Name.Trim();
			category.Description = request.Description?.Trim();
		}
	}
}