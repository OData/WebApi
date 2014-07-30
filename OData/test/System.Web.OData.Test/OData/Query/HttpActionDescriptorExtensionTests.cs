// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.OData.Extensions;
using System.Web.OData.Query.Controllers;
using System.Web.OData.TestCommon.Models;
using Microsoft.OData.Edm;
using Microsoft.TestCommon;

namespace System.Web.OData.Query
{
    public class HttpActionDescriptorExtensionTests
    {
        [Theory]
        [InlineData("Get", typeof(Customer))]
        public void GetEdmModelWorks(string methodName, Type entityClrType)
        {
            // Arrange
            HttpControllerDescriptor controllerDescriptor = new HttpControllerDescriptor(new HttpConfiguration(), "CustomerLowLevel", typeof(CustomerHighLevelController));
            HttpActionDescriptor actionDescriptor = new ReflectedHttpActionDescriptor(controllerDescriptor, typeof(CustomerHighLevelController).GetMethod(methodName));

            // Act
            IEdmModel model = actionDescriptor.GetEdmModel(entityClrType);

            // Assert
            Assert.NotNull(model);
            Assert.Equal(8, model.SchemaElements.Count());
            Assert.Contains(entityClrType.Name, model.SchemaElements.Select(e => e.Name));
            Assert.Same(model, actionDescriptor.GetEdmModel(entityClrType));
        }

        [Fact]
        public void GetEdmModelForMultipleTypesWorks()
        {
            // Arrange
            HttpControllerDescriptor controllerDescriptor = new HttpControllerDescriptor(new HttpConfiguration(), "CustomerLowLevel", typeof(CustomerHighLevelController));
            HttpActionDescriptor actionDescriptor = new ReflectedHttpActionDescriptor(controllerDescriptor, typeof(CustomerHighLevelController).GetMethod("GetObject"));

            Type type1 = typeof(Customer);
            Type type2 = typeof(BellevueCustomer);

            // Act
            IEdmModel model1 = actionDescriptor.GetEdmModel(type1);
            IEdmModel model2 = actionDescriptor.GetEdmModel(type2);

            // Assert
            Assert.NotSame(model1, model2);
            Assert.NotNull(model1);
            Assert.Equal(8, model1.SchemaElements.Count());
            Assert.Contains(type1.Name, model1.SchemaElements.Select(e => e.Name));

            Assert.NotNull(model2);
            Assert.Equal(2, model2.SchemaElements.Count());
            Assert.Contains(type2.Name, model2.SchemaElements.Select(e => e.Name));
        }
    }

}
