using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Keda.Samples.Dotnet.OrderProcessor;

public abstract class QueueWorker<TMessage>(
    IServiceScopeFactory serviceScopeFactory,
    ILogger<QueueWorker<TMessage>> logger)
    : BackgroundService
{

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = serviceScopeFactory.CreateScope();
        
        var messageProcessor = scope.ServiceProvider.GetRequiredService<ServiceBusProcessor>();
            
        messageProcessor.ProcessMessageAsync += HandleMessageAsync;
        messageProcessor.ProcessErrorAsync += HandleReceivedExceptionAsync;

        logger.LogInformation($"Starting message pump on queue {messageProcessor.EntityPath} in namespace {messageProcessor.FullyQualifiedNamespace}");
        await messageProcessor.StartProcessingAsync(stoppingToken);
        logger.LogInformation("Message pump started");

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromSeconds(1));
        }

        logger.LogInformation("Closing message pump");
        await messageProcessor.CloseAsync(cancellationToken: stoppingToken);
        logger.LogInformation("Message pump closed : {Time}", DateTimeOffset.UtcNow);
    }

        
    private async Task HandleMessageAsync (ProcessMessageEventArgs processMessageEventArgs)
    {
        try
        {
            var rawMessageBody = Encoding.UTF8.GetString(processMessageEventArgs.Message.Body);
            logger.LogInformation("Received message {MessageId} with body {MessageBody}",
                processMessageEventArgs.Message.MessageId, rawMessageBody);

            var order = JsonSerializer.Deserialize<TMessage>(rawMessageBody);
            if (order != null)
            {
                await ProcessMessage(order, processMessageEventArgs.Message.MessageId,
                    processMessageEventArgs.Message.ApplicationProperties,
                    processMessageEventArgs.CancellationToken);
            }
            else
            {
                logger.LogError(
                    "Unable to deserialize to message contract {ContractName} for message {MessageBody}",
                    typeof(TMessage), rawMessageBody);
            }

            logger.LogInformation("Message {MessageId} processed", processMessageEventArgs.Message.MessageId);

            await processMessageEventArgs.CompleteMessageAsync(processMessageEventArgs.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unable to handle message");
        }
    }

    private Task HandleReceivedExceptionAsync(ProcessErrorEventArgs exceptionEvent)
    {
        logger.LogError(exceptionEvent.Exception, "Unable to process message");
        return Task.CompletedTask;
    }

    protected abstract Task ProcessMessage(TMessage order, string messageId, IReadOnlyDictionary<string, object> userProperties, CancellationToken cancellationToken);
}