using Bogus;
using Radix.Samples.Dotnet.Contracts;
using System;
using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

var builder = Host.CreateApplicationBuilder();
builder.Configuration.AddJsonFile("appsettings.local.json", optional: true);
builder.Services.AddOptions<OrderQueueOptions>().Bind(builder.Configuration.GetSection(nameof(OrderQueueOptions)));
builder.Services.AddOrderQueueServices();
var app = builder.Build();
var sender = app.Services.GetRequiredService<ServiceBusSender>();

Console.WriteLine("Let's queue some orders, how many do you want?");
var orderCount = ReadNumber();

for (var i = 0; i < orderCount; i++)
{
    var f = new Faker();
    var customer = new Customer(f.Name.FirstName(), f.Name.LastName());
    var order = new Order(Guid.NewGuid().ToString(),f.Random.Int(),f.Commerce.Product(), customer);

    var rawOrder = JsonSerializer.Serialize(order);
    var orderMessage = new ServiceBusMessage(rawOrder);

    Console.WriteLine($"Queuing order {order.Id} - A {order.ArticleNumber} for {order.Customer.FirstName} {order.Customer.LastName}");
    await sender.SendMessageAsync(orderMessage);
}

Console.WriteLine("That's it, see you later!");
return;


static int ReadNumber()
{
    var rawAmount = Console.ReadLine();
    if (int.TryParse(rawAmount, out var amount))
    {
        return amount;
    }

    Console.WriteLine("That's not a valid amount, let's try that again");
    return ReadNumber();
}
