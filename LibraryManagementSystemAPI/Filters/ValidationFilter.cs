using FluentValidation;

namespace LibraryManagementSystemAPI.Filters
{
	public class ValidationFilter<T>(IValidator<T> validator) : IEndpointFilter
	{
		public async ValueTask<object?> InvokeAsync(
			EndpointFilterInvocationContext context,
			EndpointFilterDelegate next)
		{
			var request = context.Arguments.OfType<T>().FirstOrDefault();

			if (request is null)
			{
				return TypedResults.Problem(
					detail: "Request body is required.",
					statusCode: StatusCodes.Status400BadRequest
				);
			}

			var result = await validator.ValidateAsync(
				request,
				context.HttpContext.RequestAborted
			);

			if (!result.IsValid)
			{
				return TypedResults.ValidationProblem(result.ToDictionary());
			}

			return await next(context);
		}
	}

	public static class ValidationFilterExtensions
	{
		public static RouteHandlerBuilder WithValidation<T>(this RouteHandlerBuilder builder) =>
			builder
				.AddEndpointFilter<ValidationFilter<T>>()
				.ProducesValidationProblem();
	}
}