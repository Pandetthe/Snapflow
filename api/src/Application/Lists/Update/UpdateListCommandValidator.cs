using FluentValidation;

namespace Snapflow.Application.Lists.Update;

internal sealed class UpdateListCommandValidator : AbstractValidator<UpdateListCommand>
{
    public UpdateListCommandValidator()
    {
    }
}