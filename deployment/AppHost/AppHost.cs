using Aspire.Hosting.Yarp.Transforms;

using System.Diagnostics;

var builder = DistributedApplication.CreateBuilder(args);

var redis = builder.AddRedis("redis")
                   .WithDataVolume("snapflow-redis-data")
                   .WithRedisInsight();

var postgres = builder.AddPostgres("postgres")
                      .WithDataVolume("snapflow-postgres-data")
                      .WithPgAdmin();

var db = postgres.AddDatabase("snapflow-main");

var mailpit = builder.AddMailPit("mailserver");

var gateway = builder.AddYarp("gateway");

var isHttps = builder.Configuration["DOTNET_LAUNCH_PROFILE"] == "https";
var gatewayUrl = isHttps ? gateway.GetEndpoint("https") : gateway.GetEndpoint("http");

var api = builder.AddProject<Projects.Presentation>("api-server")
                 .WithReference(redis, "Redis")
                 .WithReference(db, "Postgres")
                 .WaitFor(db)
                 .WaitFor(redis)
                 .WaitFor(mailpit)
                 .WithEnvironment("Services__WebUrl", gatewayUrl)
                 .WithEnvironment("Services__ApiUrl", $"{gatewayUrl}/api")
                 .WithEnvironment("Jwt__Secret", "super-tajne-haslo-minimum-32-znaki-!!!!")
                 .WithEnvironment("Email__Host", mailpit.GetEndpoint("smtp").Property(EndpointProperty.Host))
                 .WithEnvironment("Email__Port", mailpit.GetEndpoint("smtp").Property(EndpointProperty.Port))
                 .WithEnvironment("Email__RequireAuthentication", "False")
                 .WithReplicas(1);

var apiUrl = api.GetEndpoint("http");

var web = builder.AddViteApp("web-client", "../../web")
                 .WithReference(api)
                 .WithEnvironment("API_BASE_URL", apiUrl)
                 .WithEnvironment("PUBLIC_API_BASE_URL", $"{gatewayUrl}/api");

var otlpHttpEndpoint = builder.Configuration["ASPIRE_DASHBOARD_OTLP_HTTP_ENDPOINT_URL"];
if (!string.IsNullOrEmpty(otlpHttpEndpoint))
{
    web.WithEnvironment("OTEL_EXPORTER_OTLP_ENDPOINT", otlpHttpEndpoint);
}

web.PublishAsDockerFile();

gateway.WithConfiguration(yarp =>
{
    yarp.AddRoute("/api/{**catch-all}", api)
        .WithTransformPathRemovePrefix("/api");

    yarp.AddRoute("{**catch-all}", web);
});

builder.Build().Run();