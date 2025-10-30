namespace Snapflow.Api.Hubs.Board;

// Hub methods for board operations
internal sealed partial class BoardHub
{
    public Task UpdateBoard()
    {
        logger.LogInformation("Board update requested by connection {ConnectionId}.", Context.ConnectionId);
        return Task.CompletedTask;
    }
}