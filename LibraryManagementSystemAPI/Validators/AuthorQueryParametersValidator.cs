using FluentValidation;
using LibraryManagementSystemAPI.Dtos;

namespace LibraryManagementSystemAPI.Validators
{
	public class AuthorQueryParametersValidator : AbstractValidator<AuthorQueryParameters>
	{
		public AuthorQueryParametersValidator()
		{
			RuleFor(x => x.Search)
				.MaximumLength(100).WithMessage("Search can't be longer than 100 characters.");

			RuleFor(x => x.Nationality)
				.MaximumLength(100).WithMessage("Nationality can't be longer than 100 characters.");

			RuleFor(x => x.Page).ValidPage();
			RuleFor(x => x.PageSize).ValidPageSize();
		}
	}
}