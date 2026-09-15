using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Boards.GetById;
using Snapflow.Application.Boards.GetDetails;
using Snapflow.Common;
using Snapflow.Domain.Boards;
using static Snapflow.Presentation.Hubs.Board.IBoardHubClient;

namespace Snapflow.Presentation.Hubs.Board;

[Authorize(BoardPermissions.Boards.View)]
public sealed partial class BoardHub(
    IServiceScopeFactory scopeFactory,
    BoardConnectionRegistry connectionRegistry,
    ILogger<BoardHub> logger) : Hub<IBoardHubClient>
{
    public override async Task OnConnectedAsync()
    {
        HttpContext? httpContext = Context.GetHttpContext();
        if (httpContext is null)
        {
            logger.LogError("Connection {ConnectionId} aborted: HttpContext is null.", Context.ConnectionId);
            Context.Abort();
            return;
        }
        if (!httpContext.Request.RouteValues.TryGetValue("boardId", out var boardIdObj) ||
            !int.TryParse(boardIdObj?.ToString(), out var boardId))
        {
            logger.LogWarning("Connection {ConnectionId} aborted: invalid or missing boardId in route.", Context.ConnectionId);
            Context.Abort();
            return;
        }
        Context.SetBoardId(boardId);
        var userIdString = Context.UserIdentifier;
        await Groups.AddToGroupAsync(Context.ConnectionId, $"{boardId}", Context.ConnectionAborted);

        if (!string.IsNullOrEmpty(userIdString))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"{boardId}-{userIdString}", Context.ConnectionAborted);

            // Noted so the connection can be taken out of the board group if the user is removed
            // from the board; the group itself cannot be enumerated.
            if (int.TryParse(userIdString, out var userId))
            {
                Context.SetUserId(userId);
                connectionRegistry.Add(boardId, userId, Context.ConnectionId);
            }
        }

        // Loaded after joining the board groups, so no change made meanwhile is missed: the client applies
        // the changes that reach it before the snapshot on top of it.
        BoardSnapshotPayload? snapshot = await LoadSnapshotAsync(boardId, Context.ConnectionAborted);
        if (snapshot is null)
        {
            logger.LogWarning("Connection {ConnectionId} aborted: board {BoardId} could not be loaded.", Context.ConnectionId, boardId);
            Context.Abort();
            return;
        }

        await base.OnConnectedAsync();
        await Clients.Caller.BoardSnapshot(snapshot, Context.ConnectionAborted);

        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation("Connection {ConnectionId} connected to board {BoardId}.", Context.ConnectionId, boardId);
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        if (Context.TryGetBoardId(out var boardId) && Context.TryGetUserId(out var userId))
        {
            connectionRegistry.Remove(boardId, userId, Context.ConnectionId);
        }

        return base.OnDisconnectedAsync(exception);
    }

    // Combines the board and its members from their own queries; Application slices do not share queries.
    private async Task<BoardSnapshotPayload?> LoadSnapshotAsync(int boardId, CancellationToken cancellationToken)
    {
        await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();
        var boardHandler = scope.ServiceProvider
            .GetRequiredService<IQueryHandler<GetBoardByIdQuery, GetBoardByIdResponse>>();
        var detailsHandler = scope.ServiceProvider
            .GetRequiredService<IQueryHandler<GetBoardDetailsQuery, GetBoardDetailsResponse>>();

        // One after the other: both handlers share the scope's DbContext.
        Result<GetBoardByIdResponse> board = await boardHandler.Handle(new GetBoardByIdQuery(boardId), cancellationToken);
        if (!board.IsSuccess)
            return null;

        Result<GetBoardDetailsResponse> details = await detailsHandler.Handle(new GetBoardDetailsQuery(boardId), cancellationToken);
        if (!details.IsSuccess)
            return null;

        return new BoardSnapshotPayload(
            board.Value.Id,
            board.Value.Title,
            board.Value.Description,
            board.Value.Swimlanes,
            board.Value.Tags,
            details.Value.Members);
    }
}
