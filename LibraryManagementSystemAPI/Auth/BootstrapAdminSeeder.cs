using LibraryManagementSystemAPI.Common;
using LibraryManagementSystemAPI.DataAccess;
using LibraryManagementSystemAPI.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystemAPI.Auth
{
	public static class BootstrapAdminSeeder
	{
		// Creates the first Admin if none exists. Safe to run on every startup.
		public static async Task SeedBootstrapAdminAsync(this WebApplication app)
		{
			var email = app.Configuration["BootstrapAdmin:Email"];
			var password = app.Configuration["BootstrapAdmin:Password"];

			// Not configured (tests, production after setup): do nothing
			if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
			{
				return;
			}

			using var scope = app.Services.CreateScope();
			var db = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
			var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>();

			if (await db.Users.AnyAsync(u => u.Role == UserRole.Admin))
			{
				return; // already done: idempotent
			}

			var admin = new User
			{
				Email = EmailHelper.Normalize(email),
				PasswordHash = string.Empty,
				Role = UserRole.Admin
			};

			admin.PasswordHash = passwordHasher.HashPassword(admin, password);

			db.Users.Add(admin);
			await db.SaveChangesAsync();

			app.Logger.LogInformation("Bootstrap admin {Email} was created.", admin.Email);
		}
	}
}