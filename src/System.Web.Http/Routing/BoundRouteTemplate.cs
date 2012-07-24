// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Web.Http.Routing
{
    /// <summary>
    /// Represents a URI generated from a <see cref="HttpParsedRoute"/>. 
    /// </summary>
    internal class BoundRouteTemplate
    {
        public string BoundTemplate { get; set; }

        public HttpRouteValueDictionary Values { get; set; }
    }
}
