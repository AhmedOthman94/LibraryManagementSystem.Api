using FluentValidation;
using LibraryManagementSystemAPI.Common;
using LibraryManagementSystemAPI.Dtos;
using LibraryManagementSystemAPI.Entities;

namespace LibraryManagementSystemAPI.Validators
{
	public class BorrowBookRequestValidator : AbstractValidator<BorrowBookRequest>
	{
		public BorrowBookRequestValidator()
		{
			RuleFor(x => x.CardNumber)
				.Cascade(CascadeMode.Stop)
				.NotEmpty().WithMessage("Card number is required.")
				.MaximumLength(20).WithMessage("Card number can't be longer than 20 characters.")
				.Must(card => MembershipCard.TryParse(card, out _)).WithMessage("Card number is not valid.");

			RuleFor(x => x.Barcode)
				.Cascade(CascadeMode.Stop)
				.NotEmpty().WithMessage("Barcode is required.")
				.MaximumLength(50).WithMessage("Barcode can't be longer than 50 characters.")
				.Matches("^[A-Za-z0-9-]+$").WithMessage("Barcode can only contain letters, digits, and hyphens.");
		}
	}

	public class LoanQueryParametersValidator : AbstractValidator<LoanQueryParameters>
	{
		public LoanQueryParametersValidator()
		{
			RuleFor(x => x.Status)
				.Must(status => status is null
					|| Enum.GetNames<LoanStatus>().Contains(status, StringComparer.OrdinalIgnoreCase))
				.WithMessage($"Status must be one of: {string.Join(", ", Enum.GetNames<LoanStatus>())}.");

			RuleFor(x => x.Page).ValidPage();
			RuleFor(x => x.PageSize).ValidPageSize();
		}
	}
}