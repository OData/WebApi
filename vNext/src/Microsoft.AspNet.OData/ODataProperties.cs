using System;
using Microsoft.AspNet.OData.Routing;
using Microsoft.OData.Edm;

namespace Microsoft.AspNet.OData
{

    public class ODataProperties
    {
        public IEdmModel Model { get; set; }

        public ODataPath Path { get; set; }

        public Microsoft.OData.Core.UriParser.Semantic.ODataPath NewPath { get; set; }

        public long? TotalCount { get; set; }

        public Uri NextLink { get; set; }

        public bool IsValidODataRequest { get; set; }
    }
}
