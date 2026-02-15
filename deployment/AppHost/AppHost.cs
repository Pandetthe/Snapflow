using Aspire.Hosting.Yarp;
using Aspire.Hosting.Yarp.Transforms;

var builder = DistributedApplication.CreateBuilder(args);

var redis = builder.AddRedis("redis")
                     .WithRedisInsight();

var postgres = builder.AddPostgres("postgres")
                      .WithPgAdmin();

var db = postgres.AddDatabase("snapflow-main");

var api = builder.AddProject<Projects.Presentation>("api-server")
                 .WithReference(redis, "Redis")
                 .WithReference(db, "Postgres")
                 .WaitFor(db)
                 .WaitFor(redis)
                 .WithEnvironment("Jwt__Secret", "super-tajne-haslo-minimum-32-znaki-!!!!")
                 .WithEnvironment("Jwt__Issuer", "snapflow")
                 .WithEnvironment("Jwt__Audience", "snapflow-users")
                 .WithEnvironment("Jwt__ExpirationMinutes", "15")
                 .WithEnvironment("Services__ApiUrl", "http://test.pl")
                 .WithEnvironment("Services__WebUrl", "http://test.pl")
                 .WithReplicas(1);

var web = builder.AddNodeApp("web-client", "../../web", "index.js")
                 .WithNpm()
                 .WithRunScript("dev")
                 .WithReference(api)
                 .WithEnvironment("API_BASE_URL", api.GetEndpoint("http"))
                 .WithEnvironment("PUBLIC_API_BASE_URL", "http://localhost:8080/api")
                 .WithHttpEndpoint(targetPort: 3000, env: "PORT");

var gateway = builder.AddYarp("gateway")
       .WithHttpEndpoint(port: 8080, targetPort: 8080, name: "gateway-endpoint")
       .WithConfiguration(yarp =>
       {
           yarp.AddRoute("/api/{**catch-all}", api)
               .WithTransformPathRemovePrefix("/api");
           yarp.AddRoute(web);
       })
       .WithExternalHttpEndpoints();

builder.Build().Run();