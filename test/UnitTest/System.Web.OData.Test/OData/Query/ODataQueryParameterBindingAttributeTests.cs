﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Metadata;
using System.Web.Http.Routing;
using System.Web.OData.Builder;
using System.Web.OData.Extensions;
using System.Web.OData.Query.Controllers;
using System.Web.OData.TestCommon.Models;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Microsoft.TestCommon;
using Moq;
using ODataPath = System.Web.OData.Routing.ODataPath;

namespace System.Web.OData.Query
{
    public class ODataQueryParameterBindingAttributeTests
    {
        public static TheoryDataSet<string, Type> GoodMethodNames
        {
            get
            {
                return new TheoryDataSet<string, Type>
                {
                    { "Get", typeof(Customer) },
                    { "GetIEnumerableOfCustomer", typeof(BellevueCustomer) },
                    { "GetCollectionOfCustomer", typeof(SeattleCustomer) },
                    { "GetListOfCustomer", typeof(RedmondCustomer) },
                    { "GetStronglyTypedCustomer", typeof(Customer) },
                    { "GetObject_WithODataQueryOptionsOfT", typeof(Customer) },
                    { "GetNonQueryable_WithODataQueryOptionsOfT", typeof(Customer) },
                    { "GetTwoGenericsCollection_WithODataQueryOptionsOfT", typeof(Customer) },
                };
            }
        }

        public static TheoryDataSet<string> BadMethodNames
        {
            get
            {
                return new TheoryDataSet<string>
                {
                    "GetObject",
                    "GetNonQueryable",
                    "GetTwoGenericsCollection",
                };
            }
        }

        [Theory]
        [PropertyData("GoodMethodNames")]
        public void DifferentReturnTypeWorks(string methodName, Type entityClrType)
        {
            // Arrange
            ODataModelBuilder odataModel = new ODataModelBuilder();
            string setName = typeof(Customer).Name;
            odataModel.EntityType<Customer>().HasKey(c => c.Id);
            odataModel.EntitySet<Customer>(setName);
            IEdmModel model = odataModel.GetEdmModel();
            ODataQueryParameterBindingAttribute attribute = new ODataQueryParameterBindingAttribute();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/Customer/?$orderby=Name");
            request.EnableODataDependencyInjectionSupport(model);
            HttpControllerContext controllerContext = new HttpControllerContext(request.GetConfiguration(), new HttpRouteData(new HttpRoute()), request);
            HttpControllerDescriptor controllerDescriptor = new HttpControllerDescriptor(request.GetConfiguration(), "CustomerLowLevel", typeof(CustomerHighLevelController));
            MethodInfo methodInfo = typeof(CustomerLowLevelController).GetMethod(methodName);
            ParameterInfo parameterInfo = methodInfo.GetParameters().First();
            HttpActionDescriptor actionDescriptor = new ReflectedHttpActionDescriptor(controllerDescriptor, methodInfo);
            HttpActionContext actionContext = new HttpActionContext(controllerContext, actionDescriptor);
            HttpParameterDescriptor parameterDescriptor = new ReflectedHttpParameterDescriptor(actionDescriptor, parameterInfo);

            // Act
            attribute.GetBinding(parameterDescriptor).ExecuteBindingAsync((ModelMetadataProvider)null, actionContext, CancellationToken.None).Wait();

            // Assert
            Assert.Equal(1, actionContext.ActionArguments.Count);
            ODataQueryOptions options = actionContext.ActionArguments[parameterDescriptor.ParameterName] as ODataQueryOptions;
            Assert.NotNull(options);
            Assert.Equal(entityClrType, options.Context.ElementClrType);
        }

