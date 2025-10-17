using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Common;

namespace SnapflowCQRS;

internal sealed class Example1QueryHandler : IQueryHandler<Example1Query, Example1Response>
{
    public Task<Result<Example1Response>> Handle(Example1Query query, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}