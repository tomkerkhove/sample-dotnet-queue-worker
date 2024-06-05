using Radix.Samples.Dotnet.Contracts;
using Radix.Samples.Dotnet.OrderProcessor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Hosting;
//
// var builder = new HostBuilder()
//         .ConfigureServices((context, services) =>
//         {
//             services.AddOrderQueueServices();
//             services.AddAzureClients(clientBuilder =>
//             {
//                 clientBuilder.AddServiceBusClient();
//             });
//         })
//
//     ;

var builder = Host.CreateApplicationBuilder();
builder.Configuration.AddJsonFile("appsettings.local.json", optional: true);
builder.Services.AddOptions<OrderQueueOptions>().BindConfiguration(nameof(OrderQueueOptions));
builder.Services.AddOrderQueueServices();
builder.Services.AddHostedService<OrdersQueueProcessor>();

builder.Build().Run();
