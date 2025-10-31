using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Snapflow.Api.Hubs.Board;

internal static class BoardHubExtensions
{
    public static IBoardHubClient Group(this IHubCallerClients<IBoardHubClient> clients, int boardId) =>
        clients.Group($"{boardId}");

    public static IBoardHubClient Group(this IHubClients<IBoardHubClient> clients, int boardId) =>
        clients.Group($"{boardId}");

    public static IBoardHubClient Group(this IHubCallerClients<IBoardHubClient> clients, int boardId, int userId) =>
        clients.Group($"{boardId}-{userId}");

    public static IBoardHubClient Group(this IHubClients<IBoardHubClient> clients, int boardId, int userId) =>
        clients.Group($"{boardId}-{userId}");

    public static HubCallerContext SetBoardId(this HubCallerContext context, int boardId)
    {
        context.Items["BoardId"] = boardId;
        return context;
    }

    public static int GetBoardId(this HubCallerContext context) =>
        context.Items.TryGetValue("BoardId", out var boardIdObj) && boardIdObj is int boardId
            ? boardId
            : throw new InvalidOperationException("BoardId not found in HubCallerContext items.");
}

[Authorize]
internal sealed partial class BoardHub(
    ILogger<BoardHub> logger) : Hub<IBoardHubClient>
{
    public async override Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
        string connectionId = Context.ConnectionId;
        string? boardIdString = Context.GetHttpContext()?.Request.Query["boardId"];
        if (!int.TryParse(boardIdString, out int boardId))
        {
            logger.LogWarning("Connection {ConnectionId} aborted: invalid boardId query parameter '{BoardIdString}'", connectionId, boardIdString);
            Context.Abort();
            return;
        }
        Context.SetBoardId(boardId);
        await Groups.AddToGroupAsync(connectionId, $"{boardId}", Context.ConnectionAborted);
        await Groups.AddToGroupAsync(connectionId, $"{boardId}-{Context.User?.Identity?.Name}", Context.ConnectionAborted);
        logger.LogInformation("Connection {ConnectionId} connected to board {BoardId}.", connectionId, boardId);
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        return base.OnDisconnectedAsync(exception);
    }
}
