// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNet.OData.Builder;
using Microsoft.OData.Edm;

namespace Microsoft.Test.E2E.AspNet.OData.Spatial
{
    public static class IsofEdmModel
    {
        private static IEdmModel _model;

        public static IEdmModel GetEdmModel()
        {
            if (_model != null)
            {
                return _model;
            }

            var builder = new ODataConventionModelBuilder();
            builder.EntitySet<SpatialCustomer>("SpatialCustomers");
            return _model = builder.GetEdmModel();
        }
    }
}
