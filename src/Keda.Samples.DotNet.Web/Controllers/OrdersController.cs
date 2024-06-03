using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Keda.Samples.Dotnet.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Keda.Samples.DotNet.Web.Controllers;

/// <summary>
///     API endpoint to manage orders
/// </summary>
[ApiController]
[Route("api/v1/orders")]
public class OrdersController : ControllerBase
{

    /// <summary>
    ///     Create Order
    /// </summary>
    [HttpPost(Name = "Order_Create")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody, Required] Order order, [FromServices] ServiceBusSender sender, CancellationToken ct)
    {
        var jsonString = JsonSerializer.Serialize(order);
        var orderMessage = new ServiceBusMessage(jsonString);

        await sender.SendMessageAsync(orderMessage, ct);

        return Accepted();
    }
}