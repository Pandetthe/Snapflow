using FluentValidation;

namespace SnapflowCQRS;

internal sealed class Example1Validator : AbstractValidator<Example1Command>
{
    public Example1Validator()
    {
        RuleFor(c => c.Name)
            .NotEmpty().WithMessage("Name is required.");
    }
}
