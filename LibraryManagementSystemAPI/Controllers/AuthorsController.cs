using LibraryManagementSystemAPI.Auth;
using LibraryManagementSystemAPI.Common;
using LibraryManagementSystemAPI.Dtos;
using LibraryManagementSystemAPI.Services.Authors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagementSystemAPI.Controllers
{
	[ApiController]
	[Route("api/authors")]
	[Authorize(Policy = AuthPolicies.Staff)]
	public class AuthorsController(IAuthorService authorService) : ControllerBase
	{
		[HttpGet]
		[AllowAnonymous]
		[ProducesResponseType(typeof(PagedResponse<AuthorResponse>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
		public async Task<ActionResult<PagedResponse<AuthorResponse>>> GetAll(
				[FromQuery] AuthorQueryParameters parameters,
				CancellationToken ct)
		{
			var authors = await authorService.GetAllAsync(parameters, ct);
			return Ok(authors);
		}

		[HttpGet("{id:guid}")]
		[AllowAnonymous]
		[ProducesResponseType(typeof(AuthorResponse), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
		public async Task<ActionResult<AuthorResponse>> GetById(Guid id, CancellationToken ct)
		{
			var result = await authorService.GetByIdAsync(id, ct);

			if (result.IsFailure)
			{
				return this.ToProblem(result.Error);
			}

			return Ok(result.Value);
		}

		[HttpPost]
		[ProducesResponseType(typeof(AuthorResponse), StatusCodes.Status201Created)]
		[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
		public async Task<ActionResult<AuthorResponse>> Create(
			CreateAuthorRequest request,
			CancellationToken ct)
		{
			var author = await authorService.CreateAsync(request, ct);

			return CreatedAtAction(nameof(GetById), new { id = author.Id }, author);
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
			var result = await authorService.UpdateAsync(id, request, ct);

			if (result.IsFailure)
			{
				return this.ToProblem(result.Error);
			}

			return Ok(result.Value);
		}

		[HttpDelete("{id:guid}")]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
		[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
		public async Task<ActionResult> Delete(Guid id, CancellationToken ct)
		{
			var result = await authorService.DeleteAsync(id, ct);

			if (result.IsFailure)
			{
				return this.ToProblem(result.Error);
			}

			return NoContent();
		}
	}
}