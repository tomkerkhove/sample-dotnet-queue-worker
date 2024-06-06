using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Threading;
using Azure.Messaging.ServiceBus;
using Radix.Samples.Dotnet.Contracts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder();
builder.Configuration.AddJsonFile("appsettings.local.json", optional: true);
builder.Services.AddOptions<OrderQueueOptions>().BindConfiguration(nameof(OrderQueueOptions));
builder.Services.AddLogging(lb => lb.AddConsole());
builder.Services.AddControllers();
builder.Services.AddRazorPages();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddOrderQueueServices();


var app = builder.Build();
if (app.Environment.IsDevelopment())app.UseDeveloperExceptionPage();
else app.UseExceptionHandler("/Error");

app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapRazorPages();
app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/api/v1/queue",
    async ([FromServices] ServiceBusReceiver receiver, CancellationToken ct) =>
    new QueueStatus((await receiver.PeekMessagesAsync(100, null, ct)).Count));

app.MapPost("api/v1/orders",
    async ([FromBody, Required] Order order, [FromServices] ServiceBusSender sender, CancellationToken ct) =>
    {
        var jsonString = JsonSerializer.Serialize(order);
        var orderMessage = new ServiceBusMessage(jsonString);
        await sender.SendMessageAsync(orderMessage, ct);

        return TypedResults.Accepted(order.Id);
    }).WithName("Order_Create");

await app.RunAsync();
