using LibraryManagementSystemAPI.Entities;

namespace LibraryManagementSystemAPI.Endpoints
{
	public static class CategoriesEndpoints
	{
		private static List<Category> _categories =
		[
			new Category{Id = Guid.NewGuid(), Name = "Biography"},
			new Category{Id = Guid.NewGuid(), Name = "Novel"}
		];

		public static void MapCategoryEndpoints(
			this IEndpointRouteBuilder app
		)
		{
			var group = app
				.MapGroup("api/categories")
				.WithTags("Categories");

			group.MapGet("/", GetAll);
		}

		
		private static async Task<IResult> GetAll()
		{
			var categories = _categories;

			return Results.Ok(categories);
		}
	}
}
