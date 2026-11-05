using LibraryManagementSystemAPI.Common;
using LibraryManagementSystemAPI.DataAccess;
using LibraryManagementSystemAPI.Dtos;
using LibraryManagementSystemAPI.Entities;
using LibraryManagementSystemAPI.Mappings;
using LibraryManagementSystemAPI.Services.Books;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystemAPI.Services.BookCopies
{
	public class BookCopyService(LibraryDbContext db) : IBookCopyService
	{
		public async Task<Result<List<BookCopyResponse>>> GetAllForBookAsync(
			Guid bookId,
			CancellationToken ct)
		{
			if (!await BookExistsAsync(bookId, ct))
			{
				return BookErrors.NotFound(bookId);
			}

			var copies = await db.BookCopies
				.AsNoTracking()
				.Where(c => c.BookId == bookId)
				.OrderBy(c => c.Barcode)
				.Select(c => c.ToResponse())
				.ToListAsync(ct);

			return copies;
		}

		public async Task<Result<BookCopyResponse>> GetByIdAsync(
			Guid bookId,
			Guid copyId,
			CancellationToken ct)
		{
			var copy = await db.BookCopies
				.AsNoTracking()
				.FirstOrDefaultAsync(c => c.Id == copyId && c.BookId == bookId, ct);

			if (copy is null)
			{
				return BookCopyErrors.NotFound(copyId);
			}

			return copy.ToResponse();
		}

		public async Task<Result<BookCopyResponse>> CreateAsync(
			Guid bookId,
			CreateBookCopyRequest request,
			CancellationToken ct)
		{
			// 1) The book in the URL must exist (404)
			if (!await BookExistsAsync(bookId, ct))
			{
				return BookErrors.NotFound(bookId);
			}

			// 2) The barcode must be free (409)
			var barcode = BarcodeHelper.Normalize(request.Barcode);

			if (await BarcodeExistsAsync(barcode, excludeId: null, ct))
			{
				return BookCopyErrors.BarcodeAlreadyExists(barcode);
			}

			var copy = request.ToEntity(bookId);

			db.BookCopies.Add(copy);
			await db.SaveChangesAsync(ct);

			return copy.ToResponse();
		}

		public async Task<Result<BookCopyResponse>> UpdateAsync(
			Guid bookId,
			Guid copyId,
			UpdateBookCopyRequest request,
			CancellationToken ct)
		{
			var copy = await FindCopyAsync(bookId, copyId, ct);

			if (copy is null)
			{
				return BookCopyErrors.NotFound(copyId);
			}

			var barcode = BarcodeHelper.Normalize(request.Barcode);

			if (await BarcodeExistsAsync(barcode, excludeId: copyId, ct))
			{
				return BookCopyErrors.BarcodeAlreadyExists(barcode);
			}

			request.ApplyTo(copy);
			await db.SaveChangesAsync(ct);

			return copy.ToResponse();
		}

		public async Task<Result<BookCopyResponse>> ChangeStatusAsync(
			Guid bookId,
			Guid copyId,
			ChangeCopyStatusRequest request,
			CancellationToken ct)
		{
			var copy = await FindCopyAsync(bookId, copyId, ct);

			if (copy is null)
			{
				return BookCopyErrors.NotFound(copyId);
			}

			// Only the loan system may move a copy out of OnLoan
			if (copy.Status == CopyStatus.OnLoan)
			{
				return BookCopyErrors.CopyIsOnLoan;
			}

			copy.Status = request.Status;
			await db.SaveChangesAsync(ct);

			return copy.ToResponse();
		}

		public async Task<Result> DeleteAsync(
			Guid bookId,
			Guid copyId,
			CancellationToken ct)
		{
			var copy = await FindCopyAsync(bookId, copyId, ct);

			if (copy is null)
			{
				return BookCopyErrors.NotFound(copyId);
			}

			if (copy.Status == CopyStatus.OnLoan)
			{
				return BookCopyErrors.CopyIsOnLoan;
			}

			db.BookCopies.Remove(copy); // becomes a soft delete
			await db.SaveChangesAsync(ct);

			return Result.Success();
		}

		// ---------- Private helpers ----------

		// Must match BOTH ids, so a copy can't be reached through the wrong book
		private Task<BookCopy?> FindCopyAsync(Guid bookId, Guid copyId, CancellationToken ct) =>
			db.BookCopies.FirstOrDefaultAsync(
				c => c.Id == copyId && c.BookId == bookId,
				ct);

		private Task<bool> BookExistsAsync(Guid bookId, CancellationToken ct) =>
			db.Books.AnyAsync(b => b.Id == bookId, ct);

		private Task<bool> BarcodeExistsAsync(string barcode, Guid? excludeId, CancellationToken ct) =>
			db.BookCopies.AnyAsync(
				c => c.Id != excludeId && c.Barcode == barcode,
				ct);
	}
}