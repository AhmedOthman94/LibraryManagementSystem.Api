using LibraryManagementSystemAPI.Entities;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

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

			group.MapGet("/{id:Guid}", GetById)
				.WithName("GetCategoryById")
				.ProducesProblem(StatusCodes.Status404NotFound)
				.ProducesProblem(StatusCodes.Status200OK);
				

			group.MapGet("/", GetAll);
			//.Produces<IEnumerable<Category>>(StatusCodes.Status200OK);

			group.MapPost("/", Create)
				.ProducesProblem(StatusCodes.Status400BadRequest)
				.ProducesProblem(StatusCodes.Status409Conflict);

			group.MapPut("/{id:Guid}", Update)
				.ProducesProblem(StatusCodes.Status400BadRequest)
				.ProducesProblem(StatusCodes.Status404NotFound)
				.ProducesProblem(StatusCodes.Status409Conflict);

			group.MapDelete("/{id:Guid}", Delete)
				.ProducesProblem(StatusCodes.Status404NotFound)
				.ProducesProblem(StatusCodes.Status204NoContent);
				
		}

	
		private static async Task<Results<Ok<Category>, ProblemHttpResult>> GetById(Guid id)
		{
			var category = _categories.FirstOrDefault(c => c.Id == id);
			if (category is null)
			{
				return TypedResults.Problem(
					detail: $"Category with Id: {id} was not found.",
					statusCode: StatusCodes.Status404NotFound
				);
			}

			return TypedResults.Ok(category);
		}



		private static async Task<Ok<List<Category>>> GetAll()
		{
			var categories = _categories;

			return TypedResults.Ok(categories);
		}

		private static async Task<Results<CreatedAtRoute<Category>, ProblemHttpResult>> Create(
			[FromBody] Category category
		)
		{
			if(string.IsNullOrWhiteSpace(category.Name))
			{
				return TypedResults.Problem(
					detail: $"Invalid input, try again.",
					statusCode: StatusCodes.Status400BadRequest
				);
			}

			if (_categories.Any(exisitCat => exisitCat.Id == category.Id)) 
			{
				return TypedResults.Problem(
					detail: $"Category with Id: {category.Id} already exists.",
					statusCode: StatusCodes.Status409Conflict
				);
			}

			if (_categories.Any(exisitCat => exisitCat.Name == category.Name))
			{
				return TypedResults.Problem(
					detail: $"Category with name: {category.Name} already exists.",
					statusCode: StatusCodes.Status409Conflict
				);
			}

			_categories.Add(category);

			return TypedResults.CreatedAtRoute(
					category,
					"GetCategoryById",
					new { id = category.Id}
			);
		}

		private static async Task<Results<Ok<Category>, ProblemHttpResult>> Update(
			Guid id,
			[FromBody] Category updatedCategory 
		) 
		{
			if (string.IsNullOrWhiteSpace(updatedCategory.Name))
			{
				return TypedResults.Problem(
					detail: "Invalid input, try again",
					statusCode: StatusCodes.Status400BadRequest
				);
			}

			var existingCategory = _categories.FirstOrDefault(c => c.Id == id);

			if (existingCategory is null)
			{
				return TypedResults.Problem(
					detail: $"Category with Id: {id} was not found.",
					statusCode : StatusCodes.Status404NotFound
				);
			}

			if (_categories.Any(
				c => c.Id != id 
				&& c.Name == updatedCategory.Name))
			{
				return TypedResults.Problem(
					detail: $"Category with name: {updatedCategory.Name} already exists.",
					statusCode: StatusCodes.Status409Conflict
				);
			}

			existingCategory.Name = updatedCategory.Name;

			return TypedResults.Ok(existingCategory);
		}

		private static async Task<Results<NoContent, ProblemHttpResult>> Delete(
			Guid id
		)
		{
			var category = _categories.FirstOrDefault(
				c => c.Id == id
			);
			
			if (category is null) 
			{
				return TypedResults.Problem(
					detail: $"Category with Id: {id} was not found.",
					statusCode: StatusCodes.Status404NotFound
				);
			}

			_categories.Remove(category);

			return TypedResults.NoContent();
		}
	}
}
