using System;

namespace Nuwa
{
    [Flags]
    public enum HostType
    {
        WcfSelf = 1,
        IIS = 2,
        KatanaSelf = 4,
        IISExpress = 8,
        AzureWebsite = 16,
        IISKatana = 32,
        IISExpressKatana = 64,
        AzureWebsiteKatana = 128
    }
}