using Peddle.Foundation.Common.Extensions.Configuration;
using Peddle.Foundation.Common.Extensions.Configuration.Json;
using Peddle.Foundation.Common.Extensions.Configuration.SystemsManager;
using Peddle.Foundation.Common.Logging.Dependencies;
using Serilog;
using Microsoft.AspNetCore.Builder;
using ListenerServiceTemplate.Shared;
using System.Reflection;
using TinyHealthCheck;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddBasicTinyHealthCheckWithUptime((config) =>
{
    config.Port = 8088;
    config.UrlPath = "/health/liveness";
    config.Hostname = "*";
    return config;
});
// Add services to the container.
builder.Configuration.SetBasePath(builder.Environment.ContentRootPath).AddEnvironmentVariables();

builder.Configuration.AddUserSecrets(Assembly.GetExecutingAssembly());

var peddleLoggingSection = builder.Configuration.GetRequiredSection("Peddle:Logging");

var loggerSystemsManagerPaths = peddleLoggingSection
    .GetRequiredSection("SystemsManagerPaths")
    .Get<List<SystemsManagerPath>>();

var loggerConfigPaths = peddleLoggingSection
    .GetRequiredSection("JsonConfigPaths")
    .Get<List<JsonParameterPath>>();

builder.Configuration.AddSystemsManagerForPaths(loggerSystemsManagerPaths, DependenciesVersion.GetVersionString())
                .AddJsonParameters(loggerConfigPaths);

builder.Logging.ClearProviders();
builder.Host.UseSerilog((context, _, loggerConfiguration) => loggerConfiguration.ReadFrom.Configuration(context.Configuration).EnableSensitiveDataMasking());
builder.Configuration.AddSystemsManagerForProduct(Product.{{ProductName}}, builder.Configuration, builder.Services, options =>
{
    options.IdentityProviderServiceHost = builder.Configuration.GetValue<string>("IDENTITY_PROVIDER_SERVICE_HOST");
    options.GenericMicroserviceClientId = builder.Configuration.GetValue<string>("Secrets:ApiClient:GenericMicroserviceClientId");
    options.GenericMicroserviceClientSecret = builder.Configuration.GetValue<string>("Secrets:ApiClient:GenericMicroserviceClientSecret");
    options.ApiGatewayServiceHost = builder.Configuration.GetValue<string>("API_GATEWAY_SERVICE_UPSTREAM_HOST");
});
builder.Services
    .AddServicesExtension(builder.Configuration);



var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseSerilogRequestLogging();

app.Run();