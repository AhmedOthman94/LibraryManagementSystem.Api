using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace LibraryManagementSystemAPI.ErrorHandling
{
	public class DatabaseExceptionHandler(
		IProblemDetailsService problemDetailsService,
		IHostEnvironment environment,
		ILogger<DatabaseExceptionHandler> logger) : IExceptionHandler
	{
		private static readonly Dictionary<string, string> FriendlyMessages = new()
		{
			["IX_Books_Isbn"] = "A book with this ISBN already exists.",
			["IX_Categories_Name"] = "A category with this name already exists.",
			["IX_BookCopies_Barcode"] = "A copy with this barcode already exists."
		};

		public async ValueTask<bool> TryHandleAsync(
			HttpContext httpContext,
			Exception exception,
			CancellationToken cancellationToken)
		{
			if (exception is not DbUpdateException { InnerException: PostgresException postgresError })
			{
				return false;
			}

			var (statusCode, detail) = postgresError.SqlState switch
			{
				PostgresErrorCodes.UniqueViolation => (
					StatusCodes.Status409Conflict,
					GetFriendlyMessage(postgresError.ConstraintName) ?? "This record already exists."
				),

				PostgresErrorCodes.ForeignKeyViolation => (
					StatusCodes.Status409Conflict,
					"This action conflicts with related data. It may point to data that was just deleted, or be used by other data."
				),

				PostgresErrorCodes.CheckViolation => (
					StatusCodes.Status400BadRequest,
					"The data breaks one of the database rules."
				),

				PostgresErrorCodes.StringDataRightTruncation => (
					StatusCodes.Status400BadRequest,
					"A text value is longer than allowed."
				),

				_ => (0, "")
			};

			if (statusCode == 0)
			{
				return false;
			}

			logger.LogWarning(
				exception,
				"Database rule {Constraint} ({SqlState}) was violated. TraceId: {TraceId}",
				postgresError.ConstraintName,
				postgresError.SqlState,
				ErrorHandlingHelpers.GetTraceId(httpContext)
			);

			var problem = new ProblemDetails
			{
				Status = statusCode,
				Detail = detail
			};

			if (environment.IsDevelopment())
			{
				problem.Extensions["constraint"] = postgresError.ConstraintName;
			}

			return await ErrorHandlingHelpers.WriteProblemAsync(
				httpContext, problemDetailsService, problem, exception);
		}

		private static string? GetFriendlyMessage(string? constraintName) =>
			constraintName is not null && FriendlyMessages.TryGetValue(constraintName, out var message)
				? message
				: null;
	}
}