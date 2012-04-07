// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Microsoft.Web.Http.Data.EntityFramework;
using Microsoft.Web.Http.Data.EntityFramework.Metadata;
using Microsoft.Web.Http.Data.Test.Models;
using Xunit;
using Assert = Microsoft.TestCommon.AssertEx;

namespace Microsoft.Web.Http.Data.Test
{
    public class DataControllerDescriptionTest
    {
        // verify that the LinqToEntitiesMetadataProvider is registered by default for
        // LinqToEntitiesDataController<T> derived types
        [Fact]
        public void EFMetadataProvider_AttributeInference()
        {
            HttpConfiguration configuration = new HttpConfiguration();
            HttpControllerDescriptor controllerDescriptor = new HttpControllerDescriptor
            {
                Configuration = configuration,
                ControllerType = typeof(NorthwindEFTestController),
            };
            DataControllerDescription description = GetDataControllerDescription(typeof(NorthwindEFTestController));
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(typeof(Microsoft.Web.Http.Data.Test.Models.EF.Product));

            // verify key attribute
            Assert.NotNull(properties["ProductID"].Attributes[typeof(KeyAttribute)]);
            Assert.Null(properties["ProductName"].Attributes[typeof(KeyAttribute)]);

            // verify StringLengthAttribute
            StringLengthAttribute sla = (StringLengthAttribute)properties["ProductName"].Attributes[typeof(StringLengthAttribute)];
            Assert.NotNull(sla);
            Assert.Equal(40, sla.MaximumLength);

            // verify RequiredAttribute
            RequiredAttribute ra = (RequiredAttribute)properties["ProductName"].Attributes[typeof(RequiredAttribute)];
            Assert.NotNull(ra);
            Assert.False(ra.AllowEmptyStrings);

            // verify association attribute
            AssociationAttribute aa = (AssociationAttribute)properties["Category"].Attributes[typeof(AssociationAttribute)];
            Assert.NotNull(aa);
            Assert.Equal("Category_Product", aa.Name);
            Assert.True(aa.IsForeignKey);
            Assert.Equal("CategoryID", aa.ThisKey);
            Assert.Equal("CategoryID", aa.OtherKey);

            // verify metadata from "buddy class"
            PropertyDescriptor pd = properties["QuantityPerUnit"];
            sla = (StringLengthAttribute)pd.Attributes[typeof(StringLengthAttribute)];
            Assert.NotNull(sla);
            Assert.Equal(777, sla.MaximumLength);
            EditableAttribute ea = (EditableAttribute)pd.Attributes[typeof(EditableAttribute)];
            Assert.False(ea.AllowEdit);
            Assert.True(ea.AllowInitialValue);
        }

        [Fact]
        public void EFTypeDescriptor_ExcludedEntityMembers()
        {
            PropertyDescriptor pd = TypeDescriptor.GetProperties(typeof(Microsoft.Web.Http.Data.Test.Models.EF.Product))["EntityState"];
            Assert.True(LinqToEntitiesTypeDescriptor.ShouldExcludeEntityMember(pd));

            pd = TypeDescriptor.GetProperties(typeof(Microsoft.Web.Http.Data.Test.Models.EF.Product))["EntityState"];
            Assert.True(LinqToEntitiesTypeDescriptor.ShouldExcludeEntityMember(pd));

            pd = TypeDescriptor.GetProperties(typeof(Microsoft.Web.Http.Data.Test.Models.EF.Product))["SupplierReference"];
            Assert.True(LinqToEntitiesTypeDescriptor.ShouldExcludeEntityMember(pd));
        }

        [Fact]
        public void DescriptionValidation_NonAuthorizationFilter()
        {
            Assert.Throws<NotSupportedException>(
                () => GetDataControllerDescription(typeof(InvalidController_NonAuthMethodFilter)),
                String.Format(String.Format(Resource.InvalidAction_UnsupportedFilterType, "InvalidController_NonAuthMethodFilter", "UpdateProduct")));
        }

