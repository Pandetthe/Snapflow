using System.Globalization;
using Microsoft.AspNetCore.OutputCaching;

namespace Snapflow.Presentation.Caching;

internal sealed class BoardOutputCachePolicy : IOutputCachePolicy
{
    public ValueTask CacheRequestAsync(OutputCacheContext context, CancellationToken cancellationToken)
    {
        var boardIdStr = context.HttpContext.GetRouteValue("boardId")?.ToString();
        if (boardIdStr is null || !int.TryParse(boardIdStr, CultureInfo.InvariantCulture, out var boardId))
        {
            context.AllowCacheLookup = false;
            context.AllowCacheStorage = false;
            return ValueTask.CompletedTask;
        }

        context.EnableOutputCaching = true;
        context.AllowCacheLookup = true;
        context.AllowCacheStorage = true;
        context.AllowLocking = true;
        context.Tags.Add(CacheTags.Board(boardId));
        return ValueTask.CompletedTask;
    }

    public ValueTask ServeFromCacheAsync(OutputCacheContext context, CancellationToken cancellationToken)
        => ValueTask.CompletedTask;

    public ValueTask ServeResponseAsync(OutputCacheContext context, CancellationToken cancellationToken)
    {
        if (context.HttpContext.Response.StatusCode != StatusCodes.Status200OK)
            context.AllowCacheStorage = false;
        return ValueTask.CompletedTask;
    }
}
