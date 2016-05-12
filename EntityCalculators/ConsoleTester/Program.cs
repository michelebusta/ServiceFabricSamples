using Shared.Handlers;
using Shared.Services;
using System;

namespace ConsoleTester
{
    class Program
    {
        private const string LOG_TAG = "ConsoleTester";

        static void Main(string[] args)
        {
            var error = "";
            var handler = HandlersFactory.GetProfilerHandler(new ConsoleAppSettingService());
            handler.Start(LOG_TAG, "Main");

            try
            {
                handler.Info("message1");
            }
            catch (Exception ex)
            {
                error = ex.Message;
            }
            finally
            {
                handler.Stop(error);
            }

            Console.WriteLine("Done...enter to exit!");
            Console.ReadLine();
        }
    }

    class ConsoleAppSettingService : ISettingService
    {
        public string GetAzureStorageConnectionString()
        {
            //TODO:
            return "";
        }

        public string GetAzureStorageLogsTable()
        {
            //TODO:
            return "";
        }

        public string GetInstrumentationKey()
        {
            //TODO:
            return "";
        }

        public string GetOltpConnectionString()
        {
            //TODO:
            return "";
        }

        public string GetOltpConnectionType()
        {
            //TODO:
            return "";
        }

        public bool IsAzureStorageLogging()
        {
            //TODO:
            return true;
        }

        public bool IsEtwLogging()
        {
            //TODO:
            return true;
        }
    }
}
