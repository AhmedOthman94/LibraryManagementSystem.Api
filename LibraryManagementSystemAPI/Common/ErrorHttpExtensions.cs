using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagementSystemAPI.Common
{
	public static class ErrorHttpExtensions
	{
		public static int ToStatusCode(this Error error) => error.Type switch
		{
			ErrorType.Validation => StatusCodes.Status400BadRequest,
			ErrorType.NotFound => StatusCodes.Status404NotFound,
			ErrorType.Conflict => StatusCodes.Status409Conflict,
			_ => StatusCodes.Status500InternalServerError
		};

		public static ProblemHttpResult ToProblem(this Error error) =>
			TypedResults.Problem(
				detail: error.Message,
				statusCode: error.ToStatusCode()
			);

		public static ObjectResult ToProblem(this ControllerBase controller, Error error) =>
			controller.Problem(
				detail: error.Message,
				statusCode: error.ToStatusCode()
			);
	}
}