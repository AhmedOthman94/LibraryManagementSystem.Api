using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

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
					context.Result = new BadRequestObjectResult(
						new ValidationProblemDetails(result.ToDictionary())
						{
							Status = StatusCodes.Status400BadRequest
						}
					);
					return; 
				}
			}

			await next(); 
		}
	}
}