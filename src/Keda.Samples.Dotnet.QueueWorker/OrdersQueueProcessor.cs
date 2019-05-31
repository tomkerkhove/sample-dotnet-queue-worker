using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Keda.Samples.Dotnet.QueueWorker.Contracts;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Keda.Samples.Dotnet.QueueWorker
{
    public class OrdersQueueProcessor : QueueWorker<Order>
    {
        public OrdersQueueProcessor(IConfiguration configuration, ILogger<OrdersQueueProcessor> logger)
            : base(configuration, logger)
        {
        }

        protected override async Task ProcessMessage(Order order, string messageId, Message.SystemPropertiesCollection systemProperties, IDictionary<string, object> userProperties, CancellationToken cancellationToken)
        {
            Logger.LogInformation("Processing order {OrderId} for {OrderAmount} units of {OrderArticle} bought by {CustomerFirstName} {CustomerLastName} at: {Time}", order.Id, order.Amount, order.ArticleNumber, order.Customer.FirstName, order.Customer.LastName, DateTimeOffset.UtcNow);

            await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);

            Logger.LogInformation("Order {OrderId} processed at: {Time}", order.Id, DateTimeOffset.UtcNow);
        }
    }
}