using LibraryManagementSystemAPI.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LibraryManagementSystemAPI.DataAccess.Configurations
{
	public class BookCopyConfiguration : IEntityTypeConfiguration<BookCopy>
	{
		public void Configure(EntityTypeBuilder<BookCopy> builder)
		{
			builder.ToTable("BookCopies");

			builder.HasKey(c => c.Id);

			builder.Property(c => c.Barcode)
				.HasMaxLength(50)
				.IsRequired();

			// Save the enum as its NAME ("Available"), not a number (0)
			builder.Property(c => c.Status)
				.HasConversion<string>()
				.HasMaxLength(20)
				.IsRequired();

			// Optimistic concurrency: EF updates the copy only if nobody changed it since we read it
			builder.Property(c => c.Version)
				.IsRowVersion();

			builder.Property(c => c.ShelfLocation)
				.HasMaxLength(50);

			builder.HasOne(c => c.Book)
				.WithMany(b => b.Copies)
				.HasForeignKey(c => c.BookId)
				.OnDelete(DeleteBehavior.Restrict);

			// Every active barcode is unique
			builder.HasIndex(c => c.Barcode)
				.IsUnique()
				.HasFilter("\"IsDeleted\" = false");

			// Fast answers to "how many available copies does this book have?"
			builder.HasIndex(c => new { c.BookId, c.Status });

			builder.HasQueryFilter(c => !c.IsDeleted);
		}
	}
}