using Activite.Services.Integration.DTOs;
using Activite.Services.Integration.Queries;
using Convey;
using Convey.CQRS.Queries;
using Convey.Discovery.Consul;
using Convey.HTTP;
using Convey.Logging;
using Convey.WebApi;
using Convey.WebApi.CQRS;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

#if DEBUG

DotNetEnv.Env.Load();

#endif

var host = WebHost.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((hostingContext, config) =>
    {
        config.AddEnvironmentVariables();
    })
    .ConfigureServices(services =>
    {
        services
            .AddConvey()
            .AddWebApi()
            .AddHttpClient()
            .AddConsul()
            .AddQueryHandlers()
            .AddInMemoryQueryDispatcher()
            .Build();
    })
    .Configure(app =>
    {
        app
            .UseConvey()
            .UseDispatcherEndpoints(endpoints => endpoints
                .Get("/ping", ctx => ctx.Response.WriteAsync("pong"))
                .Get<GetGoogleToken, GoogleTokenDto>("/Google/Token"));
    })
    .UseLogging()
    .Build();

await host.RunAsync();