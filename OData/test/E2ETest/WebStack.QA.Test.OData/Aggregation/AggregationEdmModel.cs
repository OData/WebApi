using System.Web.Http;
using System.Web.OData.Builder;
using Microsoft.OData.Edm;
using System.Collections.Generic;
using System;
using System.Reflection;
using System.Linq;
using System.Web.OData;
using System.Data.Entity;

namespace WebStack.QA.Test.OData.Aggregation
{
    public class AggregationEdmModel
    {
        public const string stdDevMethodToken = "Custom.StdDev";
        public const string stdDevMethodLabel = "StandardDeviation";

        public static IEdmModel GetEdmModel(HttpConfiguration configuration)
        {
            var builder = new ODataConventionModelBuilder(configuration);
            builder.EntitySet<Customer>("Customers");
            builder.EntitySet<Order>("Orders");
            IEdmModel model = builder.GetEdmModel();

            var stdDevMethods = getCustomMethods(typeof(DbFunctions), stdDevMethodLabel);
            CustomAggregateMethodAnnotation customMethods = new CustomAggregateMethodAnnotation();
            customMethods.AddMethod(stdDevMethodToken, stdDevMethods);

            model.SetAnnotationValue(model, customMethods);

            return model;
        }

        private static Dictionary<Type, MethodInfo> getCustomMethods(Type customMethodsContainer, string methodName)
        {
            return customMethodsContainer.GetMethods()
                .Where(m => m.Name == methodName)
                .Where(m => m.GetParameters().Count() == 1)
                .ToDictionary(m => m.GetParameters().First().ParameterType.GetGenericArguments().First());
        }
    }
}
