using System;
using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace Keda.Samples.Dotnet.Contracts;

public static class AddServiceBusClientExtension
{
    public static IServiceCollection AddOrderQueueServices(this IServiceCollection services)
    {
        services.AddSingleton<ServiceBusClient>(svc =>
        {
            var logger = svc.GetRequiredService<ILogger<ServiceBusClient>>();
            var options = svc.GetRequiredService<IOptions<OrderQueueOptions>>();
            return AuthenticateToAzureServiceBus(options.Value, logger);
        });
        services.AddScoped<ServiceBusReceiver>(svc =>
        {
            var client = svc.GetRequiredService<ServiceBusClient>();
            var options = svc.GetRequiredService<IOptions<OrderQueueOptions>>();
            return client.CreateReceiver(options.Value.GetEntityPath());
        });
        services.AddScoped<ServiceBusSender>(svc =>
        {
            var client = svc.GetRequiredService<ServiceBusClient>();
            var options = svc.GetRequiredService<IOptions<OrderQueueOptions>>();
            return client.CreateSender(options.Value.GetEntityPath());
        });
        services.AddScoped<ServiceBusProcessor>(svc =>
        {
            var client = svc.GetRequiredService<ServiceBusClient>();
            var options = svc.GetRequiredService<IOptions<OrderQueueOptions>>();
            return client.CreateProcessor(options.Value.GetEntityPath());
        });

        return services;
    }

    private static ServiceBusClient AuthenticateToAzureServiceBus(OrderQueueOptions options, ILogger logger)
    {
        switch (options.AuthMode)
        {
            case OrderQueueOptions.AuthenticationMode.AzureDefaultCredential:
                logger.LogInformation($"Authentication with Azure Default Credential");
                return new ServiceBusClient(options.FullyQualifiedNamespace, new DefaultAzureCredential());
            case OrderQueueOptions.AuthenticationMode.ConnectionString:
                logger.LogInformation($"Authentication by using connection string");
                return new ServiceBusClient(options.ConnectionString);
            case OrderQueueOptions.AuthenticationMode.ServicePrinciple:
                logger.LogInformation("Authentication by using service principle {ClientId}", options.ClientId);
                return new ServiceBusClient(options.FullyQualifiedNamespace, new ClientSecretCredential(options.TenantId, options.ClientId, options.ClientSecret));
            case OrderQueueOptions.AuthenticationMode.PodIdentity:
                logger.LogInformation("Authentication by using pod identity {ClientId}", options.ClientId);
                return new ServiceBusClient(options.FullyQualifiedNamespace, new ManagedIdentityCredential(options.ClientId));
            case OrderQueueOptions.AuthenticationMode.WorkloadIdentity:
                logger.LogInformation("Authentication by using workload identity {ClientId}", options.ClientId);
                return new ServiceBusClient(options.FullyQualifiedNamespace, new ManagedIdentityCredential(options.ClientId));
            default:
                throw new ArgumentOutOfRangeException("AuthMode","AuthMode not supported");
        }
    }
}
