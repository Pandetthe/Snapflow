namespace Snapflow.Api.Hubs.Board;

// Hub methods for card operations
internal sealed partial class BoardHub
{
    public Task CreateCard()
    {
        logger.LogInformation("Card create requested by connection {ConnectionId}.", Context.ConnectionId);
        return Task.CompletedTask;
    }

    public Task UpdateCard()
    {
        logger.LogInformation("Card update requested by connection {ConnectionId}.", Context.ConnectionId);
        return Task.CompletedTask;
    }

    public Task MoveCard()
    {
        logger.LogInformation("Card update requested by connection {ConnectionId}.", Context.ConnectionId);
        return Task.CompletedTask;
    }

    public Task DeleteCard()
    {
        logger.LogInformation("Card delete requested by connection {ConnectionId}.", Context.ConnectionId);
        return Task.CompletedTask;
    }
}
