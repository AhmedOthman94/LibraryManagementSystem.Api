using LibraryManagementSystemAPI.Entities;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagementSystemAPI.Controllers
{
	[ApiController]
	[Route("api/authors")]
	public class AuthorsController : ControllerBase
	{
		private static List<Author> _authors =
		[
			new Author {Id = Guid.NewGuid(), FirstName = "Ahmed", LastName = "Othman", YearOfbirth = 1988},
			new Author {Id = Guid.NewGuid(), FirstName = "Sara", LastName = "Ali", YearOfbirth = 1991},
			new Author {Id = Guid.NewGuid(), FirstName = "Alla", LastName = "Gaber", YearOfbirth = 1990},
			new Author {Id = Guid.NewGuid(), FirstName = "Emy", LastName = "Ahmed", YearOfbirth = 1964},
		];

		[HttpGet("{id:Guid}")]
		[ProducesResponseType(typeof(Author), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
		public async Task<ActionResult<Author>> GetById(Guid id)
		{
			var author = _authors.FirstOrDefault(a => a.Id == id);
			if (author is null)
			{
				return Problem(
					detail: $"Author with Id: {id} was not found.",
					statusCode: StatusCodes.Status200OK
				);
			}

			return Ok(author);
		}

		[HttpGet]
		[ProducesResponseType(typeof(IEnumerable<Author>), StatusCodes.Status200OK)]
		public async Task<ActionResult<IEnumerable<Author>>> GetAll()
		{
			return Ok(_authors);
		}

		[HttpPost]
		[ProducesResponseType(typeof(Author), StatusCodes.Status201Created)]
		[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]

		public async Task<ActionResult<Author>> Create(Author createdAuthor)
		{
			if (string.IsNullOrWhiteSpace(createdAuthor.FirstName)
				|| string.IsNullOrWhiteSpace(createdAuthor.LastName)
			)
			{
				return Problem(
					detail: "First and Last name are mandatory.",
					statusCode: StatusCodes.Status400BadRequest
				);
			}

			var existingAuthor = _authors.FirstOrDefault(a => a.Id == createdAuthor.Id);
			if (existingAuthor is not null)
			{
				return Problem(
					detail: $"Author with Id: {createdAuthor.Id} already exists.",
					statusCode: StatusCodes.Status409Conflict
				);
			}

			_authors.Add(createdAuthor);

			return CreatedAtAction(
				nameof(GetById),
				new { id = createdAuthor.Id },
				createdAuthor
			);
		}

		[HttpPut("{id:Guid}")]
		[ProducesResponseType(typeof(Author), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]

		public async Task<ActionResult<Author>> Update(Guid id,
		Author updatedAuthor)
		{
			if (string.IsNullOrWhiteSpace(updatedAuthor.FirstName)
				|| string.IsNullOrWhiteSpace(updatedAuthor.LastName)
			)
			{
				return Problem(
					detail: "First and Last name are mandatory.",
					statusCode: StatusCodes.Status400BadRequest
				);
			}

			var existingAuthor = _authors.FirstOrDefault(a => a.Id == updatedAuthor.Id);
			if (existingAuthor is null)
			{
				return Problem(
					detail: $"Author with Id: {updatedAuthor.Id} was not found.",
					statusCode: StatusCodes.Status404NotFound
				);
			}

			existingAuthor.FirstName = updatedAuthor.FirstName;
			existingAuthor.LastName = updatedAuthor.LastName;
			existingAuthor.YearOfbirth = updatedAuthor.YearOfbirth;

			return Ok(existingAuthor);
		}

		[HttpDelete("{id:Guid}")]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
		public async Task<ActionResult> Delete(Guid id)
		{
			var author = _authors.FirstOrDefault(a => a.Id == id);
			if (author is null)
			{
				return Problem(
					detail: $"Author with Id: {id} was not found.",
					statusCode: StatusCodes.Status404NotFound
				);
			}
			_authors.Remove(author);

			return NoContent();
		}
	}
}
