// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using Microsoft.AspNet.OData;
using Microsoft.OData.Edm;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;

namespace Microsoft.Test.E2E.AspNet.OData.Aggregation
{
    public class AggregationEdmModel
    {
        public const string StdDevMethodToken = "Custom.StdDev";
        public const string StdDevMethodLabel = "StandardDeviation";

        public static IEdmModel GetEdmModel(WebRouteConfiguration configuration)
        {
            var builder = configuration.CreateConventionModelBuilder();
            builder.EntitySet<Customer>("Customers");
            builder.EntityType<Customer>().EnumProperty(c => c.Bucket);
            builder.EntitySet<Order>("Orders");
            IEdmModel model = builder.GetEdmModel();

            var stdDevMethods = GetCustomMethods(typeof(DbFunctions), StdDevMethodLabel);
            CustomAggregateMethodAnnotation customMethods = new CustomAggregateMethodAnnotation();
            customMethods.AddMethod(StdDevMethodToken, stdDevMethods);

            model.SetAnnotationValue(model, customMethods);

            return model;
        }

        private static Dictionary<Type, MethodInfo> GetCustomMethods(Type customMethodsContainer, string methodName)
        {
            return customMethodsContainer.GetMethods()
                .Where(m => m.Name == methodName)
                .Where(m => m.GetParameters().Count() == 1)
                .ToDictionary(m => m.GetParameters().First().ParameterType.GetGenericArguments().First());
        }
    }
}
