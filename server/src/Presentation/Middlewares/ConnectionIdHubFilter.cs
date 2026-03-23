using Microsoft.AspNetCore.SignalR;
using Snapflow.Infrastructure.Auth.Accessors;

namespace Snapflow.Presentation.Middlewares;

public sealed class ConnectionIdHubFilter(HubCallerContextAccessor accessor) : IHubFilter
{
    public async ValueTask<object?> InvokeMethodAsync(
        HubInvocationContext invocationContext,
        Func<HubInvocationContext, ValueTask<object?>> next)
    {
        accessor.HubCallerContext = invocationContext.Context;
        try
        {
            return await next(invocationContext);
        }
        finally
        {
            accessor.HubCallerContext = null;
        }
    }

    public Task OnConnectedAsync(HubLifetimeContext context, Func<HubLifetimeContext, Task> next)
    {
        accessor.HubCallerContext = context.Context;
        return next(context);
    }

    public Task OnDisconnectedAsync(HubLifetimeContext context, Exception? exception, Func<HubLifetimeContext, Exception?, Task> next)
    {
        accessor.HubCallerContext = context.Context;
        return next(context, exception);
    }
}