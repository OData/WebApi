//-----------------------------------------------------------------------------
// <copyright file="BoundOperationEdmModel.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNet.OData.Builder;
using Microsoft.OData.Edm;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;

namespace Microsoft.Test.E2E.AspNet.OData.BoundOperation
{
    internal class UnBoundFunctionEdmModel
    {
        public static IEdmModel GetEdmModel(WebRouteConfiguration configuration)
        {
            ODataConventionModelBuilder builder = configuration.CreateConventionModelBuilder();
            EntitySetConfiguration<Employee> entitySetConfiguration = builder.EntitySet<Employee>("Employees");
            EntityTypeConfiguration<Manager> entityTypeConfigurationOfManager = builder.EntityType<Manager>();
            EntityTypeConfiguration<Employee> entityTypeConfigurationOfEmployee = builder.EntityType<Employee>();

            #region functions

            // Function bound to a collection of base EntityType.
            entityTypeConfigurationOfEmployee.Collection.Function("GetCount")
                .Returns<int>();

            // Overload
            entityTypeConfigurationOfEmployee.Collection.Function("GetCount")
                .Returns<int>()
                .Parameter<string>("Name");

            // Overload with one optional parameter
            var salaryRangeCount = entityTypeConfigurationOfEmployee.Collection.Function("GetWholeSalary")
                .Returns<int>();
            salaryRangeCount.Parameter<double>("minSalary");
            salaryRangeCount.Parameter<double>("maxSalary").Optional();
            salaryRangeCount.Parameter<double>("aveSalary").HasDefaultValue("8.9");

            // Function bound to a collection of derived EntityType.
            entityTypeConfigurationOfManager.Collection.Function("GetCount")
                .Returns<int>();

            // Function bound to an base EntityType
            entityTypeConfigurationOfEmployee.Function("GetEmailsCount")
                .Returns<int>();

            entityTypeConfigurationOfEmployee.Function("GetOptionalAddresses")
                .ReturnsCollection<Address>()
                .IsComposable = true;

            entityTypeConfigurationOfEmployee.Function("GetEmails")
                .ReturnsCollection<string>()
                .IsComposable = false;

            // Function bound to a derived EntityType
            entityTypeConfigurationOfManager.Function("GetEmailsCount")
                .Returns<int>();

            // Function with primitive and collection of primitive parameters
            var function = entityTypeConfigurationOfEmployee.Collection.Function("PrimitiveFunction").Returns<string>();
            function.Parameter<int>("param");
            function.Parameter<double?>("price"); // nullable
            function.Parameter<string>("name"); // nullable
            function.CollectionParameter<string>("names"); // collection with nullable element

            // Function with Enum and collection of Enum parameters
            function = entityTypeConfigurationOfEmployee.Collection.Function("EnumFunction").Returns<string>();
            function.Parameter<Color>("bkColor");
            function.Parameter<Color?>("frColor"); // nullable
            function.CollectionParameter<Color>("colors"); // collection with non-nullable element

            // Function with complex and collection of complex parameters
            function = entityTypeConfigurationOfEmployee.Collection.Function("ComplexFunction").ReturnsCollection<Address>();
            function.Parameter<Address>("address").Nullable = false;
            function.Parameter<Address>("location"); // nullable
            function.CollectionParameter<Address>("addresses"); // collection with nullable element

            // Function with entity and collection of entity parameters
            function = entityTypeConfigurationOfEmployee.Collection.Function("EntityFunction").Returns<string>();
            function.EntityParameter<Employee>("person").Nullable = false;
            function.EntityParameter<Employee>("guard"); // nullable
            function.CollectionEntityParameter<Employee>("staff"); // collection with nullable element

            #endregion

            #region actions

            // Action bound to a collection of base EntityType
            entityTypeConfigurationOfEmployee.Collection.Action("IncreaseSalary")
                .ReturnsCollectionFromEntitySet(entitySetConfiguration)
                .Parameter<string>("Name");

            // Action bound to a collection of derived EntityType
            entityTypeConfigurationOfManager.Collection.Action("IncreaseSalary")
                .ReturnsCollectionFromEntitySet(entitySetConfiguration)
                .Parameter<string>("Name");

            // Action bound to a base EntityType
            entityTypeConfigurationOfEmployee.Action("IncreaseSalary")
                .Returns<int>();

            // Action bound to a derived EntityType
            entityTypeConfigurationOfManager.Action("IncreaseSalary")
                .Returns<int>();

            // Action with optional parameters
            var action = entityTypeConfigurationOfManager.Action("IncreaseWholeSalary")
                .Returns<int>();
            action.Parameter<double>("minSalary");
            action.Parameter<double>("maxSalary").Optional();
            action.Parameter<double>("aveSalary").HasDefaultValue("8.9");

            // Action with primitive and collection of primitive parameters
            action = entityTypeConfigurationOfEmployee.Collection.Action("PrimitiveAction");
            action.Parameter<int>("param");
            action.Parameter<double?>("price"); // nullable
            action.Parameter<string>("name"); // nullable
            action.CollectionParameter<string>("names"); // collection with nullable element

            // Action with Enum and collection of Enum parameters
            action = entityTypeConfigurationOfEmployee.Collection.Action("EnumAction");
            action.Parameter<Color>("bkColor");
            action.Parameter<Color?>("frColor"); // nullable
            action.CollectionParameter<Color>("colors"); // collection with non-nullable element

            // Action with complex and collection of complex parameters
            action = entityTypeConfigurationOfEmployee.Collection.Action("ComplexAction");
            action.Parameter<Address>("address").Nullable = false;
            action.Parameter<Address>("location"); // nullable
            action.CollectionParameter<Address>("addresses"); // collection with nullable element

            // Action with entity and collection of entity parameters
            action = entityTypeConfigurationOfEmployee.Collection.Action("EntityAction");
            action.EntityParameter<Employee>("person").Nullable = false;
            action.EntityParameter<Employee>("guard"); // nullable
            action.CollectionEntityParameter<Employee>("staff"); // collection with nullable element
            #endregion

            builder.Action("ResetDataSource");

            builder.EnumType<Color>().Namespace = "NS";
            builder.ComplexType<Address>().Namespace = "NS";
            builder.ComplexType<SubAddress>().Namespace = "NS";
            builder.EntityType<Employee>().Namespace = "NS";
            builder.EntityType<Manager>().Namespace = "NS";

            return builder.GetEdmModel();
        }
    }
}
