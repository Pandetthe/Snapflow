using Snapflow.Common;

namespace Snapflow.Domain.Ranking;

public static class RankingErrors
{
    public static Error InvalidNormalizationRange =>
        new("Ranking.InvalidNormalizationRange",
            "Cannot normalize ranks because both left and right ranks are null.",
            ErrorType.Problem);

    public static Error RankExhausted =>
        new("Ranking.RankExhausted",
            "Cannot generate a new rank between the specified ranks because the rank space is exhausted.",
            ErrorType.Problem);
}
