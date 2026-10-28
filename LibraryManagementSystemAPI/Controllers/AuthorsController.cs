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
			var error = Validate(
				request.FirstName, request.LastName,
				request.DateOfBirth, request.DateOfDeath);

			if (error is not null)
			{
				return Problem(detail: error, statusCode: StatusCodes.Status400BadRequest);
			}

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
			var error = Validate(
				request.FirstName, request.LastName,
				request.DateOfBirth, request.DateOfDeath);

			if (error is not null)
			{
				return Problem(detail: error, statusCode: StatusCodes.Status400BadRequest);
			}

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

			db.Authors.Remove(author);
			await db.SaveChangesAsync(ct);

			return NoContent();
		}

		private static string? Validate(
			string? firstName,
			string? lastName,
			DateOnly? dateOfBirth,
			DateOnly? dateOfDeath)
		{
			if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
			{
				return "First and last name are required.";
			}

			var today = DateOnly.FromDateTime(DateTime.UtcNow);

			if (dateOfBirth > today)
			{
				return "Date of birth can't be in the future.";
			}

			if (dateOfDeath > today)
			{
				return "Date of death can't be in the future.";
			}

			if (dateOfBirth is not null && dateOfDeath is not null && dateOfDeath < dateOfBirth)
			{
				return "Date of death can't be before date of birth.";
			}

			return null;
		}
	}
}