using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Common;

namespace Snapflow.Application.Swimlanes.Move;

internal sealed class MoveSwimlaneCommandCommandHandler : ICommandHandler<MoveSwimlaneCommand>
{
    public Task<Result> Handle(MoveSwimlaneCommand command, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}