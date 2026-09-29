using System.Net.Sockets;

namespace Snapflow.IntegrationTests;

public sealed class DockerFactAttribute : FactAttribute
{
    public DockerFactAttribute()
    {
        if (DockerEndpoint.ExternalConnectionString is null && !DockerEndpoint.IsAvailable)
            Skip = DockerEndpoint.SkipReason;
    }
}

internal static class DockerEndpoint
{
    private const string DefaultSocketPath = "/var/run/docker.sock";

    private static readonly Lazy<bool> Available = new(Probe, isThreadSafe: true);

    public static string? ExternalConnectionString =>
        Environment.GetEnvironmentVariable("SNAPFLOW_TEST_POSTGRES") is { Length: > 0 } value ? value : null;

    public static bool IsAvailable => Available.Value;

    public static string SkipReason =>
        $"Docker API socket is not reachable at '{Endpoint()}'. Integration tests need one " +
        "(rootless podman: `systemctl --user enable --now podman.socket`).";

    private static string Endpoint() =>
        Environment.GetEnvironmentVariable("DOCKER_HOST") is { Length: > 0 } host
            ? host
            : $"unix://{DefaultSocketPath}";

    private static bool Probe()
    {
        var endpoint = Endpoint();

        if (!endpoint.StartsWith("unix://", StringComparison.Ordinal))
            return true;

        var path = endpoint["unix://".Length..];

        try
        {
            using var socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
            socket.Connect(new UnixDomainSocketEndPoint(path));
            return true;
        }
        catch (SocketException)
        {
            return false;
        }
        catch (IOException)
        {
            return false;
        }
    }
}
