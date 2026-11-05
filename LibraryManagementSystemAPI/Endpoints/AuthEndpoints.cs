using LibraryManagementSystemAPI.Auth;
using LibraryManagementSystemAPI.Common;
using LibraryManagementSystemAPI.Dtos;
using LibraryManagementSystemAPI.Filters;
using LibraryManagementSystemAPI.Services.Auth;
using LibraryManagementSystemAPI.Services.Loans;
using Microsoft.AspNetCore.Http.HttpResults;

namespace LibraryManagementSystemAPI.Endpoints
{
	public static class AuthEndpoints
	{
		public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
		{
			// Public: you can't be logged in before logging in
			var auth = app
				.MapGroup("api/auth")
				.WithTags("Auth")
				.AllowAnonymous();

			auth.MapPost("/login", Login)
				.WithValidation<LoginRequest>()
				.ProducesProblem(StatusCodes.Status401Unauthorized);

			auth.MapPost("/register-member", RegisterMember)
				.WithValidation<RegisterMemberAccountRequest>()
				.ProducesProblem(StatusCodes.Status409Conflict);

			// Any logged-in user (the fallback policy applies)
			var me = app
				.MapGroup("api/me")
				.WithTags("Me");

			me.MapGet("/", GetMe);

			me.MapGet("/loans", GetMyLoans)
				.RequireAuthorization(AuthPolicies.Member)
				.WithValidation<LoanQueryParameters>();

			// Admin only
			var users = app
				.MapGroup("api/users")
				.WithTags("Users")
				.RequireAuthorization(AuthPolicies.Admin);

			users.MapPost("/", CreateStaffUser)
				.WithValidation<CreateStaffUserRequest>()
				.ProducesProblem(StatusCodes.Status409Conflict);
		}

		private static async Task<Results<Ok<AuthResponse>, ProblemHttpResult>> Login(
			LoginRequest request,
			IAuthService authService,
			CancellationToken ct)
		{
			var result = await authService.LoginAsync(request, ct);

			if (result.IsFailure)
			{
				return result.Error.ToProblem();
			}

			return TypedResults.Ok(result.Value);
		}

		private static async Task<Results<Ok<AuthResponse>, ProblemHttpResult>> RegisterMember(
			RegisterMemberAccountRequest request,
			IAuthService authService,
			CancellationToken ct)
		{
			var result = await authService.RegisterMemberAccountAsync(request, ct);

			if (result.IsFailure)
			{
				return result.Error.ToProblem();
			}

			return TypedResults.Ok(result.Value);
		}

		private static async Task<Results<Ok<UserResponse>, ProblemHttpResult>> GetMe(
			ICurrentUser currentUser,
			IAuthService authService,
			CancellationToken ct)
		{
			if (currentUser.UserId is not Guid userId)
			{
				return TypedResults.Problem(statusCode: StatusCodes.Status401Unauthorized);
			}

			var result = await authService.GetUserAsync(userId, ct);

			if (result.IsFailure)
			{
				return result.Error.ToProblem();
			}

			return TypedResults.Ok(result.Value);
		}

		private static async Task<Results<Ok<PagedResponse<LoanResponse>>, ProblemHttpResult>> GetMyLoans(
			[AsParameters] LoanQueryParameters parameters,
			ICurrentUser currentUser,
			ILoanService loanService,
			CancellationToken ct)
		{
			// Fail closed: a member token without a member id gets NOTHING
			if (currentUser.MemberId is not Guid memberId)
			{
				return TypedResults.Problem(statusCode: StatusCodes.Status403Forbidden);
			}

			// Identity comes from the TOKEN. Whatever the client put in ?memberId= is replaced.
			parameters.MemberId = memberId;

			var loans = await loanService.GetAllAsync(parameters, ct);
			return TypedResults.Ok(loans);
		}

		private static async Task<Results<Ok<UserResponse>, ProblemHttpResult>> CreateStaffUser(
			CreateStaffUserRequest request,
			IAuthService authService,
			CancellationToken ct)
		{
			var result = await authService.CreateStaffUserAsync(request, ct);

			if (result.IsFailure)
			{
				return result.Error.ToProblem();
			}

			return TypedResults.Ok(result.Value);
		}
	}
}