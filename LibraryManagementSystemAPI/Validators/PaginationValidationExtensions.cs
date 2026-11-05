using FluentValidation;
using LibraryManagementSystemAPI.Common;

namespace LibraryManagementSystemAPI.Validators
{
	public static class PaginationValidationExtensions
	{
		public static IRuleBuilderOptions<T, int?> ValidPage<T>(this IRuleBuilder<T, int?> rule) =>
			rule
				.Must(page => page is null || page >= 1)
				.WithMessage("Page must be 1 or greater.");

		public static IRuleBuilderOptions<T, int?> ValidPageSize<T>(this IRuleBuilder<T, int?> rule) =>
			rule
				.Must(size => size is null || (size >= 1 && size <= PaginationDefaults.MaxPageSize))
				.WithMessage($"Page size must be between 1 and {PaginationDefaults.MaxPageSize}.");
	}
}