using Microsoft.AspNet.Mvc;
using Microsoft.Framework.Internal;
using Microsoft.Framework.OptionsModel;
using Microsoft.OData.Edm;

namespace Microsoft.AspNet.OData
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("odata/$metadata")]
    public class MetadataController : Controller
    {
        private readonly IEdmModel _model;

        public MetadataController([NotNull]IOptions<ODataOptions> options)
        {
            _model = options.Options?.ModelProvider();
        }

        public IEdmModel Get()
        {
            return _model;
        }
    }
}
