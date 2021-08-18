//-----------------------------------------------------------------------------
// <copyright file="ModelBuilder.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Web.Http.OData.Builder;
using Microsoft.Data.Edm;

namespace Microsoft.Test.E2E.AspNet.OData.SxS.ODataV3.Models
{
    public static class ModelBuilder
    {
        public static IEdmModel GetEdmModel()
        {
            var builder = new ODataConventionModelBuilder();
            builder.EntitySet<Product>("Products");
            builder.EntitySet<Part>("Parts");
            return builder.GetEdmModel();
        }
    }
}
