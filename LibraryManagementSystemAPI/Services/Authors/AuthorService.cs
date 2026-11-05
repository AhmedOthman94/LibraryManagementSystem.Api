using LibraryManagementSystemAPI.Common;
using LibraryManagementSystemAPI.DataAccess;
using LibraryManagementSystemAPI.Dtos;
using LibraryManagementSystemAPI.Mappings;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystemAPI.Services.Authors
{
	public class AuthorService(LibraryDbContext db) : IAuthorService
	{
		public async Task<List<AuthorResponse>> GetAllAsync(CancellationToken ct)
		{
			return await db.Authors
				.AsNoTracking()
				.OrderBy(a => a.LastName)
				.ThenBy(a => a.FirstName)
				.Select(a => a.ToResponse())
				.ToListAsync(ct);
		}

		public async Task<Result<AuthorResponse>> GetByIdAsync(Guid id, CancellationToken ct)
		{
			var author = await db.Authors
				.AsNoTracking()
				.FirstOrDefaultAsync(a => a.Id == id, ct);

			if (author is null)
			{
				return AuthorErrors.NotFound(id);
			}

			return author.ToResponse();
		}

		public async Task<AuthorResponse> CreateAsync(
			CreateAuthorRequest request,
			CancellationToken ct)
		{
			var author = request.ToEntity();

			db.Authors.Add(author);
			await db.SaveChangesAsync(ct);

			return author.ToResponse();
		}

		public async Task<Result<AuthorResponse>> UpdateAsync(
			Guid id,
			UpdateAuthorRequest request,
			CancellationToken ct)
		{
			var author = await db.Authors
				.FirstOrDefaultAsync(a => a.Id == id, ct);

			if (author is null)
			{
				return AuthorErrors.NotFound(id);
			}

			request.ApplyTo(author);
			await db.SaveChangesAsync(ct);

			return author.ToResponse();
		}

		public async Task<Result> DeleteAsync(Guid id, CancellationToken ct)
		{
			var author = await db.Authors
				.FirstOrDefaultAsync(a => a.Id == id, ct);

			if (author is null)
			{
				return AuthorErrors.NotFound(id);
			}

			// The BookAuthor query filter ignores links to deleted books
			if (await db.BookAuthors.AnyAsync(ba => ba.AuthorId == id, ct))
			{
				return AuthorErrors.HasBooks;
			}

			db.Authors.Remove(author); // becomes a soft delete
			await db.SaveChangesAsync(ct);

			return Result.Success();
		}
	}
}