using Microsoft.AspNet.Builder;
using Microsoft.AspNet.OData.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.AspNet.OData
{
    public static class BuilderExtensions
    {
        public static IApplicationBuilder UseOData(this IApplicationBuilder app)
        {
            return app.UseRouter(new ODataRoute());
        }
    }
}
