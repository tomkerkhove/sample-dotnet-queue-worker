# .NET Core worker processing Azure Service Bus Queue scaled by KEDA
A simple Docker container written in .NET that will receive messages from a Service Bus queue and scale via KEDA.

The message processor will receive a single message at a time (per instance), and sleep for 2 second to simulate performing work. When adding a massive amount of queue messages, KEDA will drive the container to scale out according to the event source (Service Bus Queue).

![Scenario](images/scenario.png)

> 💡 *If you want to learn how to scale this sample with KEDA 1.0, feel free to read about it [here](https://github.com/kedacore/sample-dotnet-worker-servicebus-queue/tree/keda-v1.0).*

## Radix Config Trigger details:

```yaml

  horizontalScaling:
    maxReplicas: 10
    minReplicas: 0 # When you are using atleast 1 non-resource based trigger, you can scale to 0 when possible!
    triggers:
     - name: azuresb
       azureServiceBus:
         namespace: <AzureServiceBusNamespace> #.servicebus.windows.net
         queueName: orders
         messageCount: 2 # How many messages should each replica handle? 

         # Workload Identity for KEDA to access service bus
         authentication:
           identity:
             azure:
               clientId: c2f17b62-7c2f-4541-acbc-22d7cfc66e0b

```

Currently Keda needs access to your servicebus to count messages, this has the side effect of other clients also being able to scale their apps based on messages in your queue, if they also know your ClientId.

To configure Keda, create a managed identity, and assign it a federated credential, like this if you are using Terraform:
```terraform
resource "azurerm_federated_identity_credential" "keda" {
  audience            = ["api://AzureADTokenExchange"]
  issuer              = local.radix_oidc_issuer_url # https://console.radix.equinor.com/about
  name                = "keda"
  resource_group_name = azurerm_servicebus_namespace.main.resource_group_name
  subject             = "system:serviceaccount:keda:keda-operator" # RADIX Keda operator
  parent_id           = azurerm_user_assigned_identity.main.id # Your managed identity that have access to the ServiceBus
}
```
