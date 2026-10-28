using FluentValidation;
using LibraryManagementSystemAPI.Dtos;

namespace LibraryManagementSystemAPI.Validators
{
	public class CategoryRequestRules : AbstractValidator<ICategoryRequest>
	{
		public CategoryRequestRules()
		{
			RuleFor(x => x.Name)
				.NotEmpty().WithMessage("Category name is required.")
				.MaximumLength(100).WithMessage("Category name can't be longer than 100 characters.");

			RuleFor(x => x.Description)
				.MaximumLength(500).WithMessage("Description can't be longer than 500 characters.");
		}
	}

	public class CreateCategoryRequestValidator : AbstractValidator<CreateCategoryRequest>
	{
		public CreateCategoryRequestValidator()
		{
			Include(new CategoryRequestRules());
		}
	}

	public class UpdateCategoryRequestValidator : AbstractValidator<UpdateCategoryRequest>
	{
		public UpdateCategoryRequestValidator()
		{
			Include(new CategoryRequestRules());
		}
	}
}