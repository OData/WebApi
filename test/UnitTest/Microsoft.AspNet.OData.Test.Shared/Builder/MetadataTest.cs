﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Test.Builder.TestModels;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Csdl;
using Microsoft.OData.Edm.Validation;
using Xunit;

namespace Microsoft.AspNet.OData.Test.Builder
{
    public class MetadataTest
    {
        [Fact]
        [Trait("Description", "Partner relationhips are emitted correctly")]
        public void CanEmitPartnerRelationship()
        {
            var builder = new ODataModelBuilder()
                .Add_Company_EntityType()
                .Add_Employee_EntityType()
                .Add_CompanyEmployees_Relationship();
            var model = builder.GetServiceModel();
            var company = model.SchemaElements.Single(e => e.Name == nameof(Company)) as EdmEntityType;
            var ceo = company.Properties().Single(p => p.Name == nameof(Company.CEO)) as EdmNavigationProperty;
            var employee = model.SchemaElements.Single(e => e.Name == nameof(Employee)) as EdmEntityType;
            var isCeoOf = employee.Properties().Single(p => p.Name == nameof(Employee.IsCeoOf)) as EdmNavigationProperty;
            Assert.Equal(ceo.Partner, isCeoOf);
            var csdl = GetCSDL(model);
        }

        [Fact]
        [Trait("Description", "Edmlib can emit a model with a single EntityType only")]
        public void CanEmitModelWithSingleEntity()
        {
            var builder = new ODataModelBuilder().Add_Customer_EntityType();
            var model = builder.GetServiceModel();
            var csdl = GetCSDL(model);
        }

        [Fact]
        [Trait("Description", "Edmlib can emit a model with a single ComplexType only")]
        public void CanEmitModelWithSingleComplexType()
        {
            var builder = new ODataModelBuilder().Add_Address_ComplexType();
            var model = builder.GetServiceModel();
            var csdl = GetCSDL(model);
        }

        [Fact]
        [Trait("Description", "Edmlib can emit a model with two EntityTypes and one way relationship")]
        public void CanEmitModelWithTwoEntitiesAndARelationship()
        {
            var builder = new ODataModelBuilder().Add_Order_EntityType().Add_Customer_EntityType().Add_CustomerOrders_Relationship();
            var model = builder.GetServiceModel();
            var csdl = GetCSDL(model);
        }

        [Fact]
        [Trait("Description", "Edmlib can emit a model with two EntityTypes and one way relationship")]
        public void CanEmitModelWithTwoEntitiesAndAOneWayRelationship()
        {
            var builder = new ODataModelBuilder()
                .Add_Order_EntityType()
                .Add_Customer_EntityType()
                .Add_CustomerOrders_Relationship()
                .Add_Customers_EntitySet()
                .Add_Orders_EntitySet()
                .Add_CustomerOrders_Binding();
            var model = builder.GetServiceModel();
            var csdl = GetCSDL(model);
        }

        [Fact]
        [Trait("Description", "Edmlib can emit a model with two EntityTypes and two way relationship")]
        public void CanEmitModelWithTwoEntitiesAndATwoWayRelationship()
        {
            var builder = new ODataModelBuilder()
                .Add_Order_EntityType()
                .Add_Customer_EntityType()
                .Add_Customers_EntitySet()
                .Add_Orders_EntitySet()
                .Add_CustomerOrders_Binding() // creates nav prop too
                .Add_OrderCustomer_Binding(); // creates nav prop too
            var model = builder.GetServiceModel();
            var csdl = GetCSDL(model);
        }

        [Fact]
        [Trait("Description", "Edmlib can emit a model with two EntityTypes and two way relationship")]
        public void CanEmitModelWithTwoEntitiesAndARelationshipWithNoBinding()
        {
            var builder = new ODataModelBuilder()
                .Add_Order_EntityType()
                .Add_Customer_EntityType()
                .Add_CustomerOrders_Relationship()
                .Add_Customers_EntitySet()
                .Add_Orders_EntitySet();
            var model = builder.GetServiceModel();
            var csdl = GetCSDL(model);
        }

        [Fact]
        public void CanEmitModelWithSingleton()
        {
            // Arrange
            var builder = new ODataModelBuilder()
                .Add_Company_Singleton();

            // Act
            var model = builder.GetServiceModel();

            // Assert
            var csdl = GetCSDL(model);
        }

        [Fact]
        public void CanEmitModelWithEntitySetHasSingletonBinding()
        {
            // Arrange
            var builder = new ODataModelBuilder()
                .Add_Company_EntityType()
                .Add_Employee_EntityType()
                .Add_CompanyEmployees_Relationship()
                .Add_EmployeeComplany_Relationship()
                .Add_CompaniesCEO_Binding();

            // Act
            var model = builder.GetServiceModel();

            // Assert
            var csdl = GetCSDL(model);
        }

        [Fact]
        public void CanEmitModelWithSingletonHasSingletonBinding()
        {
            // Arrange
            var builder = new ODataModelBuilder()
                .Add_Company_EntityType()
                .Add_Employee_EntityType()
                .Add_CompanyEmployees_Relationship()
                .Add_EmployeeComplany_Relationship()
                .Add_MicrosoftCEO_Binding();

            // Act
            var model = builder.GetServiceModel();

            // Assert
            var csdl = GetCSDL(model);
        }

        [Fact]
        public void CanEmitModelWithSingletonHasEntitysetBinding()
        {
            // Arrange
            var builder = new ODataModelBuilder()
                .Add_Company_EntityType()
                .Add_Employee_EntityType()
                .Add_CompanyEmployees_Relationship()
                .Add_EmployeeComplany_Relationship()
                .Add_MicrosoftEmployees_Binding();

            // Act
            var model = builder.GetServiceModel();

            // Assert
            var csdl = GetCSDL(model);
        }

        public static string GetCSDL(IEdmModel model)
        {
            StringWriter writer = new StringWriter();
            var xwriter = XmlWriter.Create(writer);
            IEnumerable<EdmError> errors;
            if (CsdlWriter.TryWriteCsdl(model, xwriter, CsdlTarget.OData, out errors))
            {
                xwriter.Flush();
                return writer.ToString();
            }
            else
            {
                throw new Exception(errors.First().ErrorMessage);
            }
        }
    }
}
