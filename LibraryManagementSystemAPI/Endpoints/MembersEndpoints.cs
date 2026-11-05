using LibraryManagementSystemAPI.Common;
using LibraryManagementSystemAPI.Dtos;
using LibraryManagementSystemAPI.Filters;
using LibraryManagementSystemAPI.Services.Members;
using Microsoft.AspNetCore.Http.HttpResults;

namespace LibraryManagementSystemAPI.Endpoints
{
	public static class MembersEndpoints
	{
		public static void MapMemberEndpoints(this IEndpointRouteBuilder app)
		{
			var group = app
				.MapGroup("api/members")
				.WithTags("Members");

			group.MapGet("/", GetAll)
				.WithValidation<MemberQueryParameters>();

			group.MapGet("/{id:guid}", GetById)
				.WithName("GetMemberById")
				.ProducesProblem(StatusCodes.Status404NotFound);

			group.MapPost("/", Create)
				.WithValidation<CreateMemberRequest>()
				.ProducesProblem(StatusCodes.Status409Conflict);

			group.MapPut("/{id:guid}", Update)
				.WithValidation<UpdateMemberRequest>()
				.ProducesProblem(StatusCodes.Status404NotFound)
				.ProducesProblem(StatusCodes.Status409Conflict);

			// Actions: POST, because they are business operations (renew is not idempotent)
			group.MapPost("/{id:guid}/suspend", Suspend)
				.WithValidation<SuspendMemberRequest>()
				.ProducesProblem(StatusCodes.Status404NotFound)
				.ProducesProblem(StatusCodes.Status409Conflict);

			group.MapPost("/{id:guid}/reactivate", Reactivate)
				.ProducesProblem(StatusCodes.Status404NotFound)
				.ProducesProblem(StatusCodes.Status409Conflict);

			group.MapPost("/{id:guid}/renew", Renew)
				.ProducesProblem(StatusCodes.Status404NotFound)
				.ProducesProblem(StatusCodes.Status409Conflict);

			group.MapDelete("/{id:guid}", Delete)
				.ProducesProblem(StatusCodes.Status404NotFound);
		}

		private static async Task<Ok<PagedResponse<MemberResponse>>> GetAll(
			[AsParameters] MemberQueryParameters parameters,
			IMemberService memberService,
			CancellationToken ct)
		{
			var members = await memberService.GetAllAsync(parameters, ct);
			return TypedResults.Ok(members);
		}

		private static async Task<Results<Ok<MemberResponse>, ProblemHttpResult>> GetById(
			Guid id,
			IMemberService memberService,
			CancellationToken ct)
		{
			var result = await memberService.GetByIdAsync(id, ct);

			if (result.IsFailure)
			{
				return result.Error.ToProblem();
			}

			return TypedResults.Ok(result.Value);
		}

		private static async Task<Results<CreatedAtRoute<MemberResponse>, ProblemHttpResult>> Create(
			CreateMemberRequest request,
			IMemberService memberService,
			CancellationToken ct)
		{
			var result = await memberService.CreateAsync(request, ct);

			if (result.IsFailure)
			{
				return result.Error.ToProblem();
			}

			return TypedResults.CreatedAtRoute(
				result.Value,
				"GetMemberById",
				new { id = result.Value.Id }
			);
		}

		private static async Task<Results<Ok<MemberResponse>, ProblemHttpResult>> Update(
			Guid id,
			UpdateMemberRequest request,
			IMemberService memberService,
			CancellationToken ct)
		{
			var result = await memberService.UpdateAsync(id, request, ct);

			if (result.IsFailure)
			{
				return result.Error.ToProblem();
			}

			return TypedResults.Ok(result.Value);
		}

		private static async Task<Results<Ok<MemberResponse>, ProblemHttpResult>> Suspend(
			Guid id,
			SuspendMemberRequest request,
			IMemberService memberService,
			CancellationToken ct)
		{
			var result = await memberService.SuspendAsync(id, request, ct);

			if (result.IsFailure)
			{
				return result.Error.ToProblem();
			}

			return TypedResults.Ok(result.Value);
		}

		private static async Task<Results<Ok<MemberResponse>, ProblemHttpResult>> Reactivate(
			Guid id,
			IMemberService memberService,
			CancellationToken ct)
		{
			var result = await memberService.ReactivateAsync(id, ct);

			if (result.IsFailure)
			{
				return result.Error.ToProblem();
			}

			return TypedResults.Ok(result.Value);
		}

		private static async Task<Results<Ok<MemberResponse>, ProblemHttpResult>> Renew(
			Guid id,
			IMemberService memberService,
			CancellationToken ct)
		{
			var result = await memberService.RenewAsync(id, ct);

			if (result.IsFailure)
			{
				return result.Error.ToProblem();
			}

			return TypedResults.Ok(result.Value);
		}

		private static async Task<Results<NoContent, ProblemHttpResult>> Delete(
			Guid id,
			IMemberService memberService,
			CancellationToken ct)
		{
			var result = await memberService.DeleteAsync(id, ct);

			if (result.IsFailure)
			{
				return result.Error.ToProblem();
			}

			return TypedResults.NoContent();
		}
	}
}