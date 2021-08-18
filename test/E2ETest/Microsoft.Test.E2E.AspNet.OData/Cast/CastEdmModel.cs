//-----------------------------------------------------------------------------
// <copyright file="CastEdmModel.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNet.OData.Builder;
using Microsoft.OData.Edm;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;

namespace Microsoft.Test.E2E.AspNet.OData.Cast
{
    internal class CastEdmModel
    {
        public static IEdmModel GetEdmModel(WebRouteConfiguration configuration)
        {
            ODataConventionModelBuilder builder = configuration.CreateConventionModelBuilder();
            EntitySetConfiguration<Product> employees = builder.EntitySet<Product>("Products");
            var airPlaneType=builder.EntityType<AirPlane>();
            airPlaneType.DerivesFrom<Product>();

            builder.Namespace = typeof(Product).Namespace;

            var edmModel = builder.GetEdmModel();
            return edmModel;
        }
    }
}
