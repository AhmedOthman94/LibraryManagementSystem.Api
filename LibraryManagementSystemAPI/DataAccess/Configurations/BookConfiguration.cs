using LibraryManagementSystemAPI.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LibraryManagementSystemAPI.DataAccess.Configurations
{
	public class BookConfiguration : IEntityTypeConfiguration<Book>
	{
		public void Configure(EntityTypeBuilder<Book> builder)
		{
			builder.ToTable("Books", table =>
			{
				table.HasCheckConstraint(
					"CK_Books_PageCount_Positive",
					"\"PageCount\" IS NULL OR \"PageCount\" > 0"
				);
			});

			builder.HasKey(b => b.Id);

			builder.Property(b => b.Title)
				.HasMaxLength(200)
				.IsRequired();

			builder.Property(b => b.Description)
				.HasMaxLength(2000);

			builder.Property(b => b.Isbn)
				.HasMaxLength(13)
				.IsRequired();

			builder.Property(b => b.Publisher)
				.HasMaxLength(200);

			builder.Property(b => b.Language)
				.HasMaxLength(50);

			builder.Property(b => b.CoverImageUrl)
				.HasMaxLength(500);

			builder.HasOne(b => b.Category)
				.WithMany(c => c.Books)
				.HasForeignKey(b => b.CategoryId)
				.OnDelete(DeleteBehavior.Restrict);

			builder.HasIndex(b => b.Isbn)
				.IsUnique()
				.HasFilter("\"IsDeleted\" = false");

			builder.HasIndex(b => b.Title);

			builder.HasQueryFilter(b => !b.IsDeleted);
		}
	}
}