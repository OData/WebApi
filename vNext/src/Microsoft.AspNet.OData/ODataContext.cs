using Microsoft.OData.Edm;
using System;

namespace Microsoft.AspNet.OData
{
    internal class ODataContext
    {
        public IEdmModel Model { get; }

        public ODataContext(Type t)
        {
            Model = DefaultODataModelProvider.BuildEdmModel(t);
        }
    }
}
