using LibraryManagementSystemAPI.Common;
using LibraryManagementSystemAPI.DataAccess;
using LibraryManagementSystemAPI.Dtos;
using LibraryManagementSystemAPI.Entities;
using LibraryManagementSystemAPI.Filters;
using LibraryManagementSystemAPI.Mappings;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystemAPI.Endpoints
{
	public static class BooksEndpoints
	{
		public static void MapBookEndpoints(this IEndpointRouteBuilder app)
		{
			var group = app
				.MapGroup("api/books")
				.WithTags("Books");

			group.MapGet("/", GetAll);

			group.MapGet("/{id:guid}", GetById)
				.WithName("GetBookById")
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

		// One place that says "load a book with everything the response needs"
		private static IQueryable<Book> BooksWithDetails(LibraryDbContext db) =>
			db.Books
				.Include(b => b.Category)
				.Include(b => b.BookAuthors)
					.ThenInclude(ba => ba.Author);

		private static async Task<Ok<List<BookResponse>>> GetAll(
			LibraryDbContext db,
			CancellationToken ct)
		{
			var books = await BooksWithDetails(db)
				.AsNoTracking()
				.OrderBy(b => b.Title)
				.ToListAsync(ct);

			return TypedResults.Ok(books.Select(b => b.ToResponse()).ToList());
		}

		private static async Task<Results<Ok<BookResponse>, ProblemHttpResult>> GetById(
			Guid id,
			LibraryDbContext db,
			CancellationToken ct)
		{
			var book = await BooksWithDetails(db)
				.AsNoTracking()
				.FirstOrDefaultAsync(b => b.Id == id, ct);

			if (book is null)
			{
				return TypedResults.Problem(
					detail: $"Book with Id: {id} was not found.",
					statusCode: StatusCodes.Status404NotFound
				);
			}

			return TypedResults.Ok(book.ToResponse());
		}

		private static async Task<Results<CreatedAtRoute<BookResponse>, ProblemHttpResult>> Create(
			CreateBookRequest request,
			LibraryDbContext db,
			CancellationToken ct)
		{
			var isbn = IsbnHelper.Normalize(request.Isbn);

			if (await db.Books.AnyAsync(b => b.Isbn == isbn, ct))
			{
				return TypedResults.Problem(
					detail: $"A book with ISBN: {isbn} already exists.",
					statusCode: StatusCodes.Status409Conflict
				);
			}

			var referenceError = await CheckReferencesAsync(
				request.CategoryId, request.AuthorIds, db, ct);

			if (referenceError is not null)
			{
				return referenceError;
			}

			var book = request.ToEntity();

			db.Books.Add(book);
			await db.SaveChangesAsync(ct);

			// Reload with Category and Authors so the response has their names
			var created = await BooksWithDetails(db)
				.AsNoTracking()
				.FirstAsync(b => b.Id == book.Id, ct);

			return TypedResults.CreatedAtRoute(
				created.ToResponse(),
				"GetBookById",
				new { id = book.Id }
			);
		}

		private static async Task<Results<Ok<BookResponse>, ProblemHttpResult>> Update(
			Guid id,
			UpdateBookRequest request,
			LibraryDbContext db,
			CancellationToken ct)
		{
			var book = await db.Books
				.Include(b => b.BookAuthors)
				.FirstOrDefaultAsync(b => b.Id == id, ct);

			if (book is null)
			{
				return TypedResults.Problem(
					detail: $"Book with Id: {id} was not found.",
					statusCode: StatusCodes.Status404NotFound
				);
			}

			var isbn = IsbnHelper.Normalize(request.Isbn);

			if (await db.Books.AnyAsync(b => b.Id != id && b.Isbn == isbn, ct))
			{
				return TypedResults.Problem(
					detail: $"A book with ISBN: {isbn} already exists.",
					statusCode: StatusCodes.Status409Conflict
				);
			}

			var referenceError = await CheckReferencesAsync(
				request.CategoryId, request.AuthorIds, db, ct);

			if (referenceError is not null)
			{
				return referenceError;
			}

			request.ApplyTo(book);
			await db.SaveChangesAsync(ct);

			var updated = await BooksWithDetails(db)
				.AsNoTracking()
				.FirstAsync(b => b.Id == id, ct);

			return TypedResults.Ok(updated.ToResponse());
		}

		private static async Task<Results<NoContent, ProblemHttpResult>> Delete(
			Guid id,
			LibraryDbContext db,
			CancellationToken ct)
		{
			var book = await db.Books
				.FirstOrDefaultAsync(b => b.Id == id, ct);

			if (book is null)
			{
				return TypedResults.Problem(
					detail: $"Book with Id: {id} was not found.",
					statusCode: StatusCodes.Status404NotFound
				);
			}

			db.Books.Remove(book); // becomes a soft delete
			await db.SaveChangesAsync(ct);

			return TypedResults.NoContent();
		}

		// Checks that the category and all authors really exist.
		// Returns a problem, or null if everything is OK.
		private static async Task<ProblemHttpResult?> CheckReferencesAsync(
			Guid categoryId,
			List<Guid> authorIds,
			LibraryDbContext db,
			CancellationToken ct)
		{
			var categoryExists = await db.Categories
				.AnyAsync(c => c.Id == categoryId, ct);

			if (!categoryExists)
			{
				return TypedResults.Problem(
					detail: $"Category with Id: {categoryId} does not exist.",
					statusCode: StatusCodes.Status400BadRequest
				);
			}

			var foundAuthorIds = await db.Authors
				.Where(a => authorIds.Contains(a.Id))
				.Select(a => a.Id)
				.ToListAsync(ct);

			var missingAuthorIds = authorIds.Except(foundAuthorIds).ToList();

			if (missingAuthorIds.Count > 0)
			{
				return TypedResults.Problem(
					detail: $"These authors do not exist: {string.Join(", ", missingAuthorIds)}",
					statusCode: StatusCodes.Status400BadRequest
				);
			}

			return null;
		}
	}
}