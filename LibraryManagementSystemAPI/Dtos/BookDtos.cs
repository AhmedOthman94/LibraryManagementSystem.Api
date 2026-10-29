namespace LibraryManagementSystemAPI.Dtos
{
	public interface IBookRequest
	{
		string Title { get; }
		string? Description { get; }
		string Isbn { get; }
		string? Publisher { get; }
		int? PublicationYear { get; }
		int? PageCount { get; }
		string? Language { get; }
		string? CoverImageUrl { get; }
		Guid CategoryId { get; }
		List<Guid> AuthorIds { get; }
	}

	public record CreateBookRequest(
		string Title,
		string? Description,
		string Isbn,
		string? Publisher,
		int? PublicationYear,
		int? PageCount,
		string? Language,
		string? CoverImageUrl,
		Guid CategoryId,
		List<Guid> AuthorIds
	) : IBookRequest;

	public record UpdateBookRequest(
		string Title,
		string? Description,
		string Isbn,
		string? Publisher,
		int? PublicationYear,
		int? PageCount,
		string? Language,
		string? CoverImageUrl,
		Guid CategoryId,
		List<Guid> AuthorIds
	) : IBookRequest;

	// Small pieces used inside the book response
	public record CategorySummary(Guid Id, string Name);
	public record AuthorSummary(Guid Id, string FullName);

	public record BookResponse(
		Guid Id,
		string Title,
		string? Description,
		string Isbn,
		string? Publisher,
		int? PublicationYear,
		int? PageCount,
		string? Language,
		string? CoverImageUrl,
		CategorySummary Category,
		List<AuthorSummary> Authors,
		DateTime CreatedAt,
		DateTime? UpdatedAt
	);
}