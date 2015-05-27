using System;
using Microsoft.AspNet.OData.Routing;
using Microsoft.OData.Core;
using Microsoft.OData.Edm;

namespace Microsoft.AspNet.OData
{

    public class ODataProperties
    {
        internal const string ODataServiceVersionHeader = "OData-Version";

        internal const ODataVersion DefaultODataVersion = ODataVersion.V4;

        public IEdmModel Model { get; set; }

        public ODataPath Path { get; set; }

        public Microsoft.OData.Core.UriParser.Semantic.ODataPath NewPath { get; set; }

        public long? TotalCount { get; set; }

        public Uri NextLink { get; set; }

        public bool IsValidODataRequest { get; set; }
    }
}