        [Theory]
        [PropertyData("BadMethodNames")]
        public void BadReturnTypeThrows(string methodName)
        {
            // Arrange
            ODataQueryParameterBindingAttribute attribute = new ODataQueryParameterBindingAttribute();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/Customer/?$orderby=Name");
            HttpConfiguration config = new HttpConfiguration();
            config.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            request.SetConfiguration(config);
            HttpControllerContext controllerContext = new HttpControllerContext(config, new HttpRouteData(new HttpRoute()), request);
            HttpControllerDescriptor controllerDescriptor = new HttpControllerDescriptor(new HttpConfiguration(), "CustomerLowLevel", typeof(CustomerHighLevelController));
            MethodInfo methodInfo = typeof(CustomerLowLevelController).GetMethod(methodName);
            ParameterInfo parameterInfo = methodInfo.GetParameters().First();
            HttpActionDescriptor actionDescriptor = new ReflectedHttpActionDescriptor(controllerDescriptor, methodInfo);
            HttpActionContext actionContext = new HttpActionContext(controllerContext, actionDescriptor);
            HttpParameterDescriptor parameterDescriptor = new ReflectedHttpParameterDescriptor(actionDescriptor, parameterInfo);

            // Act
            HttpParameterBinding binding = attribute.GetBinding(parameterDescriptor);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(
                () => binding.ExecuteBindingAsync((ModelMetadataProvider)null, actionContext, CancellationToken.None).Wait(),
                String.Format(
                        "Cannot create an EDM model as the action '{0}' on controller '{1}' has a return type '{2}' that does not implement IEnumerable<T>.",
                        actionDescriptor.ActionName,
                        actionDescriptor.ControllerDescriptor.ControllerName,
                        actionDescriptor.ReturnType.FullName));
        }

        [Fact]
        public void VoidReturnTypeThrows()
        {
            // Arrange
            ODataQueryParameterBindingAttribute attribute = new ODataQueryParameterBindingAttribute();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/Customer/?$orderby=Name");
            HttpConfiguration config = new HttpConfiguration();
            config.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            request.SetConfiguration(config);
            HttpControllerContext controllerContext = new HttpControllerContext(config, new HttpRouteData(new HttpRoute()), request);
            HttpControllerDescriptor controllerDescriptor = new HttpControllerDescriptor(new HttpConfiguration(), "CustomerLowLevel", typeof(CustomerHighLevelController));
            MethodInfo methodInfo = typeof(CustomerLowLevelController).GetMethod("GetVoidReturn");
            ParameterInfo parameterInfo = methodInfo.GetParameters().First();
            HttpActionDescriptor actionDescriptor = new ReflectedHttpActionDescriptor(controllerDescriptor, methodInfo);
            HttpActionContext actionContext = new HttpActionContext(controllerContext, actionDescriptor);
            HttpParameterDescriptor parameterDescriptor = new ReflectedHttpParameterDescriptor(actionDescriptor, parameterInfo);
            HttpParameterBinding binding = attribute.GetBinding(parameterDescriptor);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(
                () => binding.ExecuteBindingAsync((ModelMetadataProvider)null, actionContext, CancellationToken.None).Wait(),
                "Cannot create an EDM model as the action 'GetVoidReturn' on controller 'CustomerLowLevel' has a void return type.");
        }

        [Theory]
        [InlineData(typeof(int[]), typeof(int))]
        [InlineData(typeof(IEnumerable<int>), typeof(int))]
        [InlineData(typeof(List<int>), typeof(int))]
        [InlineData(typeof(IQueryable<int>), typeof(int))]
        [InlineData(typeof(Task<IQueryable<int>>), typeof(int))]
        public void GetEntityClrTypeFromActionReturnType_Returns_CorrectEntityType(Type returnType, Type elementType)
        {
            Mock<HttpActionDescriptor> action = new Mock<HttpActionDescriptor>();
            action.Setup(a => a.ReturnType).Returns(returnType);

            Assert.Equal(
                elementType,
                ODataQueryParameterBindingAttribute.ODataQueryParameterBinding.GetEntityClrTypeFromActionReturnType(action.Object));
        }

        [Theory]
        [InlineData(typeof(ODataQueryOptions<int>), typeof(int))]
        [InlineData(typeof(ODataQueryOptions<string>), typeof(string))]
        [InlineData(typeof(ODataQueryOptions), null)]
        [InlineData(typeof(int), null)]
        public void GetEntityClrTypeFromParameterType_Returns_CorrectEntityType(Type parameterType, Type elementType)
        {
            Mock<HttpParameterDescriptor> parameter = new Mock<HttpParameterDescriptor>();
            parameter.Setup(p => p.ParameterType).Returns(parameterType);

            Assert.Equal(
                elementType,
                ODataQueryParameterBindingAttribute.ODataQueryParameterBinding.GetEntityClrTypeFromParameterType(parameter.Object));
        }

