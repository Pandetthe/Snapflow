namespace Snapflow.Application.Boards.Get;

public sealed record BoardResponse
{
    public int Id { get; init; }
    public required string Title { get; init; }
    public string Description { get; init; } = "";
}
