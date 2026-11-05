using LibraryManagementSystemAPI.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LibraryManagementSystemAPI.DataAccess.Configurations
{
	public class MemberConfiguration : IEntityTypeConfiguration<Member>
	{
		public void Configure(EntityTypeBuilder<Member> builder)
		{
			builder.ToTable("Members");

			builder.HasKey(m => m.Id);

			// The database fills this from the sequence on INSERT
			builder.Property(m => m.MembershipNumber)
				.HasDefaultValueSql("nextval('\"MembershipNumbers\"')");

			// A card number must never change after it's created
			builder.Property(m => m.MembershipNumber)
				.Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Throw);

			builder.HasIndex(m => m.MembershipNumber)
				.IsUnique();

			builder.Property(m => m.FirstName)
				.HasMaxLength(100)
				.IsRequired();

			builder.Property(m => m.LastName)
				.HasMaxLength(100)
				.IsRequired();

			builder.Property(m => m.Email)
				.HasMaxLength(256)
				.IsRequired();

			builder.Property(m => m.PhoneNumber)
				.HasMaxLength(30);

			builder.Property(m => m.Status)
				.HasConversion<string>()
				.HasMaxLength(20)
				.IsRequired();

			builder.Property(m => m.SuspensionReason)
				.HasMaxLength(500);

			builder.HasIndex(m => m.Email)
				.IsUnique()
				.HasFilter("\"IsDeleted\" = false");

			builder.HasIndex(m => new { m.LastName, m.FirstName });

			builder.HasQueryFilter(m => !m.IsDeleted);
		}
	}
}