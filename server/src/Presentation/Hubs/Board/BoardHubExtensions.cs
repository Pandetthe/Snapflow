using Microsoft.AspNetCore.SignalR;

namespace Snapflow.Presentation.Hubs.Board;

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

    public static string GuestsGroupName(int boardId) => $"{boardId}-guests";

    public static IBoardHubClient Guests(this IHubClients<IBoardHubClient> clients, int boardId) =>
        clients.Group(GuestsGroupName(boardId));

    public static async Task SendToBoard(this IHubClients<IBoardHubClient> clients, int boardId,
        string? excludedConnectionId, Func<IBoardHubClient, bool, Task> send)
    {
        await send(clients.GroupExcept(boardId, excludedConnectionId), true);
        await send(clients.Guests(boardId), false);
    }

    public static IBoardHubClient GroupExcept(this IHubClients<IBoardHubClient> clients, int boardId, params string?[] excludedConnectionIds)
    {
        var groupName = $"{boardId}";
        var filtered = (excludedConnectionIds ?? Array.Empty<string?>())
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Select(id => id!)
            .Distinct()
            .ToArray();

        return filtered.Length == 0
            ? clients.Group(groupName)
            : clients.GroupExcept(groupName, filtered);
    }

    public static IBoardHubClient GroupExcept(this IHubCallerClients<IBoardHubClient> clients, int boardId, params string?[] excludedConnectionIds)
    {
        var groupName = $"{boardId}";
        var filtered = (excludedConnectionIds ?? Array.Empty<string?>())
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Select(id => id!)
            .Distinct()
            .ToArray();

        return filtered.Length == 0
            ? clients.Group(groupName)
            : clients.GroupExcept(groupName, filtered);
    }

    public static IBoardHubClient GroupExcept(this IHubClients<IBoardHubClient> clients, int boardId, int userId, params string?[] excludedConnectionIds)
    {
        var groupName = $"{boardId}-{userId}";
        var filtered = (excludedConnectionIds ?? Array.Empty<string?>())
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Select(id => id!)
            .Distinct()
            .ToArray();

        return filtered.Length == 0
            ? clients.Group(groupName)
            : clients.GroupExcept(groupName, filtered);
    }

    public static IBoardHubClient GroupExcept(this IHubCallerClients<IBoardHubClient> clients, int boardId, int userId, params string?[] excludedConnectionIds)
    {
        var groupName = $"{boardId}-{userId}";
        var filtered = (excludedConnectionIds ?? Array.Empty<string?>())
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Select(id => id!)
            .Distinct()
            .ToArray();

        return filtered.Length == 0
            ? clients.Group(groupName)
            : clients.GroupExcept(groupName, filtered);
    }


    public static HubCallerContext SetBoardId(this HubCallerContext context, int boardId)
    {
        context.Items["BoardId"] = boardId;
        return context;
    }

    public static int GetBoardId(this HubCallerContext context) =>
        context.TryGetBoardId(out var boardId)
            ? boardId
            : throw new InvalidOperationException("BoardId not found in HubCallerContext items.");

    public static bool TryGetBoardId(this HubCallerContext context, out int boardId)
    {
        if (context.Items.TryGetValue("BoardId", out var boardIdObj) && boardIdObj is int value)
        {
            boardId = value;
            return true;
        }

        boardId = 0;
        return false;
    }

    public static HubCallerContext SetUserId(this HubCallerContext context, int userId)
    {
        context.Items["UserId"] = userId;
        return context;
    }

    public static HubCallerContext SetGuest(this HubCallerContext context)
    {
        context.Items["Guest"] = true;
        return context;
    }

    public static bool IsGuest(this HubCallerContext context) =>
        context.Items.TryGetValue("Guest", out var guest) && guest is true;

    public static bool TryGetUserId(this HubCallerContext context, out int userId)
    {
        if (context.Items.TryGetValue("UserId", out var userIdObj) && userIdObj is int value)
        {
            userId = value;
            return true;
        }

        userId = 0;
        return false;
    }
}