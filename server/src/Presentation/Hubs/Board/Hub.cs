using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Boards.GetById;
using Snapflow.Application.Cards.AddComment;
using Snapflow.Common;
using Snapflow.Domain.Boards;
using Snapflow.Presentation.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Snapflow.Presentation.Hubs.Board;

[Authorize(BoardPermissions.Boards.View)]
public sealed partial class BoardHub(
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

        int boardId = 0;

        if (httpContext.Request.RouteValues.TryGetValue("boardId", out var boardIdObj) && int.TryParse(boardIdObj?.ToString(), out var parsedRouteId))
        {
            boardId = parsedRouteId;
        }
        else
        {
            var pathSegments = httpContext.Request.Path.Value?.Split('/');
            var idSegment = pathSegments?.FirstOrDefault(s => int.TryParse(s, out _));
            if (idSegment != null)
            {
                boardId = int.Parse(idSegment, CultureInfo.InvariantCulture);
            }
        }

        if (boardId == 0)
        {
            logger.LogWarning("Connection {ConnectionId} aborted: could not find boardId in URL {Url}.", Context.ConnectionId, httpContext.Request.Path.Value);
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

    public async Task AddComment(
        int cardId,
        string content,
        [FromServices] ICommandHandler<AddCardCommentCommand, GetBoardByIdResponse.CardCommentDto> handler)
    {
        // 1. Pobieranie ID użytkownika
        if (!int.TryParse(Context.UserIdentifier, out int userId)) return;

        var command = new AddCardCommentCommand(cardId, userId, content);

        // 2. Używamy konkretnego handlera do obsłużenia komendy (wywołujemy .Handle zamiast .Send)
        var result = await handler.Handle(command, default);

        // 3. Sprawdzamy wynik
        if (result.IsSuccess && result.Value is not null)
        {
            var httpContext = Context.GetHttpContext();

            if (httpContext?.Request.RouteValues.TryGetValue("boardId", out var boardIdObj) == true && boardIdObj is not null)
            {
                await Clients.Group(boardIdObj.ToString()!).CommentAdded(cardId, result.Value);
            }
        }
        else
        {
            logger.LogWarning("Failed to add comment to card {CardId}. Error: {Error}", cardId, result.Error);
        }
    }
} 