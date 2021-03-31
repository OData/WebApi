// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNet.OData.Builder;
using Microsoft.OData.Edm;

namespace AspNetCore3xEndpointSample.Web.Models
{
    public static class EdmModelBuilder
    {
        private static IEdmModel _edmModel;

        public static IEdmModel GetEdmModel()
        {
            if (_edmModel == null)
            {
                var builder = new ODataConventionModelBuilder();
                builder.EntitySet<Customer>("Customers");
                builder.EntitySet<Order>("Orders");

                // two overload function import
                var function = builder.Function("CalcByRating");
                function.Parameter<int>("order");
                function.ReturnsFromEntitySet<Customer>("Customers");

                function = builder.Function("CalcByRating");
                function.Parameter<string>("name");
                function.ReturnsFromEntitySet<Customer>("Customers");

                // action import
                var action = builder.Action("CalcByRatingAction");
                action.Parameter<int>("order");
                action.ReturnsFromEntitySet<Customer>("Customers");

                _edmModel = builder.GetEdmModel();
            }

            return _edmModel;
        }

    }
}