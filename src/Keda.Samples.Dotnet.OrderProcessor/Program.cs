using Keda.Samples.Dotnet.Contracts;
using Keda.Samples.Dotnet.OrderProcessor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder();
builder.Configuration.AddJsonFile("appsettings.local.json", optional: true).AddEnvironmentVariables();
builder.Services.AddOptions<OrderQueueOptions>().BindConfiguration(nameof(OrderQueueOptions));
builder.Services.AddOrderQueueServices();
builder.Services.AddHostedService<OrdersQueueProcessor>();

builder.Build().Run();
