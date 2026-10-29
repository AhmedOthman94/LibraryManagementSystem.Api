using LibraryManagementSystemAPI.DataAccess;
using LibraryManagementSystemAPI.Dtos;
using LibraryManagementSystemAPI.Filters;
using LibraryManagementSystemAPI.Mappings;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystemAPI.Endpoints
{
	public static class CategoriesEndpoints
	{
		public static void MapCategoryEndpoints(this IEndpointRouteBuilder app)
		{
			var group = app
				.MapGroup("api/categories")
				.WithTags("Categories");

			group.MapGet("/", GetAll);

			group.MapGet("/{id:guid}", GetById)
				.WithName("GetCategoryById")
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
			LibraryDbContext db,
			CancellationToken ct)
		{
			var categories = await db.Categories
				.AsNoTracking()
				.OrderBy(c => c.Name)
				.Select(c => c.ToResponse())
				.ToListAsync(ct);

			return TypedResults.Ok(categories);
		}

		private static async Task<Results<Ok<CategoryResponse>, ProblemHttpResult>> GetById(
			Guid id,
			LibraryDbContext db,
			CancellationToken ct)
		{
			var category = await db.Categories
				.AsNoTracking()
				.FirstOrDefaultAsync(c => c.Id == id, ct);

			if (category is null)
			{
				return TypedResults.Problem(
					detail: $"Category with Id: {id} was not found.",
					statusCode: StatusCodes.Status404NotFound
				);
			}

			return TypedResults.Ok(category.ToResponse());
		}

		private static async Task<Results<CreatedAtRoute<CategoryResponse>, ProblemHttpResult>> Create(
			CreateCategoryRequest request,
			LibraryDbContext db,
			CancellationToken ct)
		{
			var name = request.Name.Trim().ToLower();
			var nameExists = await db.Categories
				.AnyAsync(c => c.Name.ToLower() == name, ct);

			if (nameExists)
			{
				return TypedResults.Problem(
					detail: $"Category with name: {request.Name.Trim()} already exists.",
					statusCode: StatusCodes.Status409Conflict
				);
			}

			var category = request.ToEntity();

			db.Categories.Add(category);
			await db.SaveChangesAsync(ct);

			return TypedResults.CreatedAtRoute(
				category.ToResponse(),
				"GetCategoryById",
				new { id = category.Id }
			);
		}

		private static async Task<Results<Ok<CategoryResponse>, ProblemHttpResult>> Update(
			Guid id,
			UpdateCategoryRequest request,
			LibraryDbContext db,
			CancellationToken ct)
		{
			var category = await db.Categories
				.FirstOrDefaultAsync(c => c.Id == id, ct);

			if (category is null)
			{
				return TypedResults.Problem(
					detail: $"Category with Id: {id} was not found.",
					statusCode: StatusCodes.Status404NotFound
				);
			}

			var name = request.Name.Trim().ToLower();
			var nameTaken = await db.Categories
				.AnyAsync(c => c.Id != id && c.Name.ToLower() == name, ct);

			if (nameTaken)
			{
				return TypedResults.Problem(
					detail: $"Category with name: {request.Name.Trim()} already exists.",
					statusCode: StatusCodes.Status409Conflict
				);
			}

			request.ApplyTo(category);
			await db.SaveChangesAsync(ct);

			return TypedResults.Ok(category.ToResponse());
		}

		private static async Task<Results<NoContent, ProblemHttpResult>> Delete(
			Guid id,
			LibraryDbContext db,
			CancellationToken ct)
		{
			var category = await db.Categories
				.FirstOrDefaultAsync(c => c.Id == id, ct);

			if (category is null)
			{
				return TypedResults.Problem(
					detail: $"Category with Id: {id} was not found.",
					statusCode: StatusCodes.Status404NotFound
				);
			}

			var hasBooks = await db.Books.AnyAsync(b => b.CategoryId == id, ct);

			if (hasBooks)
			{
				return TypedResults.Problem(
					detail: "This category still has books. Move or delete them first.",
					statusCode: StatusCodes.Status409Conflict
				);
			}

			db.Categories.Remove(category);
			await db.SaveChangesAsync(ct);

			return TypedResults.NoContent();
		}
	}
}