using FluentValidation;

namespace Snapflow.Application.Swimlanes.Move;

internal sealed class MoveSwimlaneCommandCommandValidator : AbstractValidator<MoveSwimlaneCommandCommand>
{
    public MoveSwimlaneCommandCommandValidator()
    {
    }
}