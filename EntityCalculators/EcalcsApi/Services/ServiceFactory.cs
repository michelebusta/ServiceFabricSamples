namespace EcalcsApi.Services
{
    public class ServiceFactory
    {
        private static ISettingService _settingService;
        public static ISettingService GetSettingService()
        {
            if (_settingService == null)
                _settingService = new SettingService();

            return _settingService;
        }
    }
}