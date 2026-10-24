using LibraryManagementSystemAPI.Entities;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystemAPI.DataAccess
{
	public class LibraryDbContext(DbContextOptions<LibraryDbContext> options)
	: DbContext(options)
	{
		public DbSet<Category> Categories => Set<Category>();
		public DbSet<Author> Authors => Set<Author>();
	}
}
