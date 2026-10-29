using LibraryManagementSystemAPI.Entities;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystemAPI.DataAccess
{
	public class LibraryDbContext(DbContextOptions<LibraryDbContext> options)
	: DbContext(options)
	{
		public DbSet<Category> Categories => Set<Category>();
		public DbSet<Author> Authors => Set<Author>();
		public DbSet<Book> Books => Set<Book>();
		public DbSet<BookAuthor> BookAuthors => Set<BookAuthor>();

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			// Finds every IEntityTypeConfiguration class in this project
			// (CategoryConfiguration, AuthorConfiguration, ...) and applies it.
			modelBuilder.ApplyConfigurationsFromAssembly(typeof(LibraryDbContext).Assembly);
		}

		// All SaveChanges / SaveChangesAsync calls end up in these two methods,
		// so the audit rules run on every save.
		public override int SaveChanges(bool acceptAllChangesOnSuccess)
		{
			ApplyAuditRules();
			return base.SaveChanges(acceptAllChangesOnSuccess);
		}

		public override Task<int> SaveChangesAsync(
			bool acceptAllChangesOnSuccess,
			CancellationToken cancellationToken = default)
		{
			ApplyAuditRules();
			return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
		}

		private void ApplyAuditRules()
		{
			var now = DateTime.UtcNow;

			foreach (var entry in ChangeTracker.Entries<BaseEntity>())
			{
				switch (entry.State)
				{
					case EntityState.Added:
						entry.Entity.CreatedAt = now;
						break;

					case EntityState.Modified:
						entry.Entity.UpdatedAt = now;
						// Protect CreatedAt from being changed by an update.
						entry.Property(e => e.CreatedAt).IsModified = false;
						break;

					case EntityState.Deleted:
						// Turn a real DELETE into a soft delete (an UPDATE).
						entry.State = EntityState.Modified;
						entry.Entity.IsDeleted = true;
						entry.Entity.DeletedAt = now;
						break;
				}
			}
		}
	}
}
