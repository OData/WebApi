﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

#if NETFX // HttpActionDescriptor.GetEdmModel implmention is part of EnableQueryAttribute.GetModel on AspNetCore
using System;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Controllers;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Test.Common.Models;
using Microsoft.AspNet.OData.Test.Query.Controllers;
using Microsoft.OData.Edm;
using Xunit;

namespace Microsoft.AspNet.OData.Test.Query
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
            Assert.Equal(11, model.SchemaElements.Count());
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
            Assert.Equal(11, model1.SchemaElements.Count());
            Assert.Contains(type1.Name, model1.SchemaElements.Select(e => e.Name));

            Assert.NotNull(model2);
            Assert.Equal(2, model2.SchemaElements.Count());
            Assert.Contains(type2.Name, model2.SchemaElements.Select(e => e.Name));
        }
    }
}
#endif
