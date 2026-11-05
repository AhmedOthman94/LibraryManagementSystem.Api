using LibraryManagementSystemAPI.Common;
using LibraryManagementSystemAPI.Dtos;
using LibraryManagementSystemAPI.Filters;
using LibraryManagementSystemAPI.Services.BookCopies;
using Microsoft.AspNetCore.Http.HttpResults;

namespace LibraryManagementSystemAPI.Endpoints
{
	public static class BookCopiesEndpoints
	{
		public static void MapBookCopyEndpoints(this IEndpointRouteBuilder app)
		{
			// Nested route: copies always belong to a book
			var group = app
				.MapGroup("api/books/{bookId:guid}/copies")
				.WithTags("Book Copies");

			group.MapGet("/", GetAll)
				.ProducesProblem(StatusCodes.Status404NotFound);

			group.MapGet("/{copyId:guid}", GetById)
				.WithName("GetBookCopyById")
				.ProducesProblem(StatusCodes.Status404NotFound);

			group.MapPost("/", Create)
				.WithValidation<CreateBookCopyRequest>()
				.ProducesProblem(StatusCodes.Status404NotFound)
				.ProducesProblem(StatusCodes.Status409Conflict);

			group.MapPut("/{copyId:guid}", Update)
				.WithValidation<UpdateBookCopyRequest>()
				.ProducesProblem(StatusCodes.Status404NotFound)
				.ProducesProblem(StatusCodes.Status409Conflict);

			group.MapPut("/{copyId:guid}/status", ChangeStatus)
				.WithValidation<ChangeCopyStatusRequest>()
				.ProducesProblem(StatusCodes.Status404NotFound)
				.ProducesProblem(StatusCodes.Status409Conflict);

			group.MapDelete("/{copyId:guid}", Delete)
				.ProducesProblem(StatusCodes.Status404NotFound)
				.ProducesProblem(StatusCodes.Status409Conflict);
		}

		private static async Task<Results<Ok<List<BookCopyResponse>>, ProblemHttpResult>> GetAll(
			Guid bookId,
			IBookCopyService copyService,
			CancellationToken ct)
		{
			var result = await copyService.GetAllForBookAsync(bookId, ct);

			if (result.IsFailure)
			{
				return result.Error.ToProblem();
			}

			return TypedResults.Ok(result.Value);
		}

		private static async Task<Results<Ok<BookCopyResponse>, ProblemHttpResult>> GetById(
			Guid bookId,
			Guid copyId,
			IBookCopyService copyService,
			CancellationToken ct)
		{
			var result = await copyService.GetByIdAsync(bookId, copyId, ct);

			if (result.IsFailure)
			{
				return result.Error.ToProblem();
			}

			return TypedResults.Ok(result.Value);
		}

		private static async Task<Results<CreatedAtRoute<BookCopyResponse>, ProblemHttpResult>> Create(
			Guid bookId,
			CreateBookCopyRequest request,
			IBookCopyService copyService,
			CancellationToken ct)
		{
			var result = await copyService.CreateAsync(bookId, request, ct);

			if (result.IsFailure)
			{
				return result.Error.ToProblem();
			}

			return TypedResults.CreatedAtRoute(
				result.Value,
				"GetBookCopyById",
				new { bookId, copyId = result.Value.Id }
			);
		}

		private static async Task<Results<Ok<BookCopyResponse>, ProblemHttpResult>> Update(
			Guid bookId,
			Guid copyId,
			UpdateBookCopyRequest request,
			IBookCopyService copyService,
			CancellationToken ct)
		{
			var result = await copyService.UpdateAsync(bookId, copyId, request, ct);

			if (result.IsFailure)
			{
				return result.Error.ToProblem();
			}

			return TypedResults.Ok(result.Value);
		}

		private static async Task<Results<Ok<BookCopyResponse>, ProblemHttpResult>> ChangeStatus(
			Guid bookId,
			Guid copyId,
			ChangeCopyStatusRequest request,
			IBookCopyService copyService,
			CancellationToken ct)
		{
			var result = await copyService.ChangeStatusAsync(bookId, copyId, request, ct);

			if (result.IsFailure)
			{
				return result.Error.ToProblem();
			}

			return TypedResults.Ok(result.Value);
		}

		private static async Task<Results<NoContent, ProblemHttpResult>> Delete(
			Guid bookId,
			Guid copyId,
			IBookCopyService copyService,
			CancellationToken ct)
		{
			var result = await copyService.DeleteAsync(bookId, copyId, ct);

			if (result.IsFailure)
			{
				return result.Error.ToProblem();
			}

			return TypedResults.NoContent();
		}
	}
}