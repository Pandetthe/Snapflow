using Snapflow.Application.Abstractions.Messaging;

namespace SnapflowCQRS;

internal sealed sealed class Example1CommandValidator : AbstractValidator<Example1Command>
{
    public Example1CommandValidator()
    {
    }
}