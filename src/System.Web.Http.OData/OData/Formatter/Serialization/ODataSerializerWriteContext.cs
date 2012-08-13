// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Web.Http.Routing;
using Microsoft.Data.Edm;

namespace System.Web.Http.OData.Formatter.Serialization
{
    /// <summary>
    /// Context information used by the serilizers while serializing objects in OData message format.
    /// </summary>
    public class ODataSerializerWriteContext
    {
        private ODataResponseContext _responseContext;

        /// <summary>
        /// This constructor is used for unit testing only.
        /// </summary>
        public ODataSerializerWriteContext()
        {
        }

        /// <summary>
        ///  Initializes a new instance of <see cref="ODataSerializerWriteContext"/>.
        /// </summary>
        /// <param name="responseContext">An instance of <see cref="ODataResponseContext"/> that has context information for serializing various types. </param>
        public ODataSerializerWriteContext(ODataResponseContext responseContext)
        {
            if (responseContext == null)
            {
                throw Error.ArgumentNull("responseContext");
            }

            _responseContext = responseContext;
        }

        /// <summary>
        /// Gets the ResponseContext that has context information for serializing various types.
        /// </summary>
        public ODataResponseContext ResponseContext
        {
            get { return _responseContext; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is request.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is request; otherwise, <c>false</c>.
        /// </value>
        public bool IsRequest
        {
            get { return _responseContext.ODataRequestMessage != null; }
        }

        public UrlHelper UrlHelper { get; set; }

        public ODataQueryProjectionNode RootProjectionNode { get; set; }

        public ODataQueryProjectionNode CurrentProjectionNode { get; set; }

        /// <summary>
        /// Gets or sets the entity set.
        /// </summary>
        /// <value>
        /// The entity set.
        /// </value>
        public IEdmEntitySet EntitySet { get; set; }
    }
}
