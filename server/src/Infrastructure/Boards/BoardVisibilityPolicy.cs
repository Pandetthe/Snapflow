using Microsoft.Extensions.Options;
using Snapflow.Application.Abstractions.Services;
using Snapflow.Domain.Boards;

namespace Snapflow.Infrastructure.Boards;

public sealed class BoardVisibilityPolicy(IOptions<BoardVisibilityOptions> options) : IBoardVisibilityPolicy
{
    private static readonly BoardVisibility[] DefaultVisibilities = [BoardVisibility.Private, BoardVisibility.Unlisted, BoardVisibility.Public];

    public IReadOnlyList<BoardVisibility> AllowedVisibilities { get; } =
        (options.Value.AllowedVisibilities ?? DefaultVisibilities)
            .Append(BoardVisibility.Private)
            .Distinct()
            .Order()
            .ToArray();

    public bool IsAllowed(BoardVisibility visibility) => AllowedVisibilities.Contains(visibility);

    public bool CanNonMemberView(BoardVisibility visibility, bool isAuthenticated) =>
        GetEffectiveVisibility(visibility) switch
        {
            BoardVisibility.Unlisted or BoardVisibility.Public => true,
            _ => false
        };

    public bool IsListed(BoardVisibility visibility) =>
        GetEffectiveVisibility(visibility) == BoardVisibility.Public;

    private BoardVisibility GetEffectiveVisibility(BoardVisibility visibility) =>
        AllowedVisibilities.Where(allowed => allowed <= visibility).Max();
}
