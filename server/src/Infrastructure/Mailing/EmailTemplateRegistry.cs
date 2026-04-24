using Microsoft.AspNetCore.Components;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace Snapflow.Infrastructure.Mailing;

internal sealed class EmailTemplateRegistry
{
    private readonly ConcurrentDictionary<string, Type> _map
        = new(StringComparer.OrdinalIgnoreCase);

    public void Register<THtml>(string name)
        where THtml : IComponent
    {
        _map[name] = typeof(THtml);
    }

    public bool TryGet(string name, [NotNullWhen(true)] out Type? htmlComponent)
    {
        htmlComponent = null;
        if (!_map.TryGetValue(name, out Type? value))
            return false;

        htmlComponent = value;
        return true;
    }
}
