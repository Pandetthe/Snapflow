using FluentValidation;

namespace Snapflow.Application.Users.Me.RequestEmailChange;

internal sealed class RequestEmailChangeValidator : AbstractValidator<RequestEmailChangeCommand>
{
    public RequestEmailChangeValidator()
    {
        RuleFor(c => c.NewEmail)
            .NotEmpty()
            .EmailAddress()
            .WithMessage("A valid email address is required.");
    }
}
