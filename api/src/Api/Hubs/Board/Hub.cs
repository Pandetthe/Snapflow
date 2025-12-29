using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Snapflow.Domain.Boards;

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

[Authorize(BoardPermissions.Boards.View)]
public sealed partial class BoardHub(
    ILogger<BoardHub> logger) : Hub<IBoardHubClient>
{
    public async override Task OnConnectedAsync()
    {
        var httpContext = Context.GetHttpContext();
        if (httpContext is null)
        {
            logger.LogError("Connection {ConnectionId} aborted: HttpContext is null.", Context.ConnectionId);
            Context.Abort();
            return;
        }
        if (!httpContext.Request.RouteValues.TryGetValue("boardId", out var boardIdObj) ||
            !int.TryParse(boardIdObj?.ToString(), out int boardId))
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
        }

        await base.OnConnectedAsync();
        logger.LogInformation("Connection {ConnectionId} connected to board {BoardId}.", Context.ConnectionId, boardId);
    }
}
