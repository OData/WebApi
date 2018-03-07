// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.OData.Edm;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;

namespace Microsoft.Test.E2E.AspNet.OData.UriParserExtension
{
    public class UriParserExtenstionEdmModel
    {
        public static IEdmModel GetEdmModel(WebRouteConfiguration configuration)
        {
            var builder = configuration.CreateConventionModelBuilder();
            builder.EntitySet<Customer>("Customers");
            builder.EntitySet<Order>("Orders");

            builder.EntityType<Customer>().Function("CalculateSalary").Returns<int>().Parameter<int>("month");
            builder.EntityType<Customer>().Action("UpdateAddress");
            builder.EntityType<Customer>()
                .Collection.Function("GetCustomerByGender")
                .ReturnsCollectionFromEntitySet<Customer>("Customers")
                .Parameter<Gender>("gender");

            return builder.GetEdmModel();
        }
    }
}
