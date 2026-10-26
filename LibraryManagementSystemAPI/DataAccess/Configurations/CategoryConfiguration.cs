using LibraryManagementSystemAPI.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LibraryManagementSystemAPI.DataAccess.Configurations
{
	public class CategoryConfiguration : IEntityTypeConfiguration<Category>
	{
		public void Configure(EntityTypeBuilder<Category> builder)
		{
			builder.ToTable("Categories");

			builder.HasKey(c => c.Id);

			builder.Property(c => c.Name)
				.HasMaxLength(100)
				.IsRequired();

			builder.Property(c => c.Description)
				.HasMaxLength(500);

			builder.HasIndex(c => c.Name)
				.IsUnique()
				.HasFilter("\"IsDeleted\" = false");

			builder.HasQueryFilter(c => !c.IsDeleted);
		}
	}
}
