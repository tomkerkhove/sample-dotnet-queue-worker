using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Radix.Samples.Dotnet.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Radix.Samples.Dotnet.OrderProcessor;

public class OrdersQueueProcessor(ServiceBusProcessor processor, ILogger<OrdersQueueProcessor> logger) : QueueWorker<Order>(processor, logger)
{
    protected override async Task ProcessMessage(Order order, string messageId, IReadOnlyDictionary<string, object> userProperties, CancellationToken cancellationToken)
    {
        logger.LogInformation("Processing order {OrderId} for {OrderAmount} units of {OrderArticle} bought by {CustomerFirstName} {CustomerLastName}", order.Id, order.Amount, order.ArticleNumber, order.Customer.FirstName, order.Customer.LastName);

        await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);

        logger.LogInformation("Order {OrderId} processed", order.Id);
    }
}
