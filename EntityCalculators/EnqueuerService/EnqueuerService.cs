using Contracts;
using Microsoft.ServiceFabric.Actors.Client;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Shared.Handlers;
using Shared.Helpers;
using Shared.Models;
using Shared.Services;
using Shared.Services.LogListeners;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;

namespace EnqueuerService
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    internal sealed class EnqueuerService : StatefulService, IEnqueuerService, IEntityActorEvents
    {
        private const string LOG_TAG = "Enqueuer Service";
        private const string ProcessQueueName = "ProcessQueue";

        private static TimeSpan TxTimeout = TimeSpan.FromSeconds(4);

        /// <summary>
        /// TODO: Temporary property-injection for an ILoggerService, IActorLocationService & IMosaicConnectorService until constructor injection is available.
        /// </summary>
        public ISettingService SettingService { private get; set; }
        public IActorLocationService ActorLocationService { private get; set; }

        public EnqueuerService(StatefulServiceContext context)
            : base(context)
        {
            this.SettingService = ServiceFactory.GetSettingService();
            this.ActorLocationService = ServiceFactory.GetActorLocationService();
        }

        /// <summary>
        /// Optional override to create listeners (e.g., HTTP, Service Remoting, WCF, etc.) for this service replica to handle client or user requests.
        /// </summary>
        /// <remarks>
        /// For more information on service communication, see http://aka.ms/servicefabricservicecommunication 
        /// and https://azure.microsoft.com/en-us/documentation/articles/service-fabric-reliable-services-communication-remoting/
        /// </remarks>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return new[]
                {
                    new ServiceReplicaListener(context =>
                        this.CreateServiceRemotingListener(context),
                        "rpcPrimaryEndpoint",
                        false)
                };
        }

        /// <summary>
        /// This is the main entry point for your service replica.
        /// This method executes when this replica of your service becomes primary and has write status.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service replica.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            // Gets (or creates) a replicated dictionary called "myDictionary" in this partition.
            var requests = await this.StateManager.GetOrAddAsync<IReliableQueue<EntityTransaction>>(ProcessQueueName);

            try
            {
                // This partition's replica continues processing until the replica is terminated.
                while (!cancellationToken.IsCancellationRequested)
                {
                    using (var tx = this.StateManager.CreateTransaction())
                    {
                        var result = await requests.TryDequeueAsync(tx, TxTimeout, cancellationToken);

                        if (result.HasValue)
                        {
                            var error = "";
                            var handler = HandlersFactory.GetProfilerHandler(SettingService);
                            handler.Start(LOG_TAG, "AcquiredQueueItem", GetServiceProperties());

                            try
                            {
                                EntityTransaction transaction = result.Value;
                                handler.Info("Acquired item business key: " + transaction.BusinessKey + " - entity type: " + transaction.EntityType);

                                IActorLocationService locator = ServiceFactory.GetActorLocationService();
                                IEntityActor entityActor = locator.Create<IEntityActor>(new Entity(transaction.EntityType, transaction.BusinessKey).GetPartitionKey(), Constants.ApplicationName);
                                await entityActor.SubscribeAsync<IEntityActorEvents>(this);
                                await entityActor.Process(transaction);

                            }
                            catch (Exception ex)
                            {
                                error = ex.Message;
                            }
                            finally
                            {
                                handler.Stop(error);
                            }
                        }

                        // This commits the dequeue operations.
                        // If the request to add the stock to the inventory service throws, this commit will not execute
                        // and the items will remain on the queue, so we can be sure that we didn't dequeue items
                        // that didn't get saved successfully in the inventory service.
                        // However there is a very small chance that the stock was added to the inventory service successfully,
                        // but service execution stopped before reaching this commit (machine crash, for example).
                        await tx.CommitAsync();
                    }

                    // Pause for 2 second before continue processing.
                    await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
                }
            }
            catch (Exception ex)
            {
            }

            // Pause for 1 second before continue processing.
            await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
        }

        protected override void OnAbort()
        {
            var handler = HandlersFactory.GetProfilerHandler(SettingService);
            handler.Start(LOG_TAG, "OnAbort", GetServiceProperties());
            handler.Stop();
            base.OnAbort();
        }

        // Enqueuer Interface Implementation
        public async Task<bool> EnqueueProcess(EntityTransaction transaction)
        {
            var error = "";
            var handler = HandlersFactory.GetProfilerHandler(SettingService);
            handler.Start(LOG_TAG, "EnqueueProcess", GetServiceProperties());
            bool blReturn = false;

            try
            {
                handler.Info("Processing business key: " + transaction.BusinessKey + " - entity type: " + transaction.EntityType);

                // Gets (or creates) a replicated dictionary called "ProcessQueue" in this partition.
                var requests = await this.StateManager.GetOrAddAsync<IReliableQueue<EntityTransaction>>(ProcessQueueName);

                using (var tx = this.StateManager.CreateTransaction())
                {
                    await requests.EnqueueAsync(tx, transaction);
                    await tx.CommitAsync();
                }

                blReturn = true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
            }
            finally
            {
                handler.Stop(error);
            }

            return blReturn;
        }

        // IEntityActorEvents Interface Implementation
        public void MeasuresRecalculated(Entity entity, int purchases, int cancellations, int netSales, double netRevenue, double tax, double shipping)
        {
            // TODO: Not really sure how useful to get this here. But I did it to illustrate events.
            var error = "";
            var handler = HandlersFactory.GetProfilerHandler(SettingService);
            handler.Start(LOG_TAG, "MeasuresRecalculated", GetServiceProperties());

            try
            {
                var message = string.Format("Measures {0} - {1} - {2} - {3} - {4} - {5} - {6} - {7}", entity.Type, entity.BusinessKey, purchases, cancellations, netSales, netRevenue, tax, shipping);
                handler.Info(message);
            }
            catch (Exception ex)
            {
                error = ex.Message;
            }
            finally
            {
                handler.Stop(error);
            }
        }

        // PRIVATE
        private Dictionary<string, string> GetServiceProperties()
        {
            Dictionary<string, string> props = new Dictionary<string, string>();
            props.Add(Constants.ServicePropType, GetType().ToString());
            props.Add(Constants.ServicePropId, Context.ReplicaOrInstanceId.ToString());
            props.Add(Constants.ServicePropApplicationType, Context.CodePackageActivationContext.ApplicationTypeName);
            props.Add(Constants.ServicePropApplicationName, Context.CodePackageActivationContext.ApplicationName);
            props.Add(Constants.ServicePropServiceType, Context.ServiceTypeName);
            props.Add(Constants.ServicePropServiceName, Context.ServiceName.ToString());
            props.Add(Constants.ServicePropPartitionId, Context.PartitionId + "");
            props.Add(Constants.ServicePropReplicationId, Context.ReplicaId + "");
            props.Add(Constants.ServicePropNode, Context.NodeContext.NodeName);
            return props;
        }
    }
}
