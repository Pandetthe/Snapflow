namespace Snapflow.Presentation.Hubs.Board;

/// <summary>
/// The live board connections of each user, so a user who loses access to a board can be taken out
/// of its group, and so the board can show who is looking at it. SignalR can move a connection
/// between groups by its id but cannot enumerate a group, so the ids are kept here.
/// </summary>
/// <remarks>
/// Only the connections held by this server. With the Redis backplane in front of several servers,
/// a connection on another server keeps its group until it reconnects; taking it out everywhere
/// needs a signal across the backplane. For the same reason a viewer on another server is not
/// listed here.
/// </remarks>
public sealed class BoardConnectionRegistry
{
    private readonly Dictionary<(int BoardId, int UserId), HashSet<string>> _connections = [];
    private readonly Lock _gate = new();

    /// <returns><see langword="true"/> when the user had nothing else open on the board.</returns>
    public bool Add(int boardId, int userId, string connectionId)
    {
        lock (_gate)
        {
            if (!_connections.TryGetValue((boardId, userId), out var connectionIds))
            {
                connectionIds = [];
                _connections[(boardId, userId)] = connectionIds;
            }

            return connectionIds.Add(connectionId) && connectionIds.Count == 1;
        }
    }

    /// <returns><see langword="true"/> when the user has nothing left open on the board.</returns>
    public bool Remove(int boardId, int userId, string connectionId)
    {
        lock (_gate)
        {
            if (!_connections.TryGetValue((boardId, userId), out var connectionIds))
                return false;

            if (!connectionIds.Remove(connectionId) || connectionIds.Count > 0)
                return false;

            _connections.Remove((boardId, userId));
            return true;
        }
    }

    public IReadOnlyList<string> GetConnectionIds(int boardId, int userId)
    {
        lock (_gate)
        {
            return _connections.TryGetValue((boardId, userId), out var connectionIds)
                ? [.. connectionIds]
                : [];
        }
    }

    public IReadOnlyList<int> GetUserIds(int boardId)
    {
        lock (_gate)
        {
            return [.. _connections.Keys.Where(key => key.BoardId == boardId).Select(key => key.UserId)];
        }
    }
}
