namespace Snapflow.Api.Hubs.BoardHub;

// Hub methods for list operations
internal sealed partial class BoardHub
{
    public Task CreateList()
    {
        logger.LogInformation("List create requested by connection {ConnectionId}.", Context.ConnectionId);
        return Task.CompletedTask;
    }

    public Task UpdateList()
    {
        logger.LogInformation("List update requested by connection {ConnectionId}.", Context.ConnectionId);
        return Task.CompletedTask;
    }

    public Task MoveList()
    {
        logger.LogInformation("List update requested by connection {ConnectionId}.", Context.ConnectionId);
        return Task.CompletedTask;
    }

    public Task DeleteList()
    {
        logger.LogInformation("List delete requested by connection {ConnectionId}.", Context.ConnectionId);
        return Task.CompletedTask;
    }
}