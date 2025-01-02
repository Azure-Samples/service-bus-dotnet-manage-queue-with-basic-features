// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Azure;
using Azure.Core;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.Resources;
using Azure.ResourceManager.Samples.Common;
using Azure.ResourceManager.ServiceBus;
using Azure.ResourceManager.ServiceBus.Models;

namespace ServiceBusQueueBasic
{
    public class Program
    {
        /**
         * Azure Service Bus basic scenario sample.
         * - Create namespace with a queue.
         * - Add another queue in same namespace.
         * - Update Queue.
         * - Update namespace
         * - List namespaces
         * - List queues
         * - Get default authorization rule.
         * - Regenerate the keys in the authorization rule.
         * - Get the keys from authorization rule to connect to queue.
         * - Delete queue
         * - Delete namespace
         */
        private static ResourceIdentifier? _resourceGroupId = null;
        public static async Task RunSample(ArmClient client)
        {
            try
            {
                //============================================================

                // Create a namespace.

                // Get default subscription
                SubscriptionResource subscription = await client.GetDefaultSubscriptionAsync();

                // Create a resource group in the USWest region
                var rgName = Utilities.CreateRandomName("rgSB01_");
                Utilities.Log("Creating resource group with name : " + rgName);
                var rgLro = await subscription.GetResourceGroups().CreateOrUpdateAsync(WaitUntil.Completed, rgName, new ResourceGroupData(AzureLocation.WestUS));
                var resourceGroup = rgLro.Value;
                _resourceGroupId = resourceGroup.Id;
                Utilities.Log("Created resource group with name: " + resourceGroup.Data.Name + "...");

                //create namespace and wait for completion
                var nameSpaceName = Utilities.CreateRandomName("nameSpace");
                Utilities.Log("Creating namespace " + nameSpaceName + " in resource group " + rgName + "...");
                var namespaceCollection = resourceGroup.GetServiceBusNamespaces();
                var data = new ServiceBusNamespaceData(AzureLocation.WestUS)
                {
                    Sku = new ServiceBusSku(ServiceBusSkuName.Basic),
                    Location = AzureLocation.WestUS,
                };
                var serviceBusNamespace = (await namespaceCollection.CreateOrUpdateAsync(WaitUntil.Completed, nameSpaceName, data)).Value;
                Utilities.Log("Created service bus " + serviceBusNamespace.Data.Name);
                
                //Create a queue
                var queue1Name = Utilities.CreateRandomName("queue1_");
                Utilities.Log("Creating queue...");
                var queueCollection = serviceBusNamespace.GetServiceBusQueues();
                var queueData = new ServiceBusQueueData()
                {
                    MaxSizeInMegabytes = 1024,
                };
                var queue = (await queueCollection.CreateOrUpdateAsync(WaitUntil.Completed, queue1Name, queueData)).Value;
                Utilities.Log("Created queue with name : " + queue.Data.Name);

                //============================================================
                
                // Create a second queue in same namespace
                var queue2Name = Utilities.CreateRandomName("queue2_");
                Utilities.Log("Creating second queue " + queue2Name + " in namespace " + nameSpaceName + "...");
                var queue2Data = new ServiceBusQueueData()
                {
                    MaxSizeInMegabytes = 2048,
                    LockDuration = TimeSpan.FromSeconds(20),
                    DeadLetteringOnMessageExpiration = true,
                };
                var queue2 = (await queueCollection.CreateOrUpdateAsync(WaitUntil.Completed, queue2Name, queue2Data)).Value;
                Utilities.Log("Created second queue in namespace with name : " + queue2.Data.Name);

                //============================================================

                // Get and update second queue.
                Utilities.Log("Updating second queue to change its size in MB...");
                var secondQueue = (await serviceBusNamespace.GetServiceBusQueueAsync(queue2Name)).Value;
                var updateData = new ServiceBusQueueData()
                {
                    MaxSizeInMegabytes = 3072,
                    LockDuration = TimeSpan.FromSeconds(20),
                    DeadLetteringOnMessageExpiration = true,
                };
                _ = await secondQueue.UpdateAsync(WaitUntil.Completed,updateData);
                Utilities.Log("Updated second queue to change its size in MB");

                //=============================================================
                
                // Update namespace
                Utilities.Log("Updating sku of namespace " + serviceBusNamespace.Data.Name + "...");
                var patch = new ServiceBusNamespacePatch(AzureLocation.EastUS)
                {
                    Sku = new ServiceBusSku(ServiceBusSkuName.Standard)
                };
                _ = await serviceBusNamespace.UpdateAsync(patch);
                Utilities.Log("Updated sku of namespace " + serviceBusNamespace.Data.Name);

                //=============================================================

                // List namespaces
                Utilities.Log("List of namespaces in resource group " + rgName + "...");
                var serviceBusNamespaces = resourceGroup.GetServiceBusNamespaces().ToList();
                Utilities.Log("Number of queues in namespace :" + serviceBusNamespaces.Count());

                //=============================================================
                
                // List queues in namespaces
                Utilities.Log("Getting number of queues rule for namespace...");
                var queues = serviceBusNamespace.GetServiceBusQueues().ToList();
                Utilities.Log("Number of queues in namespace :" + queues.Count());

                //=============================================================

                // Get connection string for default authorization rule of namespace
                Utilities.Log("Getting number of authorization rule for namespace...");
                var namespaceAuthorizationRules = serviceBusNamespace.GetServiceBusNamespaceAuthorizationRules().ToList();
                Utilities.Log("Number of authorization rule for namespace :" + namespaceAuthorizationRules.Count());

                Utilities.Log("Getting keys for authorization rule ...");
                _ = namespaceAuthorizationRules.FirstOrDefault().GetKeys();
                Utilities.Log("Got keys for authorization rule ...");

                Utilities.Log("Regenerating secondary key for authorization rule ...");
                var content = new ServiceBusRegenerateAccessKeyContent(ServiceBusAccessKeyType.SecondaryKey);
                _ = await namespaceAuthorizationRules.FirstOrDefault().RegenerateKeysAsync(content);
                Utilities.Log("Regenerated secondary key for authorization rule ...");

                //=============================================================

                // Delete a queue and namespace
                Utilities.Log("Deleting queue " + queue1Name + "in namespace " + nameSpaceName + "...");
                _ = await queue.DeleteAsync(WaitUntil.Completed);
                Utilities.Log("Deleted queue " + queue1Name);

                Utilities.Log("Deleting namespace " + nameSpaceName + "...");
                try
                {
                    _ = await serviceBusNamespace.DeleteAsync(WaitUntil.Completed);
                }
                catch (Exception)
                {
                }
                Console.WriteLine("Deleted namespace " + nameSpaceName + "...");
            }
            finally
            {
                try
                {
                    if (_resourceGroupId is not null)
                    {
                        Utilities.Log($"Deleting Resource Group: {_resourceGroupId}");
                        await client.GetResourceGroupResource(_resourceGroupId).DeleteAsync(WaitUntil.Completed);
                        Utilities.Log($"Deleted Resource Group: {_resourceGroupId}");
                    }
                }
                catch (NullReferenceException)
                {
                    Utilities.Log("Did not create any resources in Azure. No clean up is necessary");
                }
                catch (Exception g)
                {
                    Utilities.Log(g);
                }
            }
        }
        public static async Task Main(string[] args)
        {
            try
            {
                var clientId = Environment.GetEnvironmentVariable("CLIENT_ID");
                var clientSecret = Environment.GetEnvironmentVariable("CLIENT_SECRET");
                var tenantId = Environment.GetEnvironmentVariable("TENANT_ID");
                var subscription = Environment.GetEnvironmentVariable("SUBSCRIPTION_ID");
                ClientSecretCredential credential = new ClientSecretCredential(tenantId, clientId, clientSecret);
                ArmClient client = new ArmClient(credential, subscription);
                await RunSample(client);
            }
            catch (Exception e)
            {
                Utilities.Log(e);
            }
        }
    }
}
