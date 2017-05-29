using System.Text;
using System.Web.Http.OData.Routing;
using Microsoft.Data.Edm;
using Microsoft.Data.OData;

namespace WebStack.QA.Test.OData.Common
{
    public class AzureODataPathHandler : DefaultODataPathHandler
    {
        protected override ODataPathSegment ParseAtEntityCollection(IEdmModel model, ODataPathSegment previous, IEdmType previousEdmType, string segment)
        {
            try
            {
                return base.ParseAtEntityCollection(model, previous, previousEdmType, segment);
            }
            catch (ODataException)
            {
                return new KeyValuePathSegment(segment);
            }
        }

        public override string Link(ODataPath path)
        {
            bool firstSegment = true;
            StringBuilder sb = new StringBuilder();
            foreach (ODataPathSegment segment in path.Segments)
            {
                if (firstSegment)
                {
                    firstSegment = false;
                }
                else
                {
                    sb.Append('/');
                }
                sb.Append(segment);
            }
            return sb.ToString();
        }
    }
}
