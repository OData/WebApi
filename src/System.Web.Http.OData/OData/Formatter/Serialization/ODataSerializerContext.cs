// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Net.Http;
using System.Web.Http.OData.Routing;
using System.Web.Http.Routing;
using Microsoft.Data.Edm;

namespace System.Web.Http.OData.Formatter.Serialization
{
    /// <summary>
    /// Context information used by the <see cref="ODataSerializer"/> when serializing objects in OData message format.
    /// </summary>
    public class ODataSerializerContext
    {
        private ODataMetadataLevel _metadataLevel;

        /// <summary>
        /// Gets or sets the URL helper.
        /// </summary>
        public UrlHelper UrlHelper { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="IODataPathHandler"/> to use for generating OData paths.
        /// </summary>
        public IODataPathHandler PathHandler { get; set; }

        /// <summary>
        /// Gets or sets the entity set.
        /// </summary>
        public IEdmEntitySet EntitySet { get; set; }

        /// <summary>
        /// Gets or sets the EDM model associated with the request.
        /// </summary>
        public IEdmModel Model { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ODataPath"/> of the request.
        /// </summary>
        public ODataPath Path { get; set; }

        /// <summary>
        /// Gets or sets the root element name which is used when writing primitive types
        /// and complex types.
        /// </summary>
        public string RootElementName { get; set; }

        /// <summary>
        /// Get or sets whether expensive links should be calculated.
        /// </summary>
        public bool SkipExpensiveAvailabilityChecks { get; set; }

        /// <summary>
        /// Gets or sets the metadata level of the response.
        /// </summary>
        public ODataMetadataLevel MetadataLevel
        {
            get
            {
                return _metadataLevel;
            }
            set
            {
                ODataMetadataLevelHelper.Validate(value, "value");
                _metadataLevel = value;
            }
        }

        /// <summary>
        /// The next page link, if any, to use when serializing a feed.
        /// </summary>
        public Uri NextPageLink { get; set; }

        /// <summary>
        /// The inline count, if any, to use when serializing a feed.
        /// </summary>
        public long? InlineCount { get; set; }
    }
}
