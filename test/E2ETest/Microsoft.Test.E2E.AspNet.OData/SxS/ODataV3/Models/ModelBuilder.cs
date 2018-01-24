// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

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