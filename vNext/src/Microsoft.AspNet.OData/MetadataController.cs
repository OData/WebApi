using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Framework.Internal;
using Microsoft.OData.Edm;

namespace Microsoft.AspNet.OData
{
    [ApiExplorerSettings(IgnoreApi = true)]
    // [Route("odata/$metadata")]
    public class MetadataController : Controller
    {
        private readonly IEdmModel _model;

        public MetadataController([NotNull]ODataProperties odataProperties)
        {
           this._model = odataProperties.Model;
        }

        // not work: public IEdmModel Get => this._model;
        public IEdmModel Get()
        {
            return this._model;
        }

}
}
