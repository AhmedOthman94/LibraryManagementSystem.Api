namespace LibraryManagementSystemAPI.Dtos
{
	public record CreateAuthorRequest(
		string FirstName,
		string LastName,
		string? Biography,
		string? Nationality,
		DateOnly? DateOfBirth,
		DateOnly? DateOfDeath,
		string? ImageUrl
	);

	public record UpdateAuthorRequest(
		string FirstName,
		string LastName,
		string? Biography,
		string? Nationality,
		DateOnly? DateOfBirth,
		DateOnly? DateOfDeath,
		string? ImageUrl
	);

	public record AuthorResponse(
		Guid Id,
		string FirstName,
		string LastName,
		string FullName,
		string? Biography,
		string? Nationality,
		DateOnly? DateOfBirth,
		DateOnly? DateOfDeath,
		string? ImageUrl,
		DateTime CreatedAt,
		DateTime? UpdatedAt
	);
}