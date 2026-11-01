using FluentValidation;
using LibraryManagementSystemAPI.DataAccess;
using LibraryManagementSystemAPI.Endpoints;
using LibraryManagementSystemAPI.ErrorHandling;
using LibraryManagementSystemAPI.Filters;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// ---------- Services ----------

builder.Services.AddOpenApi();

builder.Services.AddControllers(options =>
{
	options.Filters.Add<FluentValidationActionFilter>();
});

builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// Every ProblemDetails in the API gets "instance" and "traceId"
builder.Services.AddProblemDetails(options =>
{
	options.CustomizeProblemDetails = context =>
	{
		var request = context.HttpContext.Request;

		context.ProblemDetails.Instance = $"{request.Method} {request.Path}";
		context.ProblemDetails.Extensions.TryAdd(
			"traceId",
			ErrorHandlingHelpers.GetTraceId(context.HttpContext)
		);
	};
});

// ORDER MATTERS: specific handlers first, the global one LAST
builder.Services.AddExceptionHandler<DatabaseExceptionHandler>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddDbContext<LibraryDbContext>(options =>
{
	options.UseNpgsql(builder.Configuration.GetConnectionString("LibraryDb"));
});

var app = builder.Build();

// ---------- Pipeline (the "doors") ----------

// FIRST door: catches exceptions from everything after it
app.UseExceptionHandler();

// Turns empty error responses (unknown route 404, 405...) into ProblemDetails
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
	app.MapOpenApi();
	app.MapScalarApiReference();

	// TEMPORARY: only for testing the global handler. Delete after testing!
	app.MapGet("/api/debug/throw", () =>
	{
		throw new InvalidOperationException("This is a test exception.");
	}).ExcludeFromDescription();
}

app.UseHttpsRedirection();

app.MapCategoryEndpoints();
app.MapBookEndpoints();
app.MapControllers();

app.Run();