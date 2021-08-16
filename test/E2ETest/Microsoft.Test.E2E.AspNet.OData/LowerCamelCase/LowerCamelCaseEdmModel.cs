//-----------------------------------------------------------------------------
// <copyright file="LowerCamelCaseEdmModel.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNet.OData.Builder;
using Microsoft.OData.Edm;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;

namespace Microsoft.Test.E2E.AspNet.OData.LowerCamelCase
{
    public class LowerCamelCaseEdmModel
    {
        public static IEdmModel GetConventionModel(WebRouteConfiguration configuration)
        {
            ODataConventionModelBuilder builder = configuration.CreateConventionModelBuilder();
            EntitySetConfiguration<Employee> employees = builder.EntitySet<Employee>("Employees");
            EntityTypeConfiguration<Employee> employee = employees.EntityType;
            employee.EnumProperty<Gender>(e => e.Sex).Name = "Gender";

            employee.Collection.Function("GetEarliestTwoEmployees").ReturnsCollectionFromEntitySet<Employee>("Employees");
            employee.Collection.Function("GetPaged").ReturnsCollectionFromEntitySet<Employee>("Employees");

            var functionConfiguration = builder.Function("GetAddress");
            functionConfiguration.Parameter<int>("id");
            functionConfiguration.Returns<Address>();

            var actionConfiguration = builder.Action("SetAddress");
            actionConfiguration.Parameter<int>("id");
            actionConfiguration.Parameter<Address>("address");
            actionConfiguration.ReturnsFromEntitySet(employees);

            var resetDataSource = builder.Action("ResetDataSource");

            builder.Namespace = typeof(Employee).Namespace;
            builder.EnableLowerCamelCase();
            var edmModel = builder.GetEdmModel();
            return edmModel;
        }
    }
}
