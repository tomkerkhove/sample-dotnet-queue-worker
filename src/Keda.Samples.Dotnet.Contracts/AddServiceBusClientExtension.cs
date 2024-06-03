using System;
using System.ComponentModel.DataAnnotations;
using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
            return client.CreateReceiver(options.Value.QueueName);
        });
        services.AddScoped<ServiceBusSender>(svc =>
        {
            var client = svc.GetRequiredService<ServiceBusClient>();
            var options = svc.GetRequiredService<IOptions<OrderQueueOptions>>();
            return client.CreateSender(options.Value.QueueName);
        });
        services.AddScoped<ServiceBusProcessor>(svc =>
        {
            var client = svc.GetRequiredService<ServiceBusClient>();
            var options = svc.GetRequiredService<IOptions<OrderQueueOptions>>();
            return client.CreateProcessor(options.Value.QueueName);
        });

        return services;
    }

    private static ServiceBusClient AuthenticateToAzureServiceBus(OrderQueueOptions options, ILogger logger)
    {
        switch (options.AuthMode)
        {
            case AuthenticationMode.ConnectionString:
                logger.LogInformation($"Authentication by using connection string");
                return new ServiceBusClient(options.ConnectionString);
            case AuthenticationMode.ServicePrinciple:
                logger.LogInformation("Authentication by using service principle {ClientId}", options.ClientId);
                return new ServiceBusClient(options.FullyQualifiedNamespace, new ClientSecretCredential(options.TenantId, options.ClientId, options.ClientSecret));
            case AuthenticationMode.PodIdentity:
                logger.LogInformation("Authentication by using pod identity {ClientId}", options.ClientId);
                return new ServiceBusClient(options.FullyQualifiedNamespace, new ManagedIdentityCredential(options.ClientId));
            case AuthenticationMode.WorkloadIdentity:
                logger.LogInformation("Authentication by using workload identity {ClientId}", options.ClientId);
                return new ServiceBusClient(options.FullyQualifiedNamespace, new ManagedIdentityCredential(options.ClientId));
            default:
                throw new ArgumentOutOfRangeException("AuthMode","AuthMode not supported");
        }
    }
}

public enum AuthenticationMode
{
    ConnectionString,
    ServicePrinciple,
    PodIdentity,
    WorkloadIdentity
}
public class OrderQueueOptions
{
    [Required]
    public AuthenticationMode AuthMode { get; set; }
    [Required]
    public string QueueName { get; set; }
    
    public string ConnectionString { get; set; }
    public string FullyQualifiedNamespace { get; set;}
    public string TenantId { get; set;}
    public string ClientId { get; set;}
    public string ClientSecret { get; set;}
}
