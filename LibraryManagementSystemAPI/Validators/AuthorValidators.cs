using FluentValidation;
using LibraryManagementSystemAPI.Dtos;

namespace LibraryManagementSystemAPI.Validators
{
	public class AuthorRequestRules : AbstractValidator<IAuthorRequest>
	{
		public AuthorRequestRules()
		{
			RuleFor(x => x.FirstName)
				.NotEmpty().WithMessage("First name is required.")
				.MaximumLength(100).WithMessage("First name can't be longer than 100 characters.");

			RuleFor(x => x.LastName)
				.NotEmpty().WithMessage("Last name is required.")
				.MaximumLength(100).WithMessage("Last name can't be longer than 100 characters.");

			RuleFor(x => x.Biography)
				.MaximumLength(2000).WithMessage("Biography can't be longer than 2000 characters.");

			RuleFor(x => x.Nationality)
				.MaximumLength(100).WithMessage("Nationality can't be longer than 100 characters.");

			RuleFor(x => x.ImageUrl)
				.MaximumLength(500).WithMessage("Image URL can't be longer than 500 characters.")
				.Must(BeAValidHttpUrl).WithMessage("Image URL must be a valid http or https link.")
				.When(x => !string.IsNullOrWhiteSpace(x.ImageUrl));

			RuleFor(x => x.DateOfBirth)
				.Must(date => date is null || date <= Today())
				.WithMessage("Date of birth can't be in the future.");

			RuleFor(x => x.DateOfDeath)
				.Must(date => date is null || date <= Today())
				.WithMessage("Date of death can't be in the future.");

			RuleFor(x => x.DateOfDeath)
				.Must((request, dateOfDeath) => dateOfDeath >= request.DateOfBirth)
				.When(x => x.DateOfBirth.HasValue && x.DateOfDeath.HasValue)
				.WithMessage("Date of death can't be before date of birth.");
		}

		private static DateOnly Today() =>
			DateOnly.FromDateTime(DateTime.UtcNow);

		private static bool BeAValidHttpUrl(string? url) =>
			Uri.TryCreate(url, UriKind.Absolute, out var uri)
			&& (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
	}

	public class CreateAuthorRequestValidator : AbstractValidator<CreateAuthorRequest>
	{
		public CreateAuthorRequestValidator()
		{
			Include(new AuthorRequestRules());
		}
	}

	public class UpdateAuthorRequestValidator : AbstractValidator<UpdateAuthorRequest>
	{
		public UpdateAuthorRequestValidator()
		{
			Include(new AuthorRequestRules());
		}
	}
}