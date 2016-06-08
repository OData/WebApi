// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Web.OData.Routing;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

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
