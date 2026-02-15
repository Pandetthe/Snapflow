using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Snapflow.Domain.Boards;

namespace Snapflow.Presentation.Hubs.Board;

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
        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation("Connection {ConnectionId} connected to board {BoardId}.", Context.ConnectionId, boardId);
    }
}
