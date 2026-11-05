using FluentValidation;
using LibraryManagementSystemAPI.Common;
using LibraryManagementSystemAPI.Dtos;
using LibraryManagementSystemAPI.Entities;

namespace LibraryManagementSystemAPI.Validators
{
	public static class PasswordValidationExtensions
	{
		// Length over complexity (NIST guidance)
		public static IRuleBuilderOptions<T, string> ValidPassword<T>(this IRuleBuilder<T, string> rule) =>
			rule
				.NotEmpty().WithMessage("Password is required.")
				.MinimumLength(10).WithMessage("Password must be at least 10 characters.")
				.MaximumLength(128).WithMessage("Password can't be longer than 128 characters.");
	}

	public class LoginRequestValidator : AbstractValidator<LoginRequest>
	{
		public LoginRequestValidator()
		{
			RuleFor(x => x.Email)
				.NotEmpty().WithMessage("Email is required.")
				.MaximumLength(256);

			// No length rules here: just "not empty".
			// Telling attackers the password rules on login helps nobody.
			RuleFor(x => x.Password)
				.NotEmpty().WithMessage("Password is required.")
				.MaximumLength(128);
		}
	}

	public class RegisterMemberAccountRequestValidator : AbstractValidator<RegisterMemberAccountRequest>
	{
		public RegisterMemberAccountRequestValidator()
		{
			RuleFor(x => x.CardNumber)
				.Must(card => MembershipCard.TryParse(card, out _))
				.WithMessage("Card number is not valid.");

			RuleFor(x => x.Email)
				.Cascade(CascadeMode.Stop)
				.NotEmpty().WithMessage("Email is required.")
				.MaximumLength(256)
				.EmailAddress().WithMessage("Email is not valid.");

			RuleFor(x => x.Password).ValidPassword();
		}
	}

	public class CreateStaffUserRequestValidator : AbstractValidator<CreateStaffUserRequest>
	{
		public CreateStaffUserRequestValidator()
		{
			RuleFor(x => x.Email)
				.Cascade(CascadeMode.Stop)
				.NotEmpty().WithMessage("Email is required.")
				.MaximumLength(256)
				.EmailAddress().WithMessage("Email is not valid.");

			RuleFor(x => x.Password).ValidPassword();

			RuleFor(x => x.Role)
				.Cascade(CascadeMode.Stop)
				.IsInEnum().WithMessage("Role is not valid.")
				.NotEqual(UserRole.Member).WithMessage("Staff accounts must be Librarian or Admin.");
		}
	}
}