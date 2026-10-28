namespace LibraryManagementSystemAPI.Dtos
{
	public interface IAuthorRequest
	{
		string FirstName { get; }
		string LastName { get; }
		string? Biography { get; }
		string? Nationality { get; }
		DateOnly? DateOfBirth { get; }
		DateOnly? DateOfDeath { get; }
		string? ImageUrl { get; }
	}

	public record CreateAuthorRequest(
		string FirstName,
		string LastName,
		string? Biography,
		string? Nationality,
		DateOnly? DateOfBirth,
		DateOnly? DateOfDeath,
		string? ImageUrl
	) : IAuthorRequest;

	public record UpdateAuthorRequest(
		string FirstName,
		string LastName,
		string? Biography,
		string? Nationality,
		DateOnly? DateOfBirth,
		DateOnly? DateOfDeath,
		string? ImageUrl
	) : IAuthorRequest;

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