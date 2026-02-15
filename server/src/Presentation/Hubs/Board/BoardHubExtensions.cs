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
        context.Items.TryGetValue("BoardId", out var boardIdObj) && boardIdObj is int boardId
            ? boardId
            : throw new InvalidOperationException("BoardId not found in HubCallerContext items.");
}