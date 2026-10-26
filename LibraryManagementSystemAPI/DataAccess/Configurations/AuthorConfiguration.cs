using LibraryManagementSystemAPI.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LibraryManagementSystemAPI.DataAccess.Configurations
{
	public class AuthorConfiguration : IEntityTypeConfiguration<Author>
	{
		public void Configure(EntityTypeBuilder<Author> builder)
		{
			builder.ToTable("Authors", table =>
			{
				table.HasCheckConstraint(
					"CK_Authors_DateOfDeath_After_DateOfBirth",
					"\"DateOfDeath\" IS NULL OR \"DateOfBirth\" IS NULL OR \"DateOfDeath\" >= \"DateOfBirth\""
				);
			});

			builder.HasKey(a => a.Id);

			builder.Property(a => a.FirstName)
				.HasMaxLength(100)
				.IsRequired();

			builder.Property(a => a.LastName)
				.HasMaxLength(100)
				.IsRequired();

			builder.Property(a => a.Biography)
				.HasMaxLength(2000);

			builder.Property(a => a.Nationality)
				.HasMaxLength(100);

			builder.Property(a => a.ImageUrl)
				.HasMaxLength(500);

			builder.HasIndex(a => new { a.LastName, a.FirstName });

			builder.HasQueryFilter(a => !a.IsDeleted);
		}
	}
}
