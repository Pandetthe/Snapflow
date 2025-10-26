using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Snapflow.Api.Hubs.BoardHub;

[Authorize]
internal sealed partial class BoardHub(
    ILogger<BoardHub> logger) : Hub
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
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        return base.OnDisconnectedAsync(exception);
    }
}
