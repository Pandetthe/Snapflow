using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Snapflow.Application.Abstractions.Identity;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Abstractions.Services;
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
    IBoardVisibilityPolicy visibilityPolicy,
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

        bool isMember = await IsMemberAsync(boardId, Context.ConnectionAborted);
        int? joinedUserId = null;
        if (!isMember)
        {
            Context.SetGuest();
            await Groups.AddToGroupAsync(Context.ConnectionId, BoardHubExtensions.GuestsGroupName(boardId), Context.ConnectionAborted);
            connectionRegistry.AddGuest(boardId, Context.ConnectionId, !string.IsNullOrEmpty(userIdString));
        }
        else if (!string.IsNullOrEmpty(userIdString))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"{boardId}", Context.ConnectionAborted);
            await Groups.AddToGroupAsync(Context.ConnectionId, $"{boardId}-{userIdString}", Context.ConnectionAborted);

            // Noted so the connection can be taken out of the board group if the user is removed
            // from the board; the group itself cannot be enumerated.
            if (int.TryParse(userIdString, out var userId))
            {
                Context.SetUserId(userId);
                if (connectionRegistry.Add(boardId, userId, Context.ConnectionId))
                    joinedUserId = userId;
            }
        }

        // Loaded after joining the board groups, so no change made meanwhile is missed: the client applies
        // the changes that reach it before the snapshot on top of it.
        BoardSnapshotPayload? snapshot = await LoadSnapshotAsync(boardId, isMember, Context.ConnectionAborted);
        if (snapshot is null)
        {
            logger.LogWarning("Connection {ConnectionId} aborted: board {BoardId} could not be loaded.", Context.ConnectionId, boardId);
            Context.Abort();
            return;
        }

        if (isMember && Context.TryGetUserId(out var memberId) && snapshot.Members.All(m => m.Id != memberId))
        {
            logger.LogWarning("Connection {ConnectionId} aborted: user {UserId} is no longer a member of board {BoardId}.", Context.ConnectionId, memberId, boardId);
            if (connectionRegistry.TryRemoveConnection(boardId, memberId, Context.ConnectionId))
                await Clients.Caller.RemovedFromBoard(Context.ConnectionAborted);
            Context.Abort();
            return;
        }

        if (!isMember && !visibilityPolicy.CanNonMemberView(snapshot.Visibility, !string.IsNullOrEmpty(userIdString)))
        {
            logger.LogWarning("Connection {ConnectionId} aborted: board {BoardId} is no longer open to non-members.", Context.ConnectionId, boardId);
            if (connectionRegistry.RemoveGuest(boardId, Context.ConnectionId))
                await Clients.Caller.RemovedFromBoard(Context.ConnectionAborted);
            Context.Abort();
            return;
        }

        await base.OnConnectedAsync();
        await Clients.Caller.BoardSnapshot(snapshot, Context.ConnectionAborted);

        if (joinedUserId is int joined && snapshot.Viewers.FirstOrDefault(v => v.Id == joined) is { } viewer)
        {
            await Clients
                .GroupExcept(boardId, Context.ConnectionId)
                .ViewerJoined(viewer, Context.ConnectionAborted);
        }

        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation("Connection {ConnectionId} connected to board {BoardId}.", Context.ConnectionId, boardId);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (Context.IsGuest() && Context.TryGetBoardId(out var guestBoardId))
            connectionRegistry.RemoveGuest(guestBoardId, Context.ConnectionId);

        if (Context.TryGetBoardId(out var boardId) && Context.TryGetUserId(out var userId) &&
            connectionRegistry.Remove(boardId, userId, Context.ConnectionId))
        {
            await Clients.GroupExcept(boardId, Context.ConnectionId).ViewerLeft(userId);
        }

        await base.OnDisconnectedAsync(exception);
    }

    // Combines the board and its members from their own queries; Application slices do not share queries.
    private async Task<bool> IsMemberAsync(int boardId, CancellationToken cancellationToken)
    {
        await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();
        var membershipService = scope.ServiceProvider.GetRequiredService<IBoardMembershipService>();
        return await membershipService.IsMemberAsync(boardId, cancellationToken);
    }

    private async Task<BoardSnapshotPayload?> LoadSnapshotAsync(int boardId, bool isMember, CancellationToken cancellationToken)
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

        IReadOnlyList<int> viewerIds = isMember ? connectionRegistry.GetUserIds(boardId) : [];
        var viewers = details.Value.Members
            .Where(m => viewerIds.Contains(m.Id))
            .Select(m => new UserDto(m.Id, m.UserName, m.AvatarUrl ?? string.Empty))
            .ToList();

        return new BoardSnapshotPayload(
            board.Value.Id,
            board.Value.Title,
            board.Value.Description,
            board.Value.Visibility,
            board.Value.Swimlanes,
            board.Value.Tags,
            details.Value.Members,
            viewers);
    }
}
