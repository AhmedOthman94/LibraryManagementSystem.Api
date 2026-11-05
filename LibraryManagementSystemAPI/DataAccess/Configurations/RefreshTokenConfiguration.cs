using LibraryManagementSystemAPI.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LibraryManagementSystemAPI.DataAccess.Configurations
{
	public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
	{
		public void Configure(EntityTypeBuilder<RefreshToken> builder)
		{
			builder.ToTable("RefreshTokens");

			builder.HasKey(t => t.Id);

			// SHA-256 as hex = exactly 64 characters
			builder.Property(t => t.TokenHash)
				.HasMaxLength(64)
				.IsRequired();

			builder.Property(t => t.RevokedReason)
				.HasMaxLength(50);

			builder.Property(t => t.Version)
				.IsRowVersion();

			builder.HasOne(t => t.User)
				.WithMany()
				.HasForeignKey(t => t.UserId)
				.OnDelete(DeleteBehavior.Cascade);

			// Every refresh request looks up a token by its hash
			builder.HasIndex(t => t.TokenHash)
				.IsUnique();

			// "Revoke this whole session" looks up by family
			builder.HasIndex(t => t.FamilyId);

			// Tokens of deleted users disappear: their sessions die automatically
			builder.HasQueryFilter(t => !t.User.IsDeleted);
		}
	}
}