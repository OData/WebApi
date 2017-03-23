using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.OData
{
    public class ODataOptionsSetup : ConfigureOptions<ODataOptions>
    {
        public ODataOptionsSetup() : base(ConfigureOData)
        {
        }

        public static void ConfigureOData(ODataOptions options)
        {
        }
    }
}
