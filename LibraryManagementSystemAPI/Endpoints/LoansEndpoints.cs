using LibraryManagementSystemAPI.Auth;
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
			// No group rule: the fallback policy already requires a logged-in user
			var group = app
				.MapGroup("api/loans")
				.WithTags("Loans");

			group.MapGet("/", GetAll)
				.RequireAuthorization(AuthPolicies.Staff)
				.WithValidation<LoanQueryParameters>();

			// Staff: any loan. Member: only their own (others look like 404)
			group.MapGet("/{id:guid}", GetById)
				.WithName("GetLoanById")
				.ProducesProblem(StatusCodes.Status404NotFound);

			group.MapPost("/", Borrow)
				.RequireAuthorization(AuthPolicies.Staff)
				.WithValidation<BorrowBookRequest>()
				.ProducesProblem(StatusCodes.Status409Conflict);

			group.MapPost("/{id:guid}/return", ReturnBook)
				.RequireAuthorization(AuthPolicies.Staff)
				.ProducesProblem(StatusCodes.Status404NotFound)
				.ProducesProblem(StatusCodes.Status409Conflict);

			// Staff, or a member renewing their own loan online
			group.MapPost("/{id:guid}/renew", Renew)
				.ProducesProblem(StatusCodes.Status404NotFound)
				.ProducesProblem(StatusCodes.Status409Conflict);
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
	ICurrentUser currentUser,
	ILoanService loanService,
	CancellationToken ct)
		{
			var result = await loanService.GetByIdAsync(id, OwnLoansOnly(currentUser), ct);

			if (result.IsFailure)
			{
				return result.Error.ToProblem();
			}

			return TypedResults.Ok(result.Value);
		}

		private static async Task<Results<Ok<LoanResponse>, ProblemHttpResult>> Renew(
			Guid id,
			ICurrentUser currentUser,
			ILoanService loanService,
			CancellationToken ct)
		{
			var result = await loanService.RenewAsync(id, OwnLoansOnly(currentUser), ct);

			if (result.IsFailure)
			{
				return result.Error.ToProblem();
			}

			return TypedResults.Ok(result.Value);
		}

		// Staff → null (no restriction).
		// Anyone else → their member id, or Guid.Empty (matches nothing) if they have none.
		private static Guid? OwnLoansOnly(ICurrentUser currentUser) =>
			currentUser.IsStaff ? null : currentUser.MemberId ?? Guid.Empty;

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

		
	}
}