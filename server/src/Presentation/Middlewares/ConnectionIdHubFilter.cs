using Microsoft.AspNetCore.SignalR;
using Snapflow.Infrastructure.Auth.Accessors;

namespace Snapflow.Presentation.Middlewares;

public sealed class ConnectionIdHubFilter : IHubFilter
{
    private readonly HubCallerContextAccessor _accessor;

    public ConnectionIdHubFilter(HubCallerContextAccessor accessor)
    {
        _accessor = accessor;
    }

    public async ValueTask<object?> InvokeMethodAsync(
        HubInvocationContext invocationContext,
        Func<HubInvocationContext, ValueTask<object?>> next)
    {
        _accessor.HubCallerContext = invocationContext.Context;
        try
        {
            return await next(invocationContext);
        }
        finally
        {
            _accessor.HubCallerContext = null;
        }
    }

    public Task OnConnectedAsync(HubLifetimeContext context, Func<HubLifetimeContext, Task> next)
    {
        _accessor.HubCallerContext = context.Context;
        return next(context);
    }

    public Task OnDisconnectedAsync(HubLifetimeContext context, Exception? exception, Func<HubLifetimeContext, Exception?, Task> next)
    {
        _accessor.HubCallerContext = context.Context;
        return next(context, exception);
    }
}