using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace LibraryManagementSystemAPI.Filters
{
	// Runs BEFORE every controller action.
	// For each argument that has a validator, it validates it.
	public class FluentValidationActionFilter : IAsyncActionFilter
	{
		public async Task OnActionExecutionAsync(
			ActionExecutingContext context,
			ActionExecutionDelegate next)
		{
			foreach (var argument in context.ActionArguments.Values)
			{
				if (argument is null)
				{
					continue;
				}

				// Build the type IValidator<ArgumentType>, e.g. IValidator<CreateAuthorRequest>
				var validatorType = typeof(IValidator<>).MakeGenericType(argument.GetType());

				// Arguments like Guid or CancellationToken have no validator: skip them
				if (context.HttpContext.RequestServices.GetService(validatorType) is not IValidator validator)
				{
					continue;
				}

				var result = await validator.ValidateAsync(
					new ValidationContext<object>(argument),
					context.HttpContext.RequestAborted
				);

				if (!result.IsValid)
				{
					context.Result = new BadRequestObjectResult(
						new ValidationProblemDetails(result.ToDictionary())
						{
							Status = StatusCodes.Status400BadRequest
						}
					);
					return; // Stop: the action will NOT run
				}
			}

			await next(); // All valid: run the action
		}
	}
}