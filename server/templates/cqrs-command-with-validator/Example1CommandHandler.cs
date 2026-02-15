using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Common;

namespace SnapflowCQRS;

internal sealed class Example1CommandHandler : ICommandHandler<Example1Command>
{
    public Task<Result> Handle(Example1Command command, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}