        /// <summary>
        /// Verify that associated entities are correctly registered in the description when
        /// using explicit data contracts
        /// </summary>
        [Fact]
        public void AssociatedEntityTypeDiscovery_ExplicitDataContract()
        {
            DataControllerDescription description = GetDataControllerDescription(typeof(IncludedAssociationTestController_ExplicitDataContract));
            List<Type> entityTypes = description.EntityTypes.ToList();
            Assert.Equal(8, entityTypes.Count);
            Assert.True(entityTypes.Contains(typeof(Microsoft.Web.Http.Data.Test.Models.EF.Order)));
            Assert.True(entityTypes.Contains(typeof(Microsoft.Web.Http.Data.Test.Models.EF.Order_Detail)));
            Assert.True(entityTypes.Contains(typeof(Microsoft.Web.Http.Data.Test.Models.EF.Customer)));
            Assert.True(entityTypes.Contains(typeof(Microsoft.Web.Http.Data.Test.Models.EF.Employee)));
            Assert.True(entityTypes.Contains(typeof(Microsoft.Web.Http.Data.Test.Models.EF.Product)));
            Assert.True(entityTypes.Contains(typeof(Microsoft.Web.Http.Data.Test.Models.EF.Category)));
            Assert.True(entityTypes.Contains(typeof(Microsoft.Web.Http.Data.Test.Models.EF.Supplier)));
            Assert.True(entityTypes.Contains(typeof(Microsoft.Web.Http.Data.Test.Models.EF.Shipper)));
        }

        /// <summary>
        /// Verify that associated entities are correctly registered in the description when
        /// using implicit data contracts
        /// </summary>
        [Fact]
        public void AssociatedEntityTypeDiscovery_ImplicitDataContract()
        {
            DataControllerDescription description = GetDataControllerDescription(typeof(IncludedAssociationTestController_ImplicitDataContract));
            List<Type> entityTypes = description.EntityTypes.ToList();
            Assert.Equal(3, entityTypes.Count);
            Assert.True(entityTypes.Contains(typeof(Microsoft.Web.Http.Data.Test.Models.Customer)));
            Assert.True(entityTypes.Contains(typeof(Microsoft.Web.Http.Data.Test.Models.Order)));
            Assert.True(entityTypes.Contains(typeof(Microsoft.Web.Http.Data.Test.Models.Order_Detail)));
        }

        /// <summary>
        /// Verify that DataControllerDescription correctly handles Task returning actions and discovers
        /// entity types from those as well (unwrapping the task type).
        /// </summary>
        [Fact]
        public void TaskReturningGetActions()
        {
            DataControllerDescription desc = GetDataControllerDescription(typeof(TaskReturningGetActionsController));
            Assert.Equal(4, desc.EntityTypes.Count());
            Assert.True(desc.EntityTypes.Contains(typeof(City)));
            Assert.True(desc.EntityTypes.Contains(typeof(CityWithInfo)));
            Assert.True(desc.EntityTypes.Contains(typeof(CityWithEditHistory)));
            Assert.True(desc.EntityTypes.Contains(typeof(State)));
        }

        internal static DataControllerDescription GetDataControllerDescription(Type controllerType)
        {
            HttpConfiguration configuration = new HttpConfiguration();
            HttpControllerDescriptor controllerDescriptor = new HttpControllerDescriptor
            {
                Configuration = configuration,
                ControllerType = controllerType
            };
            return DataControllerDescription.GetDescription(controllerDescriptor);
        }
    }

    internal class InvalidController_NonAuthMethodFilter : DataController
    {
        // attempt to apply a non-auth filter
        [TestActionFilter]
        public void UpdateProduct(Microsoft.Web.Http.Data.Test.Models.EF.Product product)
        {
        }

        // the restriction doesn't apply for non CUD actions
        [TestActionFilter]
        public IEnumerable<Microsoft.Web.Http.Data.Test.Models.EF.Product> GetProducts()
        {
            return null;
        }
    }

    internal class TaskReturningGetActionsController : DataController
    {
        public Task<IEnumerable<City>> GetCities()
        {
            return null;
        }

        public Task<State> GetState(string name)
        {
            return null;
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public class TestActionFilterAttribute : ActionFilterAttribute
    {
    }

    internal class IncludedAssociationTestController_ExplicitDataContract : LinqToEntitiesDataController<Microsoft.Web.Http.Data.Test.Models.EF.NorthwindEntities>
    {
        public IQueryable<Microsoft.Web.Http.Data.Test.Models.EF.Order> GetOrders() { return null; }
    }

    internal class IncludedAssociationTestController_ImplicitDataContract : DataController
    {
        public IQueryable<Microsoft.Web.Http.Data.Test.Models.Customer> GetCustomers() { return null; }
    }
}
