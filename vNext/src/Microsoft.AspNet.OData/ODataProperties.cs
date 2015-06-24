using System;
using Microsoft.OData.Core;
using Microsoft.OData.Core.UriParser.Semantic;
using Microsoft.OData.Edm;
using ODataPath = Microsoft.AspNet.OData.Routing.ODataPath;

namespace Microsoft.AspNet.OData
{

    public class ODataProperties
    {
        internal const string ODataServiceVersionHeader = "OData-Version";

        internal const ODataVersion DefaultODataVersion = ODataVersion.V4;

        public IEdmModel Model { get; set; }

        // TODO: Consider remove this.
        public ODataPath Path { get; set; }

        public Microsoft.OData.Core.UriParser.Semantic.ODataPath NewPath { get; set; }

        public long? TotalCount { get; set; }

        public Uri NextLink { get; set; }

        public bool IsValidODataRequest { get; set; }

        public SelectExpandClause SelectExpandClause { get; set; }
    }
}
