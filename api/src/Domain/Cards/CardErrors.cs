
using Snapflow.Common;

namespace Snapflow.Domain.Cards;

public static class CardErrors
{
    public static Error NotFound(int cardId) => Error.NotFound(
        "Cards.NotFound",
        $"The card with the Id = '{cardId}' was not found");

    public static readonly Error Unauthorized = Error.Failure(
        "Cards.Unauthorized",
        "You are not authorized to perform this action.");
}
