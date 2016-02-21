using System;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData
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
