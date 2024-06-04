using Bogus;
using Keda.Samples.Dotnet.Contracts;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder();
builder.Services.AddOptions<OrderQueueOptions>().Bind(builder.Configuration.GetSection(nameof(OrderQueueOptions)));
builder.Services.AddOrderQueueServices();
var app = builder.Build();
var sender = app.Services.GetRequiredService<ServiceBusSender>();

Console.WriteLine("Let's queue some orders, how many do you want?");

var requestedAmount = DetermineOrderAmount();
await QueueOrders(requestedAmount, sender);

Console.WriteLine("That's it, see you later!");

static async Task QueueOrders(int requestedAmount, ServiceBusSender sender)
{

    for (int currentOrderAmount = 0; currentOrderAmount < requestedAmount; currentOrderAmount++)
    {
        var order = GenerateOrder();
        var rawOrder = JsonSerializer.Serialize(order);
        var orderMessage = new ServiceBusMessage(rawOrder);

        Console.WriteLine($"Queuing order {order.Id} - A {order.ArticleNumber} for {order.Customer.FirstName} {order.Customer.LastName}");
        await sender.SendMessageAsync(orderMessage);
    }
}

static Order GenerateOrder()
{
    var customerGenerator = new Faker<Customer>()
        .RuleFor(u => u.FirstName, (f, u) => f.Name.FirstName())
        .RuleFor(u => u.LastName, (f, u) => f.Name.LastName());

    var orderGenerator = new Faker<Order>()
        .RuleFor(u => u.Customer, () => customerGenerator)
        .RuleFor(u => u.Id, f => Guid.NewGuid().ToString())
        .RuleFor(u => u.Amount, f => f.Random.Int())
        .RuleFor(u => u.ArticleNumber, f => f.Commerce.Product());

    return orderGenerator.Generate();
}

static int DetermineOrderAmount()
{
    var rawAmount = Console.ReadLine();
    if (int.TryParse(rawAmount, out int amount))
    {
        return amount;
    }

    Console.WriteLine("That's not a valid amount, let's try that again");
    return DetermineOrderAmount();
}
