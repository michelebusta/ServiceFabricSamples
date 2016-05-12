using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Shared.Helpers;
using Shared.Models;
using Shared.Services.LogSources;
using System;
using System.Threading.Tasks;

namespace Shared.Services.LogListeners
{
    public class AzureStorageLogListener : GenericLogListener
    {
        public AzureStorageLogListener(ILogMessageEventSource source, ISettingService setting) : 
            base(source, setting) 
        {
        }

        public override async Task ProcessLogMessage(LogMessage message)
        {
            // Create a new oms entity.
            OmsTableEntity oms = new OmsTableEntity(message.Tag);
            oms.LogLevel = message.Level.ToString();
            oms.CorrelationId = message.CorrelationId;
            oms.Method = message.Method;
            oms.Message = message.Message;
            oms.LogType = message.Type.ToString();
            oms.Duration = message.Duration;
            if (message.Properties != null)
            {
                string type = "";
                string id = "";
                string applicationType = "";
                string applicationName = "";
                string serviceType = "";
                string serviceName = "";
                string partitionId = "";
                string replicationId = "";
                string node = "";

                message.Properties.TryGetValue(Constants.ServicePropType, out type);
                message.Properties.TryGetValue(Constants.ServicePropId, out id);
                message.Properties.TryGetValue(Constants.ServicePropApplicationType, out applicationType);
                message.Properties.TryGetValue(Constants.ServicePropApplicationName, out applicationName);
                message.Properties.TryGetValue(Constants.ServicePropServiceType, out serviceType);
                message.Properties.TryGetValue(Constants.ServicePropServiceName, out serviceName);
                message.Properties.TryGetValue(Constants.ServicePropPartitionId, out partitionId);
                message.Properties.TryGetValue(Constants.ServicePropReplicationId, out replicationId);
                message.Properties.TryGetValue(Constants.ServicePropNode, out node);

                oms.ContextType = type;
                oms.ContextId = id;
                oms.ApplicationType = applicationType;
                oms.ApplicationName = applicationName;
                oms.ServiceType = serviceType;
                oms.ServiceName = serviceName;
                oms.PartitionId = partitionId;
                oms.ReplicationId = replicationId;
                oms.Node = node;
            }

            // Not ideal to pass the connection string ...but the setting service is the only thing that have them
            await AzureStorageManager.Current.InsertIntoOmsStorage(oms, TheSettingService.GetAzureStorageConnectionString(), TheSettingService.GetAzureStorageLogsTable());
        }
    }

    class AzureStorageManager
    {
        public static readonly AzureStorageManager Current = new AzureStorageManager();

        private CloudTable _omsTable;
        private string _connectionString;
        private string _omsTableName;

        // Instance constructor is private to enforce singleton semantics
        private AzureStorageManager()
        {
        }

        public async Task InsertIntoOmsStorage(OmsTableEntity omsEntity, string connectionString, string omsTableName)
        {
            if (_omsTable == null)
            {
                _connectionString = connectionString;
                _omsTableName = omsTableName;
                InitializeStorage();
            }

            try
            {
                // Create the TableOperation object that inserts the customer entity.
                TableOperation insertOperation = TableOperation.Insert(omsEntity);

                // Execute the insert operation.
                await _omsTable.ExecuteAsync(insertOperation);

            }
            catch (Exception ex)
            {
                /* TODO: Ignore for now */
            }
        }

        private void InitializeStorage()
        {
            try
            {
                var storageAccount = CloudStorageAccount.Parse(_connectionString);
                CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
                _omsTable = tableClient.GetTableReference(_omsTableName);
                _omsTable.CreateIfNotExists();
            }
            catch (Exception ex)
            {
                /* TODO: Ignore for now */
            }
        }
    }

    // Storage tables add timestamp automatically
    class OmsTableEntity : TableEntity
    {
        public OmsTableEntity(string tag)
        {
            this.PartitionKey = tag;
            this.RowKey = Guid.NewGuid().ToString();
        }

        public OmsTableEntity()
        {

        }

        public string LogLevel { get; set; }
        public string CorrelationId { get; set; }
        public string Method { get; set; }
        public string Message { get; set; }
        public string LogType { get; set; }
        public double Duration { get; set; }
        public string ContextType { get; set; }
        public string ContextId { get; set; }
        public string ApplicationType { get; set; }
        public string ApplicationName { get; set; }
        public string ServiceType { get; set; }
        public string ServiceName { get; set; }
        public string PartitionId { get; set; }
        public string ReplicationId { get; set; }
        public string Node { get; set; }
    }
}
