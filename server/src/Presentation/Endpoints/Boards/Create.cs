using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Boards.Create;
using Snapflow.Common;
using Snapflow.Domain.Members;
using Snapflow.Presentation.Extensions;

namespace Snapflow.Presentation.Endpoints.Boards;

internal sealed class Create : IEndpoint
{
    public sealed record CreateBoardMemberDto(int UserId, MemberRole Role);

    public sealed record CreateBoardRequest(
        string Title,
        string Description,
        IReadOnlyList<CreateBoardMemberDto>? Members = null);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("boards", async (
            CreateBoardRequest request,
            ICommandHandler<CreateBoardCommand, int> handler,
            CancellationToken cancellationToken) =>
        {
            var members = request.Members?.Select(m => new CreateBoardMemberRequest(m.UserId, m.Role)).ToList();
            var command = new CreateBoardCommand(request.Title, request.Description, members);

            Result<int> result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.OkWithId, Results.Problem);
        })
        .RequireAuthorization()
        .WithTags(EndpointTags.Boards)
        .ProducesIdResponse()
        .ProducesCustomValidationProblem();
    }
}
