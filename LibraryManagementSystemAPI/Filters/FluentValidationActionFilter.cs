using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace LibraryManagementSystemAPI.Filters
{
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

				var validatorType = typeof(IValidator<>).MakeGenericType(argument.GetType());

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
					// Put the FluentValidation errors into MVC's ModelState format
					var modelState = new ModelStateDictionary();

					foreach (var error in result.Errors)
					{
						modelState.AddModelError(error.PropertyName, error.ErrorMessage);
					}

					// Let .NET build the response, so it gets traceId and instance like every other error
					var problemDetailsFactory = context.HttpContext.RequestServices
						.GetRequiredService<ProblemDetailsFactory>();

					var problem = problemDetailsFactory.CreateValidationProblemDetails(
						context.HttpContext,
						modelState,
						StatusCodes.Status400BadRequest
					);

					context.Result = new BadRequestObjectResult(problem);
					return;
				}
			}

			await next();
		}
	}
}