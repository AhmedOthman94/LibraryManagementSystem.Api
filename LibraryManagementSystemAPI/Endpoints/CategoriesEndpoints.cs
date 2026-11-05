using LibraryManagementSystemAPI.Auth;
using LibraryManagementSystemAPI.Common;
using LibraryManagementSystemAPI.Dtos;
using LibraryManagementSystemAPI.Filters;
using LibraryManagementSystemAPI.Services.Categories;
using Microsoft.AspNetCore.Http.HttpResults;

namespace LibraryManagementSystemAPI.Endpoints
{
	public static class CategoriesEndpoints
	{
		public static void MapCategoryEndpoints(this IEndpointRouteBuilder app)
		{
			var group = app
				.MapGroup("api/categories")
				.WithTags("Categories")
				.RequireAuthorization(AuthPolicies.Staff);   // writing = staff only

			group.MapGet("/", GetAll)
				.AllowAnonymous();                           // the catalog is public

			group.MapGet("/{id:guid}", GetById)
				.WithName("GetCategoryById")
				.AllowAnonymous()
				.ProducesProblem(StatusCodes.Status404NotFound);

			group.MapPost("/", Create)
				.WithValidation<CreateCategoryRequest>()
				.ProducesProblem(StatusCodes.Status409Conflict);

			group.MapPut("/{id:guid}", Update)
				.WithValidation<UpdateCategoryRequest>()
				.ProducesProblem(StatusCodes.Status404NotFound)
				.ProducesProblem(StatusCodes.Status409Conflict);

			group.MapDelete("/{id:guid}", Delete)
				.ProducesProblem(StatusCodes.Status404NotFound)
				.ProducesProblem(StatusCodes.Status409Conflict);
		}

		private static async Task<Ok<List<CategoryResponse>>> GetAll(
			ICategoryService categoryService,
			CancellationToken ct)
		{
			var categories = await categoryService.GetAllAsync(ct);
			return TypedResults.Ok(categories);
		}

		private static async Task<Results<Ok<CategoryResponse>, ProblemHttpResult>> GetById(
			Guid id,
			ICategoryService categoryService,
			CancellationToken ct)
		{
			var result = await categoryService.GetByIdAsync(id, ct);

			if (result.IsFailure)
			{
				return result.Error.ToProblem();
			}

			return TypedResults.Ok(result.Value);
		}

		private static async Task<Results<CreatedAtRoute<CategoryResponse>, ProblemHttpResult>> Create(
			CreateCategoryRequest request,
			ICategoryService categoryService,
			CancellationToken ct)
		{
			var result = await categoryService.CreateAsync(request, ct);

			if (result.IsFailure)
			{
				return result.Error.ToProblem();
			}

			return TypedResults.CreatedAtRoute(
				result.Value,
				"GetCategoryById",
				new { id = result.Value.Id }
			);
		}

		private static async Task<Results<Ok<CategoryResponse>, ProblemHttpResult>> Update(
			Guid id,
			UpdateCategoryRequest request,
			ICategoryService categoryService,
			CancellationToken ct)
		{
			var result = await categoryService.UpdateAsync(id, request, ct);

			if (result.IsFailure)
			{
				return result.Error.ToProblem();
			}

			return TypedResults.Ok(result.Value);
		}

		private static async Task<Results<NoContent, ProblemHttpResult>> Delete(
			Guid id,
			ICategoryService categoryService,
			CancellationToken ct)
		{
			var result = await categoryService.DeleteAsync(id, ct);

			if (result.IsFailure)
			{
				return result.Error.ToProblem();
			}

			return TypedResults.NoContent();
		}
	}
}