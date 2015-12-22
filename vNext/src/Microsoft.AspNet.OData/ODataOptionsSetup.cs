using Microsoft.Extensions.OptionsModel;


namespace Microsoft.AspNet.OData
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
