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
		public async Task<List<BookResponse>> GetAllAsync(CancellationToken ct)
		{
			var books = await BooksWithDetails()
				.AsNoTracking()
				.OrderBy(b => b.Title)
				.ToListAsync(ct);

			return books.Select(b => b.ToResponse()).ToList();
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
	}
}