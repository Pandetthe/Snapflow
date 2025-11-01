using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Common;

namespace Snapflow.Application.Swimlanes.Move;

internal sealed class MoveSwimlaneCommandCommandHandler : ICommandHandler<MoveSwimlaneCommandCommand>
{
    public Task<Result> Handle(MoveSwimlaneCommandCommand command, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}