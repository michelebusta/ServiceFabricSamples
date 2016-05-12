using Shared.Handlers;
using System;
using System.Fabric;

namespace Shared.Services
{
    public class SettingService : ISettingService
    {
        private const string LOG_TAG = "SettingService";

        private const string ConfigurationSectionName = "ServiceRunTimeConfig";
        private const string InstrumentationKey = "InstrumentationKey";
        private const string OltpConnectorKey = "OltpConnector";
        private const string OltpConnectionStringKey = "OltpConnectionString";
        private const string LogsStorageConnectionStringKey = "LogsStorageConnectionString";
        private const string LogsTableStorageNameKey = "LogsStorageTableName";
        private const string IsEtwLoggingKey = "IsEtwLogging";
        private const string IsAzureTableStorageLoggingKey = "IsAzureTableStorageLogging";

        private ICodePackageActivationContext _serviceContext;

        public SettingService()
        {
            // This is really neat! The FabricRuntime.GetActivationContext() gives me access to the config context!!! :-)
            _serviceContext = FabricRuntime.GetActivationContext();
        }

        public string GetAzureStorageConnectionString()
        {
            return GetSectionParameterValue(ConfigurationSectionName, LogsStorageConnectionStringKey);
        }

        public string GetAzureStorageLogsTable()
        {
            return GetSectionParameterValue(ConfigurationSectionName, LogsTableStorageNameKey);
        }

        public string GetInstrumentationKey()
        {
            return GetSectionParameterValue(ConfigurationSectionName, InstrumentationKey);
        }

        public string GetOltpConnectionString()
        {
            return GetSectionParameterValue(ConfigurationSectionName, OltpConnectionStringKey);
        }

        public string GetOltpConnectionType()
        {
            return GetSectionParameterValue(ConfigurationSectionName, OltpConnectorKey);
        }

        public bool IsEtwLogging()
        {
            var setting = GetSectionParameterValue(ConfigurationSectionName, IsEtwLoggingKey);
            bool isLogging = true;
            bool.TryParse(setting, out isLogging);
            return isLogging;
        }

        public bool IsAzureStorageLogging()
        {
            var setting = GetSectionParameterValue(ConfigurationSectionName, IsAzureTableStorageLoggingKey);
            bool isLogging = true;
            bool.TryParse(setting, out isLogging);
            return isLogging;
        }

        // ** PRIVATE **//
        private string GetSectionParameterValue(string section, string parameterKey)
        {
            try
            {
                if (_serviceContext == null)
                    return "";

                var parameterValue = "";
                var configurationPackage = _serviceContext.GetConfigurationPackageObject("Config");
                if (configurationPackage != null)
                {
                    var configSection = configurationPackage.Settings.Sections[ConfigurationSectionName];
                    if (configSection != null)
                    {
                        var connectorParameter = configSection.Parameters[parameterKey];
                        if (connectorParameter != null)
                        {
                            parameterValue = connectorParameter.Value;
                        }
                    }
                }

                return parameterValue;
            }
            catch (Exception ex)
            {
                return "";
            }
        }
    }
}
