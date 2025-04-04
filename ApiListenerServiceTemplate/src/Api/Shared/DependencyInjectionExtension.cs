using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using AutoMapper;
using CorrelationId.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Peddle.Foundation.Common.Constants;
using Peddle.Foundation.Common.Filters;
using Peddle.Foundation.Common.JsonCasing;
using Peddle.Foundation.Messagebroker.Topology;
using Utility = Peddle.Foundation.Common.Helpers.Utility;

namespace ApiServiceTemplate.Shared
{
    public static class DependencyInjectionExtension
    {
        public static IServiceCollection AddServiceExtension(this IServiceCollection services,
            IConfiguration configuration)
        {
            var mapperConfig = new MapperConfiguration(mc => { mc.AddMaps(Assembly.GetExecutingAssembly()); });
            var mapper = mapperConfig.CreateMapper();
            services.AddSingleton(mapper);

            services.RegisterCorrelationIdServices();
            services.AddSingleton<Peddle.Foundation.Common.Middleware.Errors.IErrorResponsesProvider,
                Peddle.Foundation.Common.Middleware.Errors.ErrorResponsesProvider>();

            services.RegisterHealthCheck();
            services.RegisterMessageBroker(configuration);
            return services;
        }

        public static IServiceCollection AddConfigurations(this IServiceCollection services,
            IConfiguration configuration)
        {
            // Set the JSON serializer options
            services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
            {
                options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
                options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                options.SerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.SnakeCaseLower;
            });

            // Set the API Behavior options
            services.Configure<MvcOptions>(options =>
            {
                options.Filters.Add(typeof(ValidateModelFilterAttribute));
                options.Filters.Add(new JsonPatchDocumentSnakeCaseFilter());
                options.ValueProviderFactories.Add(
                    new SnakeCaseQueryValueProvider.SnakeCaseQueryValueProviderFactory());
            });

            // Set the API Behavior options
            services.Configure<ApiBehaviorOptions>(options =>
            {
                //ConfigureApiBehaviorOptions- To disable automatic model validation
                //[For stop 400 response with default message] and
                ////validate filed with fluent validation and show our custom message
                options.SuppressModelStateInvalidFilter = true;
                //SuppressMapClientErrors suppress 404 client error body,default false. 
                options.SuppressMapClientErrors = true;
            });

            return services;
        }

        public static IServiceCollection AddAuthentication(this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddAuthentication()
                .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
                {
                    options.Authority = configuration.GetValue<string>("IDENTITY_PROVIDER_SERVICE_HOST");
                    options.RequireHttpsMetadata = false;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateAudience = true,
                        ValidAudience = Audience.Universal
                    };
                });
            return services;
        }


        private static void RegisterCorrelationIdServices(this IServiceCollection services)
        {
            services.AddCorrelationId(options =>
            {
                options.AddToLoggingScope = true;
                options.RequestHeader = CorrelationIdConfig.HeaderName;
                options.UpdateTraceIdentifier = true;
            }).WithGuidProvider();
        }

        private static void RegisterHealthCheck(this IServiceCollection services)
        {
            services.AddHealthChecks();
        }
        
        public static IServiceCollection RegisterMessageBroker(this IServiceCollection services, IConfiguration configuration)
        {
            services.RegisterBroker(options =>
            {
                // options.HostName = configuration.GetValue<string>("Configs:RabbitMQ:HostName");
                // options.PortNumber = configuration.GetValue<int>("Configs:RabbitMQ:PortNumber");
                // options.Username = configuration.GetValue<string>("Secrets:RabbitMQ:Username");
                // options.Password = Utility.GetSecureString(configuration.GetValue<string>("Secrets:RabbitMQ:UserPassword"));
                options.HostName = "192.168.5.116";
                options.PortNumber = 5672;
                options.Username = "guest";
                options.Password = Utility.GetSecureString("guest");
                options.IdentityProviderServiceHost = configuration.GetValue<string>("IDENTITY_PROVIDER_SERVICE_HOST");
                options.GenericMessageBrokerClientId = configuration.GetValue<string>("Secrets:ApiClient:GenericMessageBrokerClientId");
                options.GenericMessageBrokerClientSecret = configuration.GetValue<string>("Secrets:ApiClient:GenericMessageBrokerClientSecret");
            }, true);
            return services;
        }
    }
}