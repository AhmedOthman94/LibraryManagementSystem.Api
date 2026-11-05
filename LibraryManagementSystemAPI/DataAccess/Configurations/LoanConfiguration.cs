using LibraryManagementSystemAPI.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LibraryManagementSystemAPI.DataAccess.Configurations
{
	public class LoanConfiguration : IEntityTypeConfiguration<Loan>
	{
		public void Configure(EntityTypeBuilder<Loan> builder)
		{
			builder.ToTable("Loans", table =>
			{
				table.HasCheckConstraint(
					"CK_Loans_DueOn_After_BorrowedOn",
					"\"DueOn\" >= \"BorrowedOn\""
				);
			});

			builder.HasKey(l => l.Id);

			// Money: exact decimal, 10 digits total, 2 after the point
			builder.Property(l => l.FineAmount)
				.HasPrecision(10, 2);

			builder.HasOne(l => l.Member)
				.WithMany()
				.HasForeignKey(l => l.MemberId)
				.OnDelete(DeleteBehavior.Restrict);

			builder.HasOne(l => l.BookCopy)
				.WithMany()
				.HasForeignKey(l => l.BookCopyId)
				.OnDelete(DeleteBehavior.Restrict);

			// Layer 2 of concurrency protection:
			// a copy can have only ONE loan that is not returned yet
			builder.HasIndex(l => l.BookCopyId)
				.IsUnique()
				.HasFilter("\"ReturnedOn\" IS NULL AND \"IsDeleted\" = false")
				.HasDatabaseName("IX_Loans_OneActiveLoanPerCopy");

			// Fast answers to "how many open loans does this member have?"
			builder.HasIndex(l => new { l.MemberId, l.ReturnedOn });

			builder.HasQueryFilter(l => !l.IsDeleted);
		}
	}
}