//-----------------------------------------------------------------------------
// <copyright file="SwaggerPathSegment.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNet.OData.Routing;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.Test.E2E.AspNet.OData.Swagger
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

        public override T TranslateWith<T>(PathSegmentTranslator<T> translator)
        {
            return default(T);
        }

        public override void HandleWith(PathSegmentHandler handler)
        {
            ODataPathSegmentHandler pathSegmentHandler = handler as ODataPathSegmentHandler;
            if (pathSegmentHandler != null)
            {
                pathSegmentHandler.Handle(this);
            }
        }

        public override IEdmType EdmType
        {
            get { return null; }
        }
    }
}
