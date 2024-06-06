using System.ComponentModel.DataAnnotations;
using Azure.Messaging.ServiceBus;

namespace Keda.Samples.Dotnet.Contracts;

public class OrderQueueOptions
{

    public enum AuthenticationMode
    {
        ConnectionString,
        ServicePrinciple,
        PodIdentity,
        WorkloadIdentity,
        AzureDefaultCredential,
    }

    [Required]
    public AuthenticationMode AuthMode { get; set; }
    [Required]
    public string QueueName { get; set; }

    public string ConnectionString { get; set; }
    [Required]
    public string FullyQualifiedNamespace { get; set;}
    public string TenantId { get; set;}
    public string ClientId { get; set;}
    public string ClientSecret { get; set;}

    public string GetEntityPath()
    {
        if (AuthMode != AuthenticationMode.ConnectionString) return QueueName;

        var sb = ServiceBusConnectionStringProperties.Parse(ConnectionString);
        return sb.EntityPath;
    }
}
