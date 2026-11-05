using FluentValidation;
using LibraryManagementSystemAPI.Dtos;

namespace LibraryManagementSystemAPI.Validators
{
	public class BookQueryParametersValidator : AbstractValidator<BookQueryParameters>
	{
		public BookQueryParametersValidator()
		{
			RuleFor(x => x.Search)
				.MaximumLength(100).WithMessage("Search can't be longer than 100 characters.");

			RuleFor(x => x.Language)
				.MaximumLength(50).WithMessage("Language can't be longer than 50 characters.");

			RuleFor(x => x.PublishedTo)
				.Must((parameters, to) => to >= parameters.PublishedFrom)
				.When(x => x.PublishedFrom.HasValue && x.PublishedTo.HasValue)
				.WithMessage("PublishedTo can't be before PublishedFrom.");

			RuleFor(x => x.SortBy)
				.Must(sortBy => sortBy is null
					|| BookQueryParameters.AllowedSortFields.Contains(sortBy, StringComparer.OrdinalIgnoreCase))
				.WithMessage($"SortBy must be one of: {string.Join(", ", BookQueryParameters.AllowedSortFields)}.");

			RuleFor(x => x.SortDirection)
				.Must(direction => direction is null
					|| BookQueryParameters.AllowedSortDirections.Contains(direction, StringComparer.OrdinalIgnoreCase))
				.WithMessage("SortDirection must be 'asc' or 'desc'.");

			RuleFor(x => x.Page).ValidPage();
			RuleFor(x => x.PageSize).ValidPageSize();
		}
	}
}