using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Common;

namespace Snapflow.Application.Cards.Delete;

internal sealed class DeleteCardCommandHandler : ICommandHandler<DeleteCardCommand>
{
    public Task<Result> Handle(DeleteCardCommand command, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}