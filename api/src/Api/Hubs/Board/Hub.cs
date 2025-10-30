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
}

[Authorize]
internal sealed partial class BoardHub(
    ILogger<BoardHub> logger) : Hub<IBoardHubClient>
{
    public override Task OnConnectedAsync()
    {
        string connectionId = Context.ConnectionId;
        string? boardIdString = Context.GetHttpContext()?.Request.Query["boardId"];
        if (!int.TryParse(boardIdString, out int boardId))
        {
            logger.LogWarning("Connection {ConnectionId} aborted: invalid boardId query parameter '{BoardIdString}'", connectionId, boardIdString);
            Context.Abort();
            return Task.CompletedTask;
        }
        // TODO add to groups
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        return base.OnDisconnectedAsync(exception);
    }
}
