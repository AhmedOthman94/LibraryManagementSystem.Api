using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagementSystemAPI.ErrorHandling
{
	public class GlobalExceptionHandler(
		IProblemDetailsService problemDetailsService,
		IHostEnvironment environment,
		ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
	{
		public async ValueTask<bool> TryHandleAsync(
			HttpContext httpContext,
			Exception exception,
			CancellationToken cancellationToken)
		{
			var traceId = ErrorHandlingHelpers.GetTraceId(httpContext);

			if (exception is OperationCanceledException
				&& httpContext.RequestAborted.IsCancellationRequested)
			{
				logger.LogInformation(
					"Request was cancelled by the client. TraceId: {TraceId}", traceId);

				httpContext.Response.StatusCode = StatusCodes.Status499ClientClosedRequest;
				return true;
			}

			if (exception is BadHttpRequestException badRequest)
			{
				logger.LogWarning(
					"Bad request: {Message}. TraceId: {TraceId}", badRequest.Message, traceId);

				var badRequestProblem = new ProblemDetails
				{
					Status = badRequest.StatusCode,
					Detail = environment.IsDevelopment()
						? badRequest.Message
						: "The request is invalid or badly formatted."
				};

				return await ErrorHandlingHelpers.WriteProblemAsync(
					httpContext, problemDetailsService, badRequestProblem, exception);
			}

			logger.LogError(
				exception, "Unhandled exception. TraceId: {TraceId}", traceId);

			var problem = new ProblemDetails
			{
				Status = StatusCodes.Status500InternalServerError,
				Title = "An unexpected error occurred.",
				Detail = environment.IsDevelopment()
					? exception.ToString()
					: "Please try again later. If the problem continues, contact support with the traceId."
			};

			return await ErrorHandlingHelpers.WriteProblemAsync(
				httpContext, problemDetailsService, problem, exception);
		}
	}
}