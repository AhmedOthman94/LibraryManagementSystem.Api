using LibraryManagementSystemAPI.Auth;
using LibraryManagementSystemAPI.Common;
using LibraryManagementSystemAPI.Dtos;
using LibraryManagementSystemAPI.Filters;
using LibraryManagementSystemAPI.Services.Books;
using Microsoft.AspNetCore.Http.HttpResults;

namespace LibraryManagementSystemAPI.Endpoints
{
	public static class BooksEndpoints
	{
		public static void MapBookEndpoints(this IEndpointRouteBuilder app)
		{
			var group = app
				.MapGroup("api/books")
				.WithTags("Books")
				.RequireAuthorization(AuthPolicies.Staff);

			group.MapGet("/", GetAll)
				.AllowAnonymous()
				.WithValidation<BookQueryParameters>();

			group.MapGet("/{id:guid}", GetById)
				.WithName("GetBookById")
				.AllowAnonymous()
				.ProducesProblem(StatusCodes.Status404NotFound);

			group.MapPost("/", Create)
				.WithValidation<CreateBookRequest>()
				.ProducesProblem(StatusCodes.Status400BadRequest)
				.ProducesProblem(StatusCodes.Status409Conflict);

			group.MapPut("/{id:guid}", Update)
				.WithValidation<UpdateBookRequest>()
				.ProducesProblem(StatusCodes.Status400BadRequest)
				.ProducesProblem(StatusCodes.Status404NotFound)
				.ProducesProblem(StatusCodes.Status409Conflict);

			group.MapDelete("/{id:guid}", Delete)
				.ProducesProblem(StatusCodes.Status404NotFound);
		}

		private static async Task<Ok<PagedResponse<BookResponse>>> GetAll(
			[AsParameters] BookQueryParameters parameters,
			IBookService bookService,
			CancellationToken ct)
		{
			var books = await bookService.GetAllAsync(parameters, ct);
			return TypedResults.Ok(books);
		}

		private static async Task<Results<Ok<BookResponse>, ProblemHttpResult>> GetById(
			Guid id,
			IBookService bookService,
			CancellationToken ct)
		{
			var result = await bookService.GetByIdAsync(id, ct);

			if (result.IsFailure)
			{
				return result.Error.ToProblem();
			}

			return TypedResults.Ok(result.Value);
		}

		private static async Task<Results<CreatedAtRoute<BookResponse>, ProblemHttpResult>> Create(
			CreateBookRequest request,
			IBookService bookService,
			CancellationToken ct)
		{
			var result = await bookService.CreateAsync(request, ct);

			if (result.IsFailure)
			{
				return result.Error.ToProblem();
			}

			return TypedResults.CreatedAtRoute(
				result.Value,
				"GetBookById",
				new { id = result.Value.Id }
			);
		}

		private static async Task<Results<Ok<BookResponse>, ProblemHttpResult>> Update(
			Guid id,
			UpdateBookRequest request,
			IBookService bookService,
			CancellationToken ct)
		{
			var result = await bookService.UpdateAsync(id, request, ct);

			if (result.IsFailure)
			{
				return result.Error.ToProblem();
			}

			return TypedResults.Ok(result.Value);
		}

		private static async Task<Results<NoContent, ProblemHttpResult>> Delete(
			Guid id,
			IBookService bookService,
			CancellationToken ct)
		{
			var result = await bookService.DeleteAsync(id, ct);

			if (result.IsFailure)
			{
				return result.Error.ToProblem();
			}

			return TypedResults.NoContent();
		}
	}
}