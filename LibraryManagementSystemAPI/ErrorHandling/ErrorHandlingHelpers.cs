using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagementSystemAPI.ErrorHandling
{
	public static class ErrorHandlingHelpers
	{
		public static string GetTraceId(HttpContext httpContext) =>
			Activity.Current?.Id ?? httpContext.TraceIdentifier;

		public static async ValueTask<bool> WriteProblemAsync(
			HttpContext httpContext,
			IProblemDetailsService problemDetailsService,
			ProblemDetails problem,
			Exception exception)
		{
			httpContext.Response.StatusCode =
				problem.Status ?? StatusCodes.Status500InternalServerError;

			var written = await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
			{
				HttpContext = httpContext,
				ProblemDetails = problem,
				Exception = exception
			});

			if (!written)
			{
				problem.Extensions.TryAdd("traceId", GetTraceId(httpContext));

				await httpContext.Response.WriteAsJsonAsync(
					problem,
					options: null,
					contentType: "application/problem+json"
				);
			}

			return true;
		}
	}
}