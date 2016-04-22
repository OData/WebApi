using System.Collections.Generic;
using System.Web.OData.Routing;
using Microsoft.OData.Core.UriParser.Semantic;
using Microsoft.OData.Core.UriParser.Visitors;
using Microsoft.OData.Edm;

namespace WebStack.QA.Test.OData.Swagger
{
    public class SwaggerPathSegment : ODataPathSegment
    {
        /// <inheritdoc/>
        public virtual string SegmentKind
        {
            get
            {
                return "$swagger";
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return "$swagger";
        }
        /*
        /// <inheritdoc/>
        public virtual bool TryMatch(ODataPathSegment pathSegment, IDictionary<string, object> values)
        {
            return pathSegment.SegmentKind == "$swagger" || pathSegment.SegmentKind == "swagger.json";
        }*/

        public override T TranslateWith<T>(PathSegmentTranslator<T> translator)
        {
            return default(T);
        }

        public override void HandleWith(PathSegmentHandler handler)
        {
            ODataPathSegmentHandler pathSegmentHandler = handler as ODataPathSegmentHandler;
            if (pathSegmentHandler != null)
            {
                pathSegmentHandler.HandleODataPathSegment(this);
            }
        }

        public override IEdmType EdmType
        {
            get { return null; }
        }
    }
}
