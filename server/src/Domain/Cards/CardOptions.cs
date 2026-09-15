namespace Snapflow.Domain.Cards;

public sealed class CardOptions
{
    public const int MinTitleLength = 3;
    public const int MaxTitleLength = 50;
    // Markdown; must match MAX_DESCRIPTION_LENGTH in the web card modal. Kept within SignalR's default 32 KB message.
    public const int MaxDescriptionLength = 10000;
}
