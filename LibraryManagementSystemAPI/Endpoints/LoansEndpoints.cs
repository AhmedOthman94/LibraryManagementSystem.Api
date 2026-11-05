using LibraryManagementSystemAPI.Common;
using LibraryManagementSystemAPI.Dtos;
using LibraryManagementSystemAPI.Filters;
using LibraryManagementSystemAPI.Services.Loans;
using Microsoft.AspNetCore.Http.HttpResults;

namespace LibraryManagementSystemAPI.Endpoints
{
	public static class LoansEndpoints
	{
		public static void MapLoanEndpoints(this IEndpointRouteBuilder app)
		{
			var group = app
				.MapGroup("api/loans")
				.WithTags("Loans");

			group.MapGet("/", GetAll)
				.WithValidation<LoanQueryParameters>();

			group.MapGet("/{id:guid}", GetById)
				.WithName("GetLoanById")
				.ProducesProblem(StatusCodes.Status404NotFound);

			group.MapPost("/", Borrow)
				.WithValidation<BorrowBookRequest>()
				.ProducesProblem(StatusCodes.Status409Conflict);

			group.MapPost("/{id:guid}/return", ReturnBook)
				.ProducesProblem(StatusCodes.Status404NotFound)
				.ProducesProblem(StatusCodes.Status409Conflict);

			group.MapPost("/{id:guid}/renew", Renew)
				.ProducesProblem(StatusCodes.Status404NotFound)
				.ProducesProblem(StatusCodes.Status409Conflict);

			// No PUT and no DELETE: loans are history
		}

		private static async Task<Ok<PagedResponse<LoanResponse>>> GetAll(
			[AsParameters] LoanQueryParameters parameters,
			ILoanService loanService,
			CancellationToken ct)
		{
			var loans = await loanService.GetAllAsync(parameters, ct);
			return TypedResults.Ok(loans);
		}

		private static async Task<Results<Ok<LoanResponse>, ProblemHttpResult>> GetById(
			Guid id,
			ILoanService loanService,
			CancellationToken ct)
		{
			var result = await loanService.GetByIdAsync(id, ct);

			if (result.IsFailure)
			{
				return result.Error.ToProblem();
			}

			return TypedResults.Ok(result.Value);
		}

		private static async Task<Results<CreatedAtRoute<LoanResponse>, ProblemHttpResult>> Borrow(
			BorrowBookRequest request,
			ILoanService loanService,
			CancellationToken ct)
		{
			var result = await loanService.BorrowAsync(request, ct);

			if (result.IsFailure)
			{
				return result.Error.ToProblem();
			}

			return TypedResults.CreatedAtRoute(
				result.Value,
				"GetLoanById",
				new { id = result.Value.Id }
			);
		}

		private static async Task<Results<Ok<LoanResponse>, ProblemHttpResult>> ReturnBook(
			Guid id,
			ReturnLoanRequest? request,
			ILoanService loanService,
			CancellationToken ct)
		{
			var result = await loanService.ReturnAsync(id, request, ct);

			if (result.IsFailure)
			{
				return result.Error.ToProblem();
			}

			return TypedResults.Ok(result.Value);
		}

		private static async Task<Results<Ok<LoanResponse>, ProblemHttpResult>> Renew(
			Guid id,
			ILoanService loanService,
			CancellationToken ct)
		{
			var result = await loanService.RenewAsync(id, ct);

			if (result.IsFailure)
			{
				return result.Error.ToProblem();
			}

			return TypedResults.Ok(result.Value);
		}
	}
}