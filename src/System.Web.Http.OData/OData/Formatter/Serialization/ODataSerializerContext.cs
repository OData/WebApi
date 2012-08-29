// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Web.Http.Routing;
using Microsoft.Data.Edm;

namespace System.Web.Http.OData.Formatter.Serialization
{
    /// <summary>
    /// Context information used by the <see cref="ODataSerializer"/> when serializing objects in OData message format.
    /// </summary>
    public class ODataSerializerContext
    {
        /// <summary>
        /// Gets or sets the URL helper.
        /// </summary>
        public UrlHelper UrlHelper { get; set; }

        /// <summary>
        /// Gets or sets the root projection node.
        /// </summary>
        public ODataQueryProjectionNode RootProjectionNode { get; set; }

        /// <summary>
        /// Gets or sets the current projection node.
        /// </summary>
        public ODataQueryProjectionNode CurrentProjectionNode { get; set; }

        /// <summary>
        /// Gets or sets the entity set.
        /// </summary>
        public IEdmEntitySet EntitySet { get; set; }

        /// <summary>
        /// Gets or sets the ServiceOperationName which is used when writing primitive types
        /// and complex types.
        /// </summary>
        public string ServiceOperationName { get; set; }
    }
}
