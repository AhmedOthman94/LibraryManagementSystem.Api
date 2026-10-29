using LibraryManagementSystemAPI.Common;
using LibraryManagementSystemAPI.Dtos;
using LibraryManagementSystemAPI.Entities;

namespace LibraryManagementSystemAPI.Mappings
{
	public static class BookMappings
	{
		// Needs Category and BookAuthors.Author to be loaded (Include)
		public static BookResponse ToResponse(this Book book) =>
			new(
				book.Id,
				book.Title,
				book.Description,
				book.Isbn,
				book.Publisher,
				book.PublicationYear,
				book.PageCount,
				book.Language,
				book.CoverImageUrl,
				new CategorySummary(book.Category.Id, book.Category.Name),
				book.BookAuthors
					.OrderBy(ba => ba.AuthorOrder)
					.Select(ba => new AuthorSummary(
						ba.Author.Id,
						$"{ba.Author.FirstName} {ba.Author.LastName}"))
					.ToList(),
				book.CreatedAt,
				book.UpdatedAt
			);

		public static Book ToEntity(this CreateBookRequest request) =>
			new()
			{
				Title = request.Title.Trim(),
				Description = request.Description?.Trim(),
				Isbn = IsbnHelper.Normalize(request.Isbn),
				Publisher = request.Publisher?.Trim(),
				PublicationYear = request.PublicationYear,
				PageCount = request.PageCount,
				Language = request.Language?.Trim(),
				CoverImageUrl = request.CoverImageUrl?.Trim(),
				CategoryId = request.CategoryId,
				// The position in the list becomes the author's order (1, 2, 3...)
				BookAuthors = request.AuthorIds
					.Select((authorId, index) => new BookAuthor
					{
						AuthorId = authorId,
						AuthorOrder = index + 1
					})
					.ToList()
			};

		// Book must be loaded WITH its BookAuthors (Include) before calling this
		public static void ApplyTo(this UpdateBookRequest request, Book book)
		{
			book.Title = request.Title.Trim();
			book.Description = request.Description?.Trim();
			book.Isbn = IsbnHelper.Normalize(request.Isbn);
			book.Publisher = request.Publisher?.Trim();
			book.PublicationYear = request.PublicationYear;
			book.PageCount = request.PageCount;
			book.Language = request.Language?.Trim();
			book.CoverImageUrl = request.CoverImageUrl?.Trim();
			book.CategoryId = request.CategoryId;

			// 1) Remove authors that are not in the new list
			var removed = book.BookAuthors
				.Where(ba => !request.AuthorIds.Contains(ba.AuthorId))
				.ToList();

			foreach (var link in removed)
			{
				book.BookAuthors.Remove(link);
			}

			// 2) Add new authors, and update the order of the ones that stayed
			for (var i = 0; i < request.AuthorIds.Count; i++)
			{
				var authorId = request.AuthorIds[i];
				var existing = book.BookAuthors.FirstOrDefault(ba => ba.AuthorId == authorId);

				if (existing is null)
				{
					book.BookAuthors.Add(new BookAuthor
					{
						AuthorId = authorId,
						AuthorOrder = i + 1
					});
				}
				else
				{
					existing.AuthorOrder = i + 1;
				}
			}
		}
	}
}