using System.Reflection;
using System.Text.Json.Serialization;
using System.Text.Json;
using CorrelationId;
using FastEndpoints;
using Peddle.Foundation.Common.Extensions;
using Peddle.Foundation.Common.Extensions.Configuration;
using Peddle.Foundation.Common.Extensions.Configuration.Json;
using Peddle.Foundation.Common.Extensions.Configuration.SystemsManager;
using Peddle.Foundation.Common.Logging.Dependencies;
using Peddle.Foundation.Common.Middleware.Errors;
using ApiServiceTemplate.Shared;
using Serilog;

var webAppBuilder = WebApplication.CreateBuilder(args);


webAppBuilder.Services.AddEndpointsApiExplorer();
webAppBuilder.Services.AddFastEndpoints(o => o.IncludeAbstractValidators = true);

webAppBuilder.Configuration.SetBasePath(webAppBuilder.Environment.ContentRootPath).AddEnvironmentVariables();

webAppBuilder.Configuration.AddUserSecrets(Assembly.GetExecutingAssembly());

var peddleLoggingSection = webAppBuilder.Configuration.GetRequiredSection("Peddle:Logging");

var loggerSystemsManagerPaths = peddleLoggingSection
    .GetRequiredSection("SystemsManagerPaths")
    .Get<List<SystemsManagerPath>>();

var loggerConfigPaths = peddleLoggingSection
    .GetRequiredSection("JsonConfigPaths")
    .Get<List<JsonParameterPath>>();

webAppBuilder.Configuration.AddSystemsManagerForPaths(loggerSystemsManagerPaths!, DependenciesVersion.GetVersionString())
                .AddJsonParameters(loggerConfigPaths!);

webAppBuilder.Logging.ClearProviders();
webAppBuilder.Host.UseSerilog((context, _, loggerConfiguration) => loggerConfiguration.ReadFrom.Configuration(context.Configuration).EnableSensitiveDataMasking());

webAppBuilder.Configuration.AddSystemsManagerForProduct(Product.Universal,webAppBuilder.Configuration,webAppBuilder.Services,options =>
{
    options.IdentityProviderServiceHost = webAppBuilder.Configuration.GetValue<string>("IDENTITY_PROVIDER_SERVICE_HOST");
    options.GenericMicroserviceClientId = webAppBuilder.Configuration.GetValue<string>("Secrets:ApiClient:GenericMicroserviceClientId");
    options.GenericMicroserviceClientSecret = webAppBuilder.Configuration.GetValue<string>("Secrets:ApiClient:GenericMicroserviceClientSecret");
    options.ApiGatewayServiceHost = webAppBuilder.Configuration.GetValue<string>("API_GATEWAY_SERVICE_UPSTREAM_HOST");
});

webAppBuilder.Services.AddControllers();

webAppBuilder.Services.AddConfigurations(webAppBuilder.Configuration)
    .AddServiceExtension(webAppBuilder.Configuration)
    .AddAuthentication(webAppBuilder.Configuration);
var app = webAppBuilder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
app.UseCorrelationId();
app.UseSerilogRequestLogging();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<ErrorHandlerMiddleware>();
app.UseFastEndpoints(c =>
{
    c.Serializer.Options.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
    c.Serializer.Options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    c.Serializer.Options.DictionaryKeyPolicy = JsonNamingPolicy.SnakeCaseLower;
    c.Endpoints.Configurator = ep =>
    {
        ep.PostProcessors(Order.After, new GlobalErrorLogger());
    };
    c.Versioning.Prefix = "v";
    c.Versioning.DefaultVersion = 1;
    c.Versioning.PrependToRoute = true;
});
app.UseMiddleware<ErrorHandlerMiddleware>();
app.MapLivenessHealthChecks("{{HealthCheck}}/api/v1/health/liveness");
app.MapReadinessHealthChecks("{{HealthCheck}}/api/v1/health/readiness");

app.Run();