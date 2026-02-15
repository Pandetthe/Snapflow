using FluentValidation;

namespace SnapflowCQRS;

internal sealed class Example1CommandValidator : AbstractValidator<Example1Command>
{
    public Example1CommandValidator()
    {
    }
}