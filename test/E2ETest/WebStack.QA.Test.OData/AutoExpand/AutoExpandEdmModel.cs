// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Web.Http;
using Microsoft.AspNet.OData.Builder;
using Microsoft.OData.Edm;

namespace WebStack.QA.Test.OData.AutoExpand
{
    public class AutoExpandEdmModel
    {
        public static IEdmModel GetEdmModel(HttpConfiguration configuration)
        {
            var builder = new ODataConventionModelBuilder(configuration);
            builder.EntitySet<Customer>("Customers");
            builder.EntitySet<Order>("Orders");
            builder.EntityType<SpecialOrder>();
            builder.EntityType<VipOrder>();
            builder.EntitySet<ChoiceOrder>("OrderChoices");
            builder.EntitySet<NormalOrder>("NormalOrders");
            builder.EntityType<DerivedOrder>();
            builder.EntityType<DerivedOrder2>();
            builder.EntitySet<OrderDetail>("OrderDetails");
            builder.EntitySet<People>("People");
            IEdmModel model = builder.GetEdmModel();
            return model;
        }
    }
}
