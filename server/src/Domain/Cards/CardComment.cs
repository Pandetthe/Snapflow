using Snapflow.Domain.Users;
using Snapflow.Common;

namespace Snapflow.Domain.Cards;

public class CardComment : Entity<int, CardComment>
{
    public int CardId { get; private set; }
    public int UserId { get; private set; }
    public string Content { get; private set; } = null!;
    public DateTimeOffset CreatedAt { get; private set; }
    public virtual IUser User { get; private set; } = null!;

    public static CardComment Create(int cardId, int userId, string content)
    {
        return new CardComment
        {
            CardId = cardId,
            UserId = userId,
            Content = content,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }
}