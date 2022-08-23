using FluentValidation;
namespace Frontend.Models;

public class CategoryValidator : AbstractValidator<Category>
{
    public CategoryValidator()
    {
        RuleFor(category => category.Name).NotEmpty();
    }
}