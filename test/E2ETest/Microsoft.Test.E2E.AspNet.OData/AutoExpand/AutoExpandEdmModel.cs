﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.OData.Edm;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;

namespace Microsoft.Test.E2E.AspNet.OData.AutoExpand
{
    public class AutoExpandEdmModel
    {
        public static IEdmModel GetEdmModel(WebRouteConfiguration configuration)
        {
            var builder = configuration.CreateConventionModelBuilder();
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
