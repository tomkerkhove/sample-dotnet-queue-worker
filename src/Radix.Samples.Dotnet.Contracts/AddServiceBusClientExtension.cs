using System;
using System.ComponentModel.DataAnnotations;
using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace Radix.Samples.Dotnet.Contracts;

public static class AddServiceBusClientExtension
{
    public static IServiceCollection AddOrderQueueServices(this IServiceCollection services)
    {
        services.AddSingleton<ServiceBusClient>(svc =>
        {
            var logger = svc.GetRequiredService<ILogger<ServiceBusClient>>();
            var options = svc.GetRequiredService<IOptions<OrderQueueOptions>>();
            return new ServiceBusClient(options.Value.FullyQualifiedNamespace, new DefaultAzureCredential());
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
}

public class OrderQueueOptions
{
    [Required]
    public string QueueName { get; set; }
    [Required]
    public string FullyQualifiedNamespace { get; set;}
}
