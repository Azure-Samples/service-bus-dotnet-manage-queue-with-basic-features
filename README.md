---
page_type: sample
languages:
- java
products:
- azure
services: Service-Bus
platforms: dotnet
author: yaohaizh
---

# Getting started on managing Service Bus Queues with basic features in C# #

          Azure Service Bus basic scenario sample.
          - Create namespace with a queue.
          - Add another queue in same namespace.
          - Update Queue.
          - Update namespace
          - List namespaces
          - List queues
          - Get default authorization rule.
          - Regenerate the keys in the authorization rule.
          - Get the keys from authorization rule to connect to queue.
          - Send a "Hello" message to queue using Data plan sdk for Service Bus.
          - Delete queue
          - Delete namespace


## Running this Sample ##

To run this sample:

Set the environment variable `AZURE_AUTH_LOCATION` with the full path for an auth file. See [how to create an auth file](https://github.com/Azure/azure-libraries-for-net/blob/master/AUTH.md).

    git clone https://github.com/Azure-Samples/service-bus-dotnet-manage-queue-with-basic-features.git

    cd service-bus-dotnet-manage-queue-with-basic-features

    dotnet build

    bin\Debug\net452\ServiceBusQueueBasic.exe

## More information ##

[Azure Management Libraries for C#](https://github.com/Azure/azure-sdk-for-net/tree/Fluent)
[Azure .Net Developer Center](https://azure.microsoft.com/en-us/develop/net/)
If you don't have a Microsoft Azure subscription you can get a FREE trial account [here](http://go.microsoft.com/fwlink/?LinkId=330212)

---

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.