using System.Text.Json.Serialization;
using FluentValidation;
using LibraryManagementSystemAPI.Auth;
using LibraryManagementSystemAPI.Common;
using LibraryManagementSystemAPI.DataAccess;
using LibraryManagementSystemAPI.Endpoints;
using LibraryManagementSystemAPI.ErrorHandling;
using LibraryManagementSystemAPI.Filters;
using LibraryManagementSystemAPI.Services.Auth;
using LibraryManagementSystemAPI.Services.Authors;
using LibraryManagementSystemAPI.Services.BookCopies;
using LibraryManagementSystemAPI.Services.Books;
using LibraryManagementSystemAPI.Services.Categories;
using LibraryManagementSystemAPI.Services.Loans;
using LibraryManagementSystemAPI.Services.Members;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// ---------- Services ----------

builder.Services.AddOpenApi();

builder.Services.AddControllers(options =>
{
	options.Filters.Add<FluentValidationActionFilter>();
})
.AddJsonOptions(options =>
{
	// Controllers: enums as "Available" instead of 0
	options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

// Minimal APIs: enums as "Available" instead of 0
builder.Services.ConfigureHttpJsonOptions(options =>
{
	options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
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

// Business settings from appsettings.json, checked when the app starts
builder.Services
	.AddOptions<LibraryPolicyOptions>()
	.Bind(builder.Configuration.GetSection(LibraryPolicyOptions.SectionName))
	.ValidateDataAnnotations()
	.ValidateOnStart();

// The real clock (tests will replace it with a fake one)
builder.Services.AddSingleton(TimeProvider.System);

// Scoped = one instance per HTTP request, same lifetime as LibraryDbContext
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IAuthorService, AuthorService>();
builder.Services.AddScoped<IBookService, BookService>();
builder.Services.AddScoped<IBookCopyService, BookCopyService>();
builder.Services.AddScoped<IMemberService, MemberService>();
builder.Services.AddScoped<ILoanService, LoanService>();

builder.Services.AddLibraryAuth(builder.Configuration);
builder.Services.AddScoped<IAuthService, AuthService>();

var app = builder.Build();

// ---------- Pipeline (the "doors") ----------

// FIRST door: catches exceptions from everything after it
app.UseExceptionHandler();

// Turns empty error responses (unknown route 404, 405...) into ProblemDetails
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
	app.MapOpenApi().AllowAnonymous();
	app.MapScalarApiReference().AllowAnonymous();

	// TEMPORARY: only for testing the global handler. Delete after testing!
	app.MapGet("/api/debug/throw", () =>
	{
		throw new InvalidOperationException("This is a test exception.");
	}).ExcludeFromDescription();
}

app.UseHttpsRedirection();

app.UseAuthentication();   // read the token → fill HttpContext.User
app.UseAuthorization();  // check policies for the endpoint

app.MapCategoryEndpoints();
app.MapBookEndpoints();
app.MapBookCopyEndpoints();
app.MapMemberEndpoints();
app.MapLoanEndpoints();
app.MapAuthEndpoints();
app.MapControllers();

await app.SeedBootstrapAdminAsync();

app.Run();