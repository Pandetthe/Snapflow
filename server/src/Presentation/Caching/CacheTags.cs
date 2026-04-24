namespace Snapflow.Presentation.Caching;

internal static class CacheTags
{
    public static string User(int userId) => $"user:{userId}";
    public static string Board(int boardId) => $"board:{boardId}";
}
