using Activite.Services.Integration.Commands;
using Activite.Services.Integration.DTOs;
using Activite.Services.Integration.Options;
using Activite.Services.Integration.Queries;
using Convey;
using Convey.CQRS.Commands;
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
using Microsoft.Extensions.DependencyInjection;

#if DEBUG

DotNetEnv.Env.Load();

#endif

var host = WebHost.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((hostingContext, config) =>
    {
        config.AddEnvironmentVariables();
    })
    .ConfigureServices((context, services) =>
    {
        services.Configure<EmailVerificationOptions>(context.Configuration.GetSection(EmailVerificationOptions.EmailVerification));

        services
            .AddConvey()
            .AddWebApi()
            .AddHttpClient()
            .AddConsul()
            .AddCommandHandlers()
            .AddInMemoryCommandDispatcher()
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
                .Post<SendEmailVerification>("/EmailVerification")
                .Get<GetGoogleToken, GoogleTokenDto>("/Google/Token"));
    })
    .UseLogging()
    .Build();

await host.RunAsync();