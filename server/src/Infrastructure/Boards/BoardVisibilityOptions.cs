using Snapflow.Domain.Boards;

namespace Snapflow.Infrastructure.Boards;

public sealed class BoardVisibilityOptions
{
    public const string SectionName = "Boards";

    public BoardVisibility[]? AllowedVisibilities { get; init; }
}
