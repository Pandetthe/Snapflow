namespace Snapflow.Api.Hubs.BoardHub;

// Hub methods for swimlane operations
internal sealed partial class BoardHub
{
    public Task CreateSwimlane()
    {
        logger.LogInformation("Swimlane create requested by connection {ConnectionId}.", Context.ConnectionId);
        return Task.CompletedTask;
    }

    public Task UpdateSwimlane()
    {
        logger.LogInformation("Swimlane update requested by connection {ConnectionId}.", Context.ConnectionId);
        return Task.CompletedTask;
    }

    public Task MoveSwimlane()
    {
        logger.LogInformation("Swimlane update requested by connection {ConnectionId}.", Context.ConnectionId);
        return Task.CompletedTask;
    }

    public Task DeleteSwimlane()
    {
        logger.LogInformation("Swimlane delete requested by connection {ConnectionId}.", Context.ConnectionId);
        return Task.CompletedTask;
    }
}