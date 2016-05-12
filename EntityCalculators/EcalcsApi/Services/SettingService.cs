using System;
using System.Configuration;

namespace EcalcsApi.Services
{
    public class SettingService : ISettingService
    {
        private const string LOG_TAG = "SettingService";

        private const string EcalcsBaseUrlKey = "EcalcsBaseUrl";

        public string GetEcalsBaseUrl()
        {
            return ConfigurationManager.AppSettings[EcalcsBaseUrlKey] != null ? ConfigurationManager.AppSettings[EcalcsBaseUrlKey].ToString() : "http://13.82.52.188:8080";
        }
    }
}