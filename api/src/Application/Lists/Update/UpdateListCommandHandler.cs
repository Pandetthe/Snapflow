using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Common;

namespace Snapflow.Application.Lists.Update;

internal sealed class UpdateListCommandHandler : ICommandHandler<UpdateListCommand>
{
    public Task<Result> Handle(UpdateListCommand command, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}