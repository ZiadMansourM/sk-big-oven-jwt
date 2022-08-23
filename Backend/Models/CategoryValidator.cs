using FluentValidation;
namespace Backend.Models;

public class CategoryValidator : AbstractValidator<Category>
{
    public CategoryValidator()
    {
        RuleFor(category => category.Name).NotEmpty();
    }
}