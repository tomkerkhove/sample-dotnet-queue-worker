using Azure.Messaging.ServiceBus;
using Keda.Samples.Dotnet.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace Keda.Samples.DotNet.Web.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class QueueController : ControllerBase
    {
        [HttpGet]
        [ApiExplorerSettings(IgnoreApi = true)]
        public QueueStatus Get([FromServices] ServiceBusReceiver  receiver)
        {
            return new QueueStatus(receiver.PrefetchCount);
        }
    }
}
