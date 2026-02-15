using Microsoft.AspNetCore.SignalR;

namespace Snapflow.Infrastructure.Auth.Accessors;

public sealed class HubCallerContextAccessor
{
    private readonly AsyncLocal<HubCallerContext?> _current = new();

    public HubCallerContext? HubCallerContext
    {
        get => _current.Value;
        set => _current.Value = value;
    }

    public string? ConnectionId => _current.Value?.ConnectionId;
}