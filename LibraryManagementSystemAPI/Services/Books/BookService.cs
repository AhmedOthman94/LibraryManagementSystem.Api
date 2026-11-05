using LibraryManagementSystemAPI.Common;
using LibraryManagementSystemAPI.DataAccess;
using LibraryManagementSystemAPI.Dtos;
using LibraryManagementSystemAPI.Entities;
using LibraryManagementSystemAPI.Mappings;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystemAPI.Services.Books
{
	public class BookService(LibraryDbContext db) : IBookService
	{
		public async Task<PagedResponse<BookResponse>> GetAllAsync(
			BookQueryParameters parameters,
			CancellationToken ct)
		{
			// Build the plan step by step (no SQL runs yet)
			var query = BooksWithDetails().AsNoTracking();

			query = ApplyFilters(query, parameters);
			query = ApplySorting(query, parameters);

			var page = parameters.Page ?? PaginationDefaults.Page;
			var pageSize = parameters.PageSize ?? PaginationDefaults.PageSize;

			// SQL runs here: one COUNT and one page query
			return await query.ToPagedResponseAsync(page, pageSize, b => b.ToResponse(), ct);
		}

		public async Task<Result<BookResponse>> GetByIdAsync(Guid id, CancellationToken ct)
		{
			var book = await BooksWithDetails()
				.AsNoTracking()
				.FirstOrDefaultAsync(b => b.Id == id, ct);

			if (book is null)
			{
				return BookErrors.NotFound(id);
			}

			return book.ToResponse();
		}

		public async Task<Result<BookResponse>> CreateAsync(
			CreateBookRequest request,
			CancellationToken ct)
		{
			// 1) Is the ISBN free?
			var isbn = IsbnHelper.Normalize(request.Isbn);

			if (await IsbnExistsAsync(isbn, excludeId: null, ct))
			{
				return BookErrors.IsbnAlreadyExists(isbn);
			}

			// 2) Do the category and authors exist?
			var referenceError = await CheckReferencesAsync(request.CategoryId, request.AuthorIds, ct);

			if (referenceError is not null)
			{
				return referenceError;
			}

			// 3) Everything is OK: save
			var book = request.ToEntity();

			db.Books.Add(book);
			await db.SaveChangesAsync(ct);

			return await GetResponseAsync(book.Id, ct);
		}

		public async Task<Result<BookResponse>> UpdateAsync(
			Guid id,
			UpdateBookRequest request,
			CancellationToken ct)
		{
			// 1) Does the book exist? (most basic problem first)
			var book = await db.Books
				.Include(b => b.BookAuthors)
				.FirstOrDefaultAsync(b => b.Id == id, ct);

			if (book is null)
			{
				return BookErrors.NotFound(id);
			}

			// 2) Is the ISBN free? (ignoring this book itself)
			var isbn = IsbnHelper.Normalize(request.Isbn);

			if (await IsbnExistsAsync(isbn, excludeId: id, ct))
			{
				return BookErrors.IsbnAlreadyExists(isbn);
			}

			// 3) Do the category and authors exist?
			var referenceError = await CheckReferencesAsync(request.CategoryId, request.AuthorIds, ct);

			if (referenceError is not null)
			{
				return referenceError;
			}

			// 4) Everything is OK: apply and save
			request.ApplyTo(book);
			await db.SaveChangesAsync(ct);

			return await GetResponseAsync(id, ct);
		}

		public async Task<Result> DeleteAsync(Guid id, CancellationToken ct)
		{
			var book = await db.Books
				.FirstOrDefaultAsync(b => b.Id == id, ct);

			if (book is null)
			{
				return BookErrors.NotFound(id);
			}

			db.Books.Remove(book); // becomes a soft delete
			await db.SaveChangesAsync(ct);

			return Result.Success();
		}


		// "A book with everything the response needs"
		private IQueryable<Book> BooksWithDetails() =>
			db.Books
				.Include(b => b.Category)
				.Include(b => b.BookAuthors)
					.ThenInclude(ba => ba.Author);

		// Reload a saved book with its category and authors, as a response
		private async Task<BookResponse> GetResponseAsync(Guid id, CancellationToken ct)
		{
			var book = await BooksWithDetails()
				.AsNoTracking()
				.FirstAsync(b => b.Id == id, ct);

			return book.ToResponse();
		}

		// Create: excludeId = null (check all books)
		// Update: excludeId = the book's own id (don't compare it with itself)
		private Task<bool> IsbnExistsAsync(string normalizedIsbn, Guid? excludeId, CancellationToken ct) =>
			db.Books.AnyAsync(
				b => b.Id != excludeId && b.Isbn == normalizedIsbn,
				ct);

		// Returns an Error if something is missing, or null if all is OK
		private async Task<Error?> CheckReferencesAsync(
			Guid categoryId,
			List<Guid> authorIds,
			CancellationToken ct)
		{
			var categoryExists = await db.Categories
				.AnyAsync(c => c.Id == categoryId, ct);

			if (!categoryExists)
			{
				return BookErrors.CategoryDoesNotExist(categoryId);
			}

			var foundAuthorIds = await db.Authors
				.Where(a => authorIds.Contains(a.Id))
				.Select(a => a.Id)
				.ToListAsync(ct);

			var missingAuthorIds = authorIds.Except(foundAuthorIds).ToList();

			if (missingAuthorIds.Count > 0)
			{
				return BookErrors.AuthorsDoNotExist(missingAuthorIds);
			}

			return null;
		}

		private static IQueryable<Book> ApplyFilters(
	IQueryable<Book> query,
	BookQueryParameters parameters)
		{
			if (!string.IsNullOrWhiteSpace(parameters.Search))
			{
				var pattern = LikePattern.Contains(parameters.Search);
				var isbn = IsbnHelper.Normalize(parameters.Search);

				query = query.Where(b =>
					EF.Functions.ILike(b.Title, pattern)
					|| b.Isbn == isbn
					|| b.BookAuthors.Any(ba =>
						EF.Functions.ILike(ba.Author.FirstName + " " + ba.Author.LastName, pattern)));
			}

			if (parameters.CategoryId.HasValue)
			{
				query = query.Where(b => b.CategoryId == parameters.CategoryId.Value);
			}

			if (parameters.AuthorId.HasValue)
			{
				query = query.Where(b => b.BookAuthors.Any(ba => ba.AuthorId == parameters.AuthorId.Value));
			}

			if (!string.IsNullOrWhiteSpace(parameters.Language))
			{
				var language = parameters.Language.Trim().ToLower();
				query = query.Where(b => b.Language != null && b.Language.ToLower() == language);
			}

			if (parameters.PublishedFrom.HasValue)
			{
				query = query.Where(b => b.PublicationYear >= parameters.PublishedFrom.Value);
			}

			if (parameters.PublishedTo.HasValue)
			{
				query = query.Where(b => b.PublicationYear <= parameters.PublishedTo.Value);
			}

			return query;
		}

		private static IQueryable<Book> ApplySorting(
			IQueryable<Book> query,
			BookQueryParameters parameters)
		{
			var descending = string.Equals(
				parameters.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);

			IOrderedQueryable<Book> ordered = (parameters.SortBy?.ToLowerInvariant(), descending) switch
			{
				("publicationyear", false) => query.OrderBy(b => b.PublicationYear),
				("publicationyear", true) => query.OrderByDescending(b => b.PublicationYear),
				("createdat", false) => query.OrderBy(b => b.CreatedAt),
				("createdat", true) => query.OrderByDescending(b => b.CreatedAt),
				(_, false) => query.OrderBy(b => b.Title),          // default: title A → Z
				(_, true) => query.OrderByDescending(b => b.Title)
			};

			// Tie-breaker: same order every time, so pages never repeat or skip books
			return ordered.ThenBy(b => b.Id);
		}
	}
}