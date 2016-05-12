namespace Shared.Services
{
    public interface ISettingService
    {
        string GetAzureStorageConnectionString();
        string GetAzureStorageLogsTable();
        string GetInstrumentationKey();
        string GetOltpConnectionType();
        string GetOltpConnectionString();
        bool IsEtwLogging();
        bool IsAzureStorageLogging();
    }
}
