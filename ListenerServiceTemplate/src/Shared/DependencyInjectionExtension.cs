using Peddle.Foundation.Common.Helpers;
using Peddle.Foundation.Messagebroker.Topology;
namespace ListenerServiceTemplate.Shared
{
    public static class DependencyInjectionExtension
    {
        public static void AddServicesExtension(this IServiceCollection services,
            IConfiguration configuration)
        {
            services.RegisterServices(configuration);
            services.RegisterMessageBroker(configuration);
        }

        public static IServiceCollection RegisterServices(this IServiceCollection services, IConfiguration configuration)
        {
            return services;
        }
        public static IServiceCollection RegisterMessageBroker(this IServiceCollection services, IConfiguration configuration)
        {
            services.RegisterBroker(options =>
            {
                options.HostName = configuration.GetValue<string>("Configs:RabbitMQ:HostName");
                options.PortNumber = configuration.GetValue<int>("Configs:RabbitMQ:PortNumber");
                options.Username = configuration.GetValue<string>("Secrets:RabbitMQ:Username");
                options.Password = Utility.GetSecureString(configuration.GetValue<string>("Secrets:RabbitMQ:UserPassword"));
                options.IdentityProviderServiceHost = configuration.GetValue<string>("IDENTITY_PROVIDER_SERVICE_HOST");
                options.GenericMessageBrokerClientId = configuration.GetValue<string>("Secrets:ApiClient:GenericMessageBrokerClientId");
                options.GenericMessageBrokerClientSecret = configuration.GetValue<string>("Secrets:ApiClient:GenericMessageBrokerClientSecret");
            }, true);
            return services;
        }
    }
}