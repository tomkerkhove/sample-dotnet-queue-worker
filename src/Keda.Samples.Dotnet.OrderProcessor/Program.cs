using Keda.Samples.Dotnet.Contracts;
using Keda.Samples.Dotnet.OrderProcessor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder();
builder.Services.AddOrderQueueServices();
builder.Services.AddHostedService<OrdersQueueProcessor>();

builder.Build().Run();
