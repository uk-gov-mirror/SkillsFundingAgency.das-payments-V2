﻿namespace SFA.DAS.Payments.Core.Configuration
{
    public static class ConfigurationExtensions
    {
        public static string GetConnectionString(this IConfigurationHelper helper, string connectionStringName)
        {
            return helper.GetSetting("ConnectionStringsSection", connectionStringName);
        }

        public static string GetSetting(this IConfigurationHelper helper, string settingName)
        {
            return helper.GetSetting("Settings", settingName);
        }

        public static string GetSettingOrDefault(this IConfigurationHelper helper, string settingName,
            string defaultValue)
        {
            return helper.HasSetting("Settings", settingName)
                ? helper.GetSetting("Settings", settingName)
                : defaultValue;
        }

        public static int GetSettingOrDefault(this IConfigurationHelper helper, string settingName, int defaultValue)
        {
            return helper.HasSetting("Settings", settingName)
                ? int.Parse(helper.GetSetting("Settings", settingName))
                : defaultValue;
        }

        public static int? GetSettingAsNullableInt(this IConfigurationHelper helper, string settingName)
        {
            if (!helper.HasSetting("Settings", settingName)) return null;
            return (int.TryParse(helper.GetSetting("Settings", settingName), out int result)) ? result :(int?) null;
        }
    }
}