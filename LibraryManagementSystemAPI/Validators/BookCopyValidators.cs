using FluentValidation;
using LibraryManagementSystemAPI.Dtos;
using LibraryManagementSystemAPI.Entities;

namespace LibraryManagementSystemAPI.Validators
{
	public class BookCopyRequestRules : AbstractValidator<IBookCopyRequest>
	{
		public BookCopyRequestRules()
		{
			RuleFor(x => x.Barcode)
				.Cascade(CascadeMode.Stop)
				.NotEmpty().WithMessage("Barcode is required.")
				.MaximumLength(50).WithMessage("Barcode can't be longer than 50 characters.")
				.Matches("^[A-Za-z0-9-]+$").WithMessage("Barcode can only contain letters, digits, and hyphens.");

			RuleFor(x => x.ShelfLocation)
				.MaximumLength(50).WithMessage("Shelf location can't be longer than 50 characters.");

			RuleFor(x => x.AcquiredOn)
				.Must(date => date is null || date <= DateOnly.FromDateTime(DateTime.UtcNow))
				.WithMessage("Acquired date can't be in the future.");
		}
	}

	public class CreateBookCopyRequestValidator : AbstractValidator<CreateBookCopyRequest>
	{
		public CreateBookCopyRequestValidator()
		{
			Include(new BookCopyRequestRules());
		}
	}

	public class UpdateBookCopyRequestValidator : AbstractValidator<UpdateBookCopyRequest>
	{
		public UpdateBookCopyRequestValidator()
		{
			Include(new BookCopyRequestRules());
		}
	}

	public class ChangeCopyStatusRequestValidator : AbstractValidator<ChangeCopyStatusRequest>
	{
		public ChangeCopyStatusRequestValidator()
		{
			RuleFor(x => x.Status)
				.Cascade(CascadeMode.Stop)
				.IsInEnum().WithMessage("Status is not a valid value.")
				.NotEqual(CopyStatus.OnLoan).WithMessage("OnLoan is set automatically when a copy is borrowed.");
		}
	}
}