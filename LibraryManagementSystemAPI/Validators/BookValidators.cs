using FluentValidation;
using LibraryManagementSystemAPI.Common;
using LibraryManagementSystemAPI.Dtos;

namespace LibraryManagementSystemAPI.Validators
{
	public class BookRequestRules : AbstractValidator<IBookRequest>
	{
		public BookRequestRules()
		{
			RuleFor(x => x.Title)
				.NotEmpty().WithMessage("Title is required.")
				.MaximumLength(200).WithMessage("Title can't be longer than 200 characters.");

			RuleFor(x => x.Description)
				.MaximumLength(2000).WithMessage("Description can't be longer than 2000 characters.");

			RuleFor(x => x.Isbn)
				.Cascade(CascadeMode.Stop)
				.NotEmpty().WithMessage("ISBN is required.")
				.Must(IsbnHelper.IsValid).WithMessage("ISBN is not valid. Use a valid ISBN-10 or ISBN-13.");

			RuleFor(x => x.Publisher)
				.MaximumLength(200).WithMessage("Publisher can't be longer than 200 characters.");

			RuleFor(x => x.PublicationYear)
				.Must(year => year is null || (year >= 1450 && year <= DateTime.UtcNow.Year))
				.WithMessage("Publication year must be between 1450 and the current year.");

			RuleFor(x => x.PageCount)
				.Must(pages => pages is null || pages > 0)
				.WithMessage("Page count must be greater than 0.");

			RuleFor(x => x.Language)
				.MaximumLength(50).WithMessage("Language can't be longer than 50 characters.");

			RuleFor(x => x.CoverImageUrl)
				.MaximumLength(500).WithMessage("Cover image URL can't be longer than 500 characters.")
				.Must(UrlHelper.IsValidHttpUrl).WithMessage("Cover image URL must be a valid http or https link.")
				.When(x => !string.IsNullOrWhiteSpace(x.CoverImageUrl));

			RuleFor(x => x.CategoryId)
				.NotEmpty().WithMessage("Category is required.");

			RuleFor(x => x.AuthorIds)
				.Cascade(CascadeMode.Stop)
				.NotEmpty().WithMessage("A book must have at least one author.")
				.Must(ids => ids.Count <= 10).WithMessage("A book can't have more than 10 authors.")
				.Must(ids => !ids.Contains(Guid.Empty)).WithMessage("Author Id can't be empty.")
				.Must(ids => ids.Distinct().Count() == ids.Count).WithMessage("The same author can't be added twice.");
		}
	}

	public class CreateBookRequestValidator : AbstractValidator<CreateBookRequest>
	{
		public CreateBookRequestValidator()
		{
			Include(new BookRequestRules());
		}
	}

	public class UpdateBookRequestValidator : AbstractValidator<UpdateBookRequest>
	{
		public UpdateBookRequestValidator()
		{
			Include(new BookRequestRules());
		}
	}
}