using LibraryManagementSystemAPI.Common;
using LibraryManagementSystemAPI.DataAccess;
using LibraryManagementSystemAPI.Dtos;
using LibraryManagementSystemAPI.Mappings;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystemAPI.Services.Categories
{
	public class CategoryService(LibraryDbContext db) : ICategoryService
	{
		public async Task<List<CategoryResponse>> GetAllAsync(CancellationToken ct)
		{
			return await db.Categories
				.AsNoTracking()
				.OrderBy(c => c.Name)
				.Select(c => c.ToResponse())
				.ToListAsync(ct);
		}

		public async Task<Result<CategoryResponse>> GetByIdAsync(Guid id, CancellationToken ct)
		{
			var category = await db.Categories
				.AsNoTracking()
				.FirstOrDefaultAsync(c => c.Id == id, ct);

			if (category is null)
			{
				return CategoryErrors.NotFound(id);
			}

			return category.ToResponse();
		}

		public async Task<Result<CategoryResponse>> CreateAsync(
			CreateCategoryRequest request,
			CancellationToken ct)
		{
			if (await NameExistsAsync(request.Name, excludeId: null, ct))
			{
				return CategoryErrors.NameAlreadyExists(request.Name.Trim());
			}

			var category = request.ToEntity();

			db.Categories.Add(category);
			await db.SaveChangesAsync(ct);

			return category.ToResponse();
		}

		public async Task<Result<CategoryResponse>> UpdateAsync(
			Guid id,
			UpdateCategoryRequest request,
			CancellationToken ct)
		{
			var category = await db.Categories
				.FirstOrDefaultAsync(c => c.Id == id, ct);

			if (category is null)
			{
				return CategoryErrors.NotFound(id);
			}

			if (await NameExistsAsync(request.Name, excludeId: id, ct))
			{
				return CategoryErrors.NameAlreadyExists(request.Name.Trim());
			}

			request.ApplyTo(category);
			await db.SaveChangesAsync(ct);

			return category.ToResponse();
		}

		public async Task<Result> DeleteAsync(Guid id, CancellationToken ct)
		{
			var category = await db.Categories
				.FirstOrDefaultAsync(c => c.Id == id, ct);

			if (category is null)
			{
				return CategoryErrors.NotFound(id);
			}

			if (await db.Books.AnyAsync(b => b.CategoryId == id, ct))
			{
				return CategoryErrors.HasBooks;
			}

			db.Categories.Remove(category); // becomes a soft delete
			await db.SaveChangesAsync(ct);

			return Result.Success();
		}

		// Used by Create AND Update: one rule, written once
		private Task<bool> NameExistsAsync(string name, Guid? excludeId, CancellationToken ct)
		{
			var normalizedName = name.Trim().ToLower();

			return db.Categories.AnyAsync(
				c => c.Id != excludeId && c.Name.ToLower() == normalizedName,
				ct);
		}
	}
}