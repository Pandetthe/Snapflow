using Snapflow.Common;

namespace Snapflow.Domain.Tags;

public static class TagErrors
{
    public static Error NotFound(int tagId) => Error.NotFound(
        "Tags.NotFound",
        $"The tag with the Id = '{tagId}' was not found.");

    public static Error TitleNotUnique(string title) => Error.Conflict(
        "Tags.TitleNotUnique",
        $"A tag titled '{title}' already exists on this board.");

    public static Error AlreadyOnCard(int tagId, int cardId) => Error.Conflict(
        "Tags.AlreadyOnCard",
        $"The tag with the Id = '{tagId}' is already on the card with the Id = '{cardId}'.");

    public static Error NotOnCard(int tagId, int cardId) => Error.NotFound(
        "Tags.NotOnCard",
        $"The tag with the Id = '{tagId}' is not on the card with the Id = '{cardId}'.");
}
