using LibraryManagementSystemAPI.DataAccess;
using LibraryManagementSystemAPI.Dtos;
using LibraryManagementSystemAPI.Mappings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystemAPI.Controllers
{
	[ApiController]
	[Route("api/authors")]
	public class AuthorsController(
							LibraryDbContext db) 
	: ControllerBase
	{
		[HttpGet]
		[ProducesResponseType(typeof(IEnumerable<AuthorResponse>), StatusCodes.Status200OK)]
		public async Task<ActionResult<IEnumerable<AuthorResponse>>> GetAll(CancellationToken ct)
		{
			var authors = await db.Authors
				.AsNoTracking()
				.OrderBy(a => a.LastName)
				.ThenBy(a => a.FirstName)
				.Select(a => a.ToResponse())
				.ToListAsync(ct);

			return Ok(authors);
		}

		[HttpGet("{id:guid}")]
		[ProducesResponseType(typeof(AuthorResponse), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
		public async Task<ActionResult<AuthorResponse>> GetById(Guid id, CancellationToken ct)
		{
			var author = await db.Authors
				.AsNoTracking()
				.FirstOrDefaultAsync(a => a.Id == id, ct);

			if (author is null)
			{
				return Problem(
					detail: $"Author with Id: {id} was not found.",
					statusCode: StatusCodes.Status404NotFound
				);
			}

			return Ok(author.ToResponse());
		}

		[HttpPost]
		[ProducesResponseType(typeof(AuthorResponse), StatusCodes.Status201Created)]
		[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
		public async Task<ActionResult<AuthorResponse>> Create(
			CreateAuthorRequest request,
			CancellationToken ct)
		{
			var author = request.ToEntity();

			db.Authors.Add(author);
			await db.SaveChangesAsync(ct);

			return CreatedAtAction(
				nameof(GetById),
				new { id = author.Id },
				author.ToResponse()
			);
		}

		[HttpPut("{id:guid}")]
		[ProducesResponseType(typeof(AuthorResponse), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
		public async Task<ActionResult<AuthorResponse>> Update(
			Guid id,
			UpdateAuthorRequest request,
			CancellationToken ct)
		{
			var author = await db.Authors
				.FirstOrDefaultAsync(a => a.Id == id, ct);

			if (author is null)
			{
				return Problem(
					detail: $"Author with Id: {id} was not found.",
					statusCode: StatusCodes.Status404NotFound
				);
			}

			request.ApplyTo(author);
			await db.SaveChangesAsync(ct);

			return Ok(author.ToResponse());
		}

		[HttpDelete("{id:guid}")]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
		[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
		public async Task<ActionResult> Delete(Guid id, CancellationToken ct)
		{
			var author = await db.Authors
				.FirstOrDefaultAsync(a => a.Id == id, ct);

			if (author is null)
			{
				return Problem(
					detail: $"Author with Id: {id} was not found.",
					statusCode: StatusCodes.Status404NotFound
				);
			}

			var hasBooks = await db.BookAuthors.AnyAsync(ba => ba.AuthorId == id, ct);

			if (hasBooks)
			{
				return Problem(
					detail: "This author still has books. Remove the author from them first.",
					statusCode: StatusCodes.Status409Conflict
				);
			}

			db.Authors.Remove(author);
			await db.SaveChangesAsync(ct);

			return NoContent();
		}
	}
}