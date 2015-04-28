using System.Text;
using System.Web.OData.Routing;

namespace WebStack.QA.Test.OData.Common
{
    public class AzureODataPathHandler : DefaultODataPathHandler
    {
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
