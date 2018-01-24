// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using System.Web.Http;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Builder;
using Microsoft.OData.Edm;

namespace Microsoft.Test.E2E.AspNet.OData.Aggregation
{
    public class AggregationEdmModel
    {
        public const string StdDevMethodToken = "Custom.StdDev";
        public const string StdDevMethodLabel = "StandardDeviation";

        public static IEdmModel GetEdmModel(HttpConfiguration configuration)
        {
            var builder = new ODataConventionModelBuilder(configuration);
            builder.EntitySet<Customer>("Customers");
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
