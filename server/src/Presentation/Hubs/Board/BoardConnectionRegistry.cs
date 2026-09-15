using System.Collections.Concurrent;

namespace Snapflow.Presentation.Hubs.Board;

/// <summary>
/// The live board connections of each user, so a user who loses access to a board can be taken out
/// of its group. SignalR can move a connection between groups by its id but cannot enumerate a
/// group, so the ids are kept here.
/// </summary>
/// <remarks>
/// Only the connections held by this server. With the Redis backplane in front of several servers,
/// a connection on another server keeps its group until it reconnects; taking it out everywhere
/// needs a signal across the backplane.
/// </remarks>
public sealed class BoardConnectionRegistry
{
    private readonly ConcurrentDictionary<(int BoardId, int UserId), ConcurrentDictionary<string, byte>> _connections = new();

    public void Add(int boardId, int userId, string connectionId) =>
        _connections
            .GetOrAdd((boardId, userId), _ => new ConcurrentDictionary<string, byte>())
            .TryAdd(connectionId, 0);

    public void Remove(int boardId, int userId, string connectionId)
    {
        if (!_connections.TryGetValue((boardId, userId), out var connectionIds))
            return;

        connectionIds.TryRemove(connectionId, out _);

        // A user with nothing left open should not hold an entry; a connection racing in here is
        // put back by the Add above, which creates the entry again.
        if (connectionIds.IsEmpty)
            _connections.TryRemove(new KeyValuePair<(int, int), ConcurrentDictionary<string, byte>>((boardId, userId), connectionIds));
    }

    public IReadOnlyList<string> GetConnectionIds(int boardId, int userId) =>
        _connections.TryGetValue((boardId, userId), out var connectionIds)
            ? connectionIds.Keys.ToList()
            : [];
}
