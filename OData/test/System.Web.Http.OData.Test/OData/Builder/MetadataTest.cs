// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Csdl;
using Microsoft.Data.Edm.Validation;
using Microsoft.TestCommon;

namespace System.Web.Http.OData.Builder
{
    public class MetadataTest
    {
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

        public static string GetCSDL(IEdmModel model)
        {
            StringWriter writer = new StringWriter();
            var xwriter = XmlWriter.Create(writer);
            IEnumerable<EdmError> errors;
            if (EdmxWriter.TryWriteEdmx(model, xwriter, EdmxTarget.OData, out errors))
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