        [Theory]
        [PropertyData("GoodMethodNames")]
        public void ExecuteBindingAsync_Works_WithPath(string methodName, Type entityClrType)
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage(
                HttpMethod.Get,
                "http://localhost/Customer/?$orderby=Name");

            // Get EDM model, and set path to request.
            ODataModelBuilder odataModel = new ODataModelBuilder();
            string setName = typeof(Customer).Name;
            odataModel.EntityType<Customer>().HasKey(c => c.Id);
            odataModel.EntitySet<Customer>(setName);
            IEdmModel model = odataModel.GetEdmModel();
            IEdmEntitySet entitySet = model.EntityContainer.FindEntitySet(setName);
            request.ODataProperties().Path = new ODataPath(new EntitySetSegment(entitySet));
            request.EnableODataDependencyInjectionSupport(model);

            // Setup action context and parameter descriptor.
            HttpControllerContext controllerContext = new HttpControllerContext(
                request.GetConfiguration(),
                new HttpRouteData(new HttpRoute()),
                request);
            HttpControllerDescriptor controllerDescriptor = new HttpControllerDescriptor(
                request.GetConfiguration(),
                "CustomerLowLevel",
                typeof(CustomerHighLevelController));
            MethodInfo methodInfo = typeof(CustomerLowLevelController).GetMethod(methodName);
            HttpActionDescriptor actionDescriptor = new ReflectedHttpActionDescriptor(controllerDescriptor, methodInfo);
            HttpActionContext actionContext = new HttpActionContext(controllerContext, actionDescriptor);
            HttpParameterDescriptor parameterDescriptor = new ReflectedHttpParameterDescriptor(
                actionDescriptor,
                methodInfo.GetParameters().First());

            // Act
            new ODataQueryParameterBindingAttribute().GetBinding(parameterDescriptor)
                .ExecuteBindingAsync((ModelMetadataProvider)null, actionContext, CancellationToken.None)
                .Wait();

            // Assert
            Assert.Equal(1, actionContext.ActionArguments.Count);
            ODataQueryOptions options =
                actionContext.ActionArguments[parameterDescriptor.ParameterName] as ODataQueryOptions;
            Assert.NotNull(options);
            Assert.Same(model, options.Context.Model);
            Assert.Same(entitySet, options.Context.NavigationSource);
            Assert.Same(entityClrType, options.Context.ElementClrType);
        }
    }

    public class CustomerLowLevelController : ODataController
    {
        public IQueryable<Customer> Get(ODataQueryOptions options)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<BellevueCustomer> GetIEnumerableOfCustomer(ODataQueryOptions options)
        {
            throw new NotImplementedException();
        }

        public Collection<SeattleCustomer> GetCollectionOfCustomer(ODataQueryOptions options)
        {
            throw new NotImplementedException();
        }

        public List<RedmondCustomer> GetListOfCustomer(ODataQueryOptions options)
        {
            throw new NotImplementedException();
        }

        public CustomerCollection GetStronglyTypedCustomer(ODataQueryOptions options)
        {
            throw new NotImplementedException();
        }

        public object GetObject(ODataQueryOptions options)
        {
            // this can return Customer or BellevueCustomer
            throw new NotImplementedException();
        }

        public IEnumerable GetNonQueryable(ODataQueryOptions options)
        {
            throw new NotImplementedException();
        }

        public TwoGenericsCollection GetTwoGenericsCollection(ODataQueryOptions options)
        {
            throw new NotImplementedException();
        }

        public void GetVoidReturn(ODataQueryOptions options)
        {
            throw new NotImplementedException();
        }

        public object GetObject_WithODataQueryOptionsOfT(ODataQueryOptions<Customer> options)
        {
            // this can return Customer or BellevueCustomer
            throw new NotImplementedException();
        }

        public IEnumerable GetNonQueryable_WithODataQueryOptionsOfT(ODataQueryOptions<Customer> options)
        {
            throw new NotImplementedException();
        }

        public TwoGenericsCollection GetTwoGenericsCollection_WithODataQueryOptionsOfT(ODataQueryOptions<Customer> options)
        {
            throw new NotImplementedException();
        }

        public void GetVoidReturn_WithODataQueryOptionsOfT(ODataQueryOptions<Customer> options)
        {
            throw new NotImplementedException();
        }
    }
}
