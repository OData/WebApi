using Microsoft.OData.Edm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.AspNet.OData
{
    public class ODataOptions
    {
        public delegate IEdmModel IODataModelProvider();

        public IODataModelProvider ModelProvider { get; set; }
    }
}
