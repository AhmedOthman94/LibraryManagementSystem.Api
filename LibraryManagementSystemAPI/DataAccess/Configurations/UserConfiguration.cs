using LibraryManagementSystemAPI.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LibraryManagementSystemAPI.DataAccess.Configurations
{
	public class UserConfiguration : IEntityTypeConfiguration<User>
	{
		public void Configure(EntityTypeBuilder<User> builder)
		{
			builder.ToTable("Users");

			builder.HasKey(u => u.Id);

			builder.Property(u => u.Email)
				.HasMaxLength(256)
				.IsRequired();

			builder.Property(u => u.PasswordHash)
				.HasMaxLength(500)
				.IsRequired();

			builder.Property(u => u.Role)
				.HasConversion<string>()
				.HasMaxLength(20)
				.IsRequired();

			// One-to-one (optional): a member has at most one login account
			builder.HasOne(u => u.Member)
				.WithOne()
				.HasForeignKey<User>(u => u.MemberId)
				.OnDelete(DeleteBehavior.Restrict);

			builder.HasIndex(u => u.Email)
				.IsUnique()
				.HasFilter("\"IsDeleted\" = false");

			// Two accounts can't point to the same member
			builder.HasIndex(u => u.MemberId)
				.IsUnique()
				.HasFilter("\"MemberId\" IS NOT NULL AND \"IsDeleted\" = false");

			builder.HasQueryFilter(u => !u.IsDeleted);
		}
	}
}