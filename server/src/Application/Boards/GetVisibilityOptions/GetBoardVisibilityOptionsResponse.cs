using Snapflow.Domain.Boards;

namespace Snapflow.Application.Boards.GetVisibilityOptions;

public sealed record GetBoardVisibilityOptionsResponse(IReadOnlyList<BoardVisibility> AllowedVisibilities);
