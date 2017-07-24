using System.Configuration;

namespace Nuwa.WebStack
{
    public static class NuwaGlobalConfiguration
    {
        public const string AzureWebsiteUrlKey = "Nuwa.AzureWebsiteUrl";
        public const string AzureFtpUrlKey = "Nuwa.AzureFtpUrl";
        public const string AzureFtpUserNameKey = "Nuwa.AzureFtpUserName";
        public const string AzureFtpPasswordKey = "Nuwa.AzureFtpPassword";
        public const string DefaultHostTypesKey = "Nuwa.DefaultHostTypes";
        public const string SqlAzureConnectionStringKey = "Nuwa.SqlAzureConnectionString";
        public const string BrowsersKey = "Nuwa.Browsers";
        public const string TraceWriterTypeKey = "Nuwa.TraceWriterType";
        public const string HttpConfigureKey = "Nuwa.HttpConfigure";
        public const string KatanaSelfStartingPortKey = "Nuwa.KatanaSelfStartingPort";

        public static string KatanaSelfStartingPort
        {
            get
            {
                return ConfigurationManager.AppSettings[KatanaSelfStartingPortKey];
            }
        }

        public static string AzureWebsiteUrl
        {
            get
            {
                return ConfigurationManager.AppSettings[AzureWebsiteUrlKey];
            }
        }

        public static string AzureFtpUrl
        {
            get
            {
                return ConfigurationManager.AppSettings[AzureFtpUrlKey];
            }
        }

        public static string AzureFtpUserName
        {
            get
            {
                return ConfigurationManager.AppSettings[AzureFtpUserNameKey];
            }
        }

        public static string AzureFtpPassword
        {
            get
            {
                return ConfigurationManager.AppSettings[AzureFtpPasswordKey];
            }
        }

        public static string DefaultHostTypes
        {
            get
            {
                return ConfigurationManager.AppSettings[DefaultHostTypesKey];
            }
        }

        public static string SqlAzureConnectionString
        {
            get
            {
                return ConfigurationManager.AppSettings[SqlAzureConnectionStringKey];
            }
        }

        public static string Browsers
        {
            get
            {
                return ConfigurationManager.AppSettings[BrowsersKey];
            }
        }

        public static string TraceWriterType
        {
            get
            {
                return ConfigurationManager.AppSettings[TraceWriterTypeKey];
            }
        }

        public static string HttpConfigure
        {
            get
            {
                return ConfigurationManager.AppSettings[HttpConfigureKey];
            }
        }
    }
}
