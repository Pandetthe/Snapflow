namespace Snapflow.Application.Boards.GetById;

public sealed record BoardResponse
{
    public int Id { get; init; }
    public required string Title { get; init; }
    public string Description { get; init; } = "";
    public required DateTimeOffset CreatedAt { get; init; }
    public required int CreatedBy { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }
    public int? UpdatedBy { get; init; }
    public DateTimeOffset? DeletedAt { get; init; }
    public int? DeletedBy { get; init; }
    public bool IsDeleted { get; init; }
}
