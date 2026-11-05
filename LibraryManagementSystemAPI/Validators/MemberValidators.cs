using FluentValidation;
using LibraryManagementSystemAPI.Dtos;
using LibraryManagementSystemAPI.Entities;

namespace LibraryManagementSystemAPI.Validators
{
	public class MemberRequestRules : AbstractValidator<IMemberRequest>
	{
		public MemberRequestRules()
		{
			RuleFor(x => x.FirstName)
				.NotEmpty().WithMessage("First name is required.")
				.MaximumLength(100).WithMessage("First name can't be longer than 100 characters.");

			RuleFor(x => x.LastName)
				.NotEmpty().WithMessage("Last name is required.")
				.MaximumLength(100).WithMessage("Last name can't be longer than 100 characters.");

			RuleFor(x => x.Email)
				.Cascade(CascadeMode.Stop)
				.NotEmpty().WithMessage("Email is required.")
				.MaximumLength(256).WithMessage("Email can't be longer than 256 characters.")
				.EmailAddress().WithMessage("Email is not valid.");

			RuleFor(x => x.PhoneNumber)
				.MaximumLength(30).WithMessage("Phone number can't be longer than 30 characters.")
				.Matches(@"^\+?[0-9\s\-()]{7,30}$").WithMessage("Phone number is not valid.")
				.When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));
		}
	}

	public class CreateMemberRequestValidator : AbstractValidator<CreateMemberRequest>
	{
		public CreateMemberRequestValidator()
		{
			Include(new MemberRequestRules());
		}
	}

	public class UpdateMemberRequestValidator : AbstractValidator<UpdateMemberRequest>
	{
		public UpdateMemberRequestValidator()
		{
			Include(new MemberRequestRules());
		}
	}

	public class SuspendMemberRequestValidator : AbstractValidator<SuspendMemberRequest>
	{
		public SuspendMemberRequestValidator()
		{
			RuleFor(x => x.Reason)
				.NotEmpty().WithMessage("A suspension reason is required.")
				.MaximumLength(500).WithMessage("Reason can't be longer than 500 characters.");
		}
	}

	public class MemberQueryParametersValidator : AbstractValidator<MemberQueryParameters>
	{
		public MemberQueryParametersValidator()
		{
			RuleFor(x => x.Search)
				.MaximumLength(100).WithMessage("Search can't be longer than 100 characters.");

			RuleFor(x => x.Status)
				.Must(status => status is null
					|| Enum.GetNames<MemberStatus>().Contains(status, StringComparer.OrdinalIgnoreCase))
				.WithMessage($"Status must be one of: {string.Join(", ", Enum.GetNames<MemberStatus>())}.");

			RuleFor(x => x.Page).ValidPage();
			RuleFor(x => x.PageSize).ValidPageSize();
		}
	}
}