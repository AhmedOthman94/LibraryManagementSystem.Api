using LibraryManagementSystemAPI.Dtos;
using LibraryManagementSystemAPI.Entities;

namespace LibraryManagementSystemAPI.Mappings
{
	public static class AuthorMappings
	{
		public static AuthorResponse ToResponse(this Author author) =>
			new(
				author.Id,
				author.FirstName,
				author.LastName,
				$"{author.FirstName} {author.LastName}",
				author.Biography,
				author.Nationality,
				author.DateOfBirth,
				author.DateOfDeath,
				author.ImageUrl,
				author.CreatedAt,
				author.UpdatedAt
			);

		public static Author ToEntity(this CreateAuthorRequest request) =>
			new()
			{
				FirstName = request.FirstName.Trim(),
				LastName = request.LastName.Trim(),
				Biography = request.Biography?.Trim(),
				Nationality = request.Nationality?.Trim(),
				DateOfBirth = request.DateOfBirth,
				DateOfDeath = request.DateOfDeath,
				ImageUrl = request.ImageUrl?.Trim()
			};

		public static void ApplyTo(this UpdateAuthorRequest request, Author author)
		{
			author.FirstName = request.FirstName.Trim();
			author.LastName = request.LastName.Trim();
			author.Biography = request.Biography?.Trim();
			author.Nationality = request.Nationality?.Trim();
			author.DateOfBirth = request.DateOfBirth;
			author.DateOfDeath = request.DateOfDeath;
			author.ImageUrl = request.ImageUrl?.Trim();
		}
	}
}