using Contracts;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Data;
using Shared.Handlers;
using Shared.Helpers;
using Shared.Models;
using Shared.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;

namespace EntityActor
{
    /// <remarks>
    /// This class represents an actor.
    /// Every ActorID maps to an instance of this class.
    /// The StatePersistence attribute determines persistence and replication of actor state:
    ///  - Persisted: State is written to disk and replicated.
    ///  - Volatile: State is kept in memory only and replicated.
    ///  - None: State is kept in memory only and not replicated.
    /// </remarks>
    [StatePersistence(StatePersistence.Persisted)]
    internal class EntityActor : Actor, IEntityActor, IRemindable
    {
        const string LOG_TAG = "EntityActor";
        const string ActorStatePropertyName = "MyState";

        const string ReprocessReminder = "ReprocessReminder";

        /// <summary>
        /// TODO: Temporary property-injection for an ILoggerService, IActorLocationService & IMosaicConnectorService until constructor injection is available.
        /// </summary>
        public IActorLocationService ActorLocationService { private get; set; }
        public ISettingService SettingService { private get; set; }
        public IOltpConnectorService OltpConnectorService { private get; set; }

        /// <summary>
        /// This method is called whenever an actor is activated.
        /// An actor is activated the first time any of its methods are invoked.
        /// </summary>
        protected override async Task OnActivateAsync()
        {
            this.ActorLocationService = ServiceFactory.GetActorLocationService();
            this.SettingService = ServiceFactory.GetSettingService();
            this.OltpConnectorService = ServiceFactory.GetOltpConnectorService(this.SettingService);

            var error = "";
            var message = "";
            var handler = HandlersFactory.GetProfilerHandler(SettingService);
            handler.Start(LOG_TAG, "OnActivateAsync", GetActorProperties());

            try
            {
                EntityActorState state = await GetEntityStateAsync();
                if (state == null)
                {
                    // This is the first time this actor has ever been activated.
                    state = new EntityActorState();

                    // Set the actor's initial state values.
                    state.Purchases = 0;
                    state.Cancellations = 0;
                    state.SoldItems = 0;
                    state.Revenue = 0;
                    state.Tax = 0;
                    state.Shipping = 0;

                    // Use the actor ID (which packs the enity type and business key) to load the state entity
                    List<string> actorIdParts = this.Id.GetStringId().Split('|').Select(s => s).ToList();
                    if (actorIdParts == null || actorIdParts.Count != 2)
                    {
                        message = string.Format("Actor ID {0} state could not be determined!!!!", this.Id.GetStringId()); 
                        handler.Info(message);
                        state.Entity = new Entity(EntityTypes.SalesOffice, -1);
                    }
                    else
                    {
                        state.Entity = new Entity();

                        try
                        {
                            EntityTypes type = (EntityTypes)Int32.Parse(actorIdParts.ToArray()[0]);
                            int businessKey = Int32.Parse(actorIdParts.ToArray()[1]);
                            var entity = await OltpConnectorService.GetEntity(type, businessKey);
                            if (entity != null)
                                state.Entity = await OltpConnectorService.GetEntity(type, businessKey);
                        }
                        catch (Exception)
                        {
                            message = string.Format("Actor ID {0} state could not be parsed to determine actor entity!!!!", this.Id.GetStringId());
                            handler.Info(message);
                        }
                    }

                    // Make sure the state is saved
                    await SetEntityStateAsync(state);
                }

                state = await this.StateManager.GetStateAsync<EntityActorState>(ActorStatePropertyName);
                message = string.Format("Actor {0} activated", this.Id.GetStringId());
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

        protected override Task OnDeactivateAsync()
        {
            var handler = HandlersFactory.GetProfilerHandler(SettingService);
            handler.Start(LOG_TAG, "OnDeactivateAsync", GetActorProperties());
            handler.Stop();
            return base.OnDeactivateAsync();
        }

        public async Task Process(EntityTransaction transaction)
        {
            var error = "";
            var handler = HandlersFactory.GetProfilerHandler(SettingService);
            handler.Start(LOG_TAG, "Process", GetActorProperties());

            try
            {
                if (transaction == null)
                {
                    handler.Info("Process transactions is null");
                    return;
                }

                // Scheduler a reminder to reprocess myself and my parent
                // TODO: I think the actor does not evict from memory
                // as long as there are reminders. To garbage collect, reminders
                // have to be removed
                await this.RegisterReminderAsync(
                    ReprocessReminder,
                    ObjectToByteArray(transaction),
                    TimeSpan.FromSeconds(0),  // Remind immediately
                    TimeSpan.FromDays(1));    // Remind again in 1 day - useless of course!!
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

        public async Task<Entity> GetEntity()
        {
            var error = "";
            var handler = HandlersFactory.GetProfilerHandler(SettingService);
            handler.Start(LOG_TAG, "GetEntity", GetActorProperties());

            try
            {
                var state = await this.StateManager.GetStateAsync<EntityActorState>(ActorStatePropertyName);
                return state.Entity;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return null;
            }
            finally
            {
                handler.Stop(error);
            }
        }

        public async Task<Entity> GetParent()
        {
            var error = "";
            var handler = HandlersFactory.GetProfilerHandler(SettingService);
            handler.Start(LOG_TAG, "GetParent", GetActorProperties());

            try
            {
                var state = await this.StateManager.GetStateAsync<EntityActorState>(ActorStatePropertyName);
                return state.Entity.Parent;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return null;
            }
            finally
            {
                handler.Stop(error);
            }
        }

        public async Task<List<Entity>> GetChildren()
        {
            var error = "";
            var handler = HandlersFactory.GetProfilerHandler(SettingService);
            handler.Start(LOG_TAG, "GetChildren", GetActorProperties());

            try
            {
                var state = await this.StateManager.GetStateAsync<EntityActorState>(ActorStatePropertyName);
                return await OltpConnectorService.GetChildren(state.Entity.Type, state.Entity.BusinessKey);
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return null;
            }
            finally
            {
                handler.Stop(error);
            }
        }

        public async Task<EntityView> GetView()
        {
            var error = "";
            var handler = HandlersFactory.GetProfilerHandler(SettingService);
            handler.Start(LOG_TAG, "GetView", GetActorProperties());

            try
            {
                var state = await this.StateManager.GetStateAsync<EntityActorState>(ActorStatePropertyName);
                return new EntityView()
                {
                    Type = state.Entity.Type,
                    BusinessKey = state.Entity.BusinessKey,
                    Name = state.Entity.Name,
                    Purchases = state.Purchases,
                    Cancellations = state.Cancellations,
                    SoldItems = state.SoldItems,
                    Revenue = state.Revenue,
                    Tax = state.Tax,
                    Shipping = state.Shipping
                };
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return null;
            }
            finally
            {
                handler.Stop(error);
            }
        }

        public async Task<int> GetPurchases()
        {
            var error = "";
            var handler = HandlersFactory.GetProfilerHandler(SettingService);
            handler.Start(LOG_TAG, "GetPurchases", GetActorProperties());

            try
            {
                var state = await this.StateManager.GetStateAsync<EntityActorState>(ActorStatePropertyName);
                return state.Purchases;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return 0;
            }
            finally
            {
                handler.Stop(error);
            }
        }

        public async Task<int> GetCancellations()
        {
            var error = "";
            var handler = HandlersFactory.GetProfilerHandler(SettingService);
            handler.Start(LOG_TAG, "GetCancellations", GetActorProperties());

            try
            {
                var state = await this.StateManager.GetStateAsync<EntityActorState>(ActorStatePropertyName);
                return state.Cancellations;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return 0;
            }
            finally
            {
                handler.Stop(error);
            }
        }

        public async Task<int> GetItemsSold()
        {
            var error = "";
            var handler = HandlersFactory.GetProfilerHandler(SettingService);
            handler.Start(LOG_TAG, "GetItemsSold", GetActorProperties());

            try
            {
                var state = await this.StateManager.GetStateAsync<EntityActorState>(ActorStatePropertyName);
                return state.SoldItems;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return 0;
            }
            finally
            {
                handler.Stop(error);
            }
        }

        public async Task<double> GetRevenue()
        {
            var error = "";
            var handler = HandlersFactory.GetProfilerHandler(SettingService);
            handler.Start(LOG_TAG, "GetRevenue", GetActorProperties());

            try
            {
                var state = await this.StateManager.GetStateAsync<EntityActorState>(ActorStatePropertyName);
                return state.Revenue;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return 0;
            }
            finally
            {
                handler.Stop(error);
            }
        }

        public async Task<double> GetTax()
        {
            var error = "";
            var handler = HandlersFactory.GetProfilerHandler(SettingService);
            handler.Start(LOG_TAG, "GetTax", GetActorProperties());

            try
            {
                var state = await this.StateManager.GetStateAsync<EntityActorState>(ActorStatePropertyName);
                return state.Tax;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return 0;
            }
            finally
            {
                handler.Stop(error);
            }
        }

        public async Task<double> GetShipping()
        {
            var error = "";
            var handler = HandlersFactory.GetProfilerHandler(SettingService);
            handler.Start(LOG_TAG, "GetShipping", GetActorProperties());

            try
            {
                var state = await this.StateManager.GetStateAsync<EntityActorState>(ActorStatePropertyName);
                return state.Shipping;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return 0;
            }
            finally
            {
                handler.Stop(error);
            }
        }

        // IRemindable Interface Implementation
        public async Task ReceiveReminderAsync(string reminderName, byte[] context, TimeSpan dueTime, TimeSpan period)
        {
            var error = "";
            var handler = HandlersFactory.GetProfilerHandler(SettingService);
            handler.Start(LOG_TAG, "ReceiveReminderAsync", GetActorProperties());

            try
            {
                handler.Info("Reminder " + reminderName);
                var state = await this.StateManager.GetStateAsync<EntityActorState>(ActorStatePropertyName);
                EntityTransaction transaction = (EntityTransaction)ByteArrayToObject(context);
                if (transaction == null)
                {
                    handler.Info("Transaction is null");
                }

                switch (reminderName)
                {
                    case ReprocessReminder:
                        {
                            // Unregister the reminder so the actor will be garbage collected
                            IActorReminder reminder = this.GetReminder(ReprocessReminder);
                            await this.UnregisterReminderAsync(reminder);

                            // Process this transaction
                            if (transaction.TransactionType == TransactionTypes.Purchase)
                            {
                                state.Purchases++;
                                state.SoldItems += transaction.SoldItems;
                                state.Revenue += transaction.Revenue;
                                state.Tax += transaction.Tax;
                                state.Shipping += transaction.Shipping;
                            }
                            else if (transaction.TransactionType == TransactionTypes.Cancellation)
                            {
                                state.Cancellations++;
                                state.SoldItems -= transaction.SoldItems;
                                state.Revenue -= transaction.Revenue;
                                state.Tax -= transaction.Tax;
                                state.Shipping -= transaction.Shipping;
                            }

                            // Make sure the state is saved
                            await SetEntityStateAsync(state);

                            // Publish an event that we are done processing
                            // Right now it is useless
                            // It is an excercise to see how events work
                            var ev = GetEvent<IEntityActorEvents>();
                            ev.MeasuresRecalculated(await GetEntity(), state.Purchases, state.Cancellations, state.SoldItems, state.Revenue, state.Tax, state.Shipping);

                            // If available, process the parent
                            var parent = await GetParent();
                            if (parent != null)
                            {
                                handler.Info("Parent actor type " + parent.Type);
                                IEntityActor parentActor = ActorLocationService.Create<IEntityActor>(parent.GetPartitionKey(), ApplicationName);
                                await parentActor.Process(transaction);
                            }

                            return;
                        }
                    default:
                        {
                            // We should never arrive here normally. The system won't call reminders that don't exist. 
                            // But for our own sake in case we add a new reminder somewhere and forget to handle it, this will remind us.
                            handler.Info("Unknown reminder: " + reminderName);
                            return;
                        }
                }
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

        public Dictionary<string, string> GetActorProperties()
        {
            Dictionary<string, string> props = new Dictionary<string, string>();
            props.Add(Constants.ServicePropType, GetType().ToString());
            props.Add(Constants.ServicePropId, Id.ToString());
            props.Add(Constants.ServicePropApplicationType, ActorService.Context.CodePackageActivationContext.ApplicationTypeName);
            props.Add(Constants.ServicePropApplicationName, ActorService.Context.CodePackageActivationContext.ApplicationName);
            props.Add(Constants.ServicePropServiceType, ActorService.Context.ServiceTypeName);
            props.Add(Constants.ServicePropServiceName, ActorService.Context.ServiceName.ToString());
            props.Add(Constants.ServicePropPartitionId, ActorService.Context.PartitionId + "");
            props.Add(Constants.ServicePropReplicationId, ActorService.Context.ReplicaId + "");
            props.Add(Constants.ServicePropNode, ActorService.Context.NodeContext.NodeName);
            return props;
        }

        // PRIVATE
        private async Task<EntityActorState> GetEntityStateAsync()
        {
            ConditionalValue<EntityActorState> stateResult = await this.StateManager.TryGetStateAsync<EntityActorState>(ActorStatePropertyName);
            if (stateResult.HasValue)
            {
                return stateResult.Value;
            }
            else
            {
                return null;
            }
        }

        private async Task SetEntityStateAsync(EntityActorState state)
        {
            await this.StateManager.SetStateAsync<EntityActorState>(ActorStatePropertyName, state);
            // Just to make sure though it is probably not needed
            await this.SaveStateAsync();
        }

        private byte[] ObjectToByteArray(Object obj)
        {
            if (obj == null)
                return null;

            BinaryFormatter bf = new BinaryFormatter();

            try
            {
                using (var ms = new MemoryStream())
                {
                    bf.Serialize(ms, obj);
                    return ms.ToArray();
                }

            }
            catch (Exception ex)
            {
                return null;
            }
        }

        // Convert a byte array to an Object
        private Object ByteArrayToObject(byte[] arrBytes)
        {
            try
            {
                using (var memStream = new MemoryStream())
                {
                    var binForm = new BinaryFormatter();
                    memStream.Write(arrBytes, 0, arrBytes.Length);
                    memStream.Seek(0, SeekOrigin.Begin);
                    var obj = binForm.Deserialize(memStream);
                    return obj;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
