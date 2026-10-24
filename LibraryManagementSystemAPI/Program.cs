using LibraryManagementSystemAPI.DataAccess;
using LibraryManagementSystemAPI.Endpoints;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddControllers();

builder.Services.AddDbContext<LibraryDbContext>(options => 
{
	options.UseNpgsql(builder.Configuration.GetConnectionString("LibraryDb"));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.MapOpenApi();
	app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.MapCategoryEndpoints();
app.MapControllers();

app.Run();

