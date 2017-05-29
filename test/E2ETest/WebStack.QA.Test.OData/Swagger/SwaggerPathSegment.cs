using System.Collections.Generic;
using System.Web.OData.Routing;
using Microsoft.OData.Edm;

namespace WebStack.QA.Test.OData.Swagger
{
    public class SwaggerPathSegment : ODataPathSegment
    {
        /// <inheritdoc/>
        public override string SegmentKind
        {
            get
            {
                return "$swagger";
            }
        }

        /// <inheritdoc/>
        public override IEdmType GetEdmType(IEdmType previousEdmType)
        {
            return null;
        }

        /// <inheritdoc/>
        public override IEdmNavigationSource GetNavigationSource(IEdmNavigationSource previousNavigationSource)
        {
            return null;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return "$swagger";
        }

        /// <inheritdoc/>
        public override bool TryMatch(ODataPathSegment pathSegment, IDictionary<string, object> values)
        {
            return pathSegment.SegmentKind == "$swagger" || pathSegment.SegmentKind == "swagger.json";
        }
    }
}
