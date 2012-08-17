// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Web.Http.Controllers;
using System.Web.Http.Hosting;
using System.Web.Http.Metadata;
using System.Web.Http.OData.Query.Controllers;
using System.Web.Http.OData.TestCommon.Models;
using System.Web.Http.Routing;
using Microsoft.TestCommon;
using Xunit;
using Xunit.Extensions;

namespace System.Web.Http.OData.Query
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
                    { "GetStronglyTypedCustomer", typeof(Customer) }
                };
            }
        }

        public static TheoryDataSet<string> BadMethodNames
        {
            get
            {
                return new TheoryDataSet<string>
                {
                    { "GetObject" },
                    { "GetNonQueryable" },
                    { "GetTwoGenericsCollection" }
                };
            }
        }

        [Theory]
        [PropertyData("GoodMethodNames")]
        public void DifferentReturnTypeWorks(string methodName, Type entityClrType)
        {
            // Arrange
            ODataQueryParameterBindingAttribute attribute = new ODataQueryParameterBindingAttribute();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/Customer/?$orderby=Name");
            HttpConfiguration config = new HttpConfiguration();
            request.Properties[HttpPropertyKeys.HttpConfigurationKey] = config;
            HttpControllerContext controllerContext = new HttpControllerContext(config, new HttpRouteData(new HttpRoute()), request);
            HttpControllerDescriptor controllerDescriptor = new HttpControllerDescriptor(new HttpConfiguration(), "CustomerLowLevel", typeof(CustomerHighLevelController));
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
            Assert.Equal(entityClrType, options.Context.EntityClrType);
        }

        [Theory]
        [PropertyData("BadMethodNames")]
        public void BadReturnTypeThrows(string methodName)
        {
            // Arrange
            ODataQueryParameterBindingAttribute attribute = new ODataQueryParameterBindingAttribute();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/Customer/?$orderby=Name");
            HttpConfiguration config = new HttpConfiguration();
            request.Properties[HttpPropertyKeys.HttpConfigurationKey] = config;
            HttpControllerContext controllerContext = new HttpControllerContext(config, new HttpRouteData(new HttpRoute()), request);
            HttpControllerDescriptor controllerDescriptor = new HttpControllerDescriptor(new HttpConfiguration(), "CustomerLowLevel", typeof(CustomerHighLevelController));
            MethodInfo methodInfo = typeof(CustomerLowLevelController).GetMethod(methodName);
            ParameterInfo parameterInfo = methodInfo.GetParameters().First();
            HttpActionDescriptor actionDescriptor = new ReflectedHttpActionDescriptor(controllerDescriptor, methodInfo);
            HttpActionContext actionContext = new HttpActionContext(controllerContext, actionDescriptor);
            HttpParameterDescriptor parameterDescriptor = new ReflectedHttpParameterDescriptor(actionDescriptor, parameterInfo);

            // Act
            HttpParameterBinding binding = attribute.GetBinding(parameterDescriptor);

            HttpResponseException responseException = Assert.Throws<HttpResponseException>(() =>
            binding.ExecuteBindingAsync((ModelMetadataProvider)null, actionContext, CancellationToken.None).Wait());
            Assert.Equal(HttpStatusCode.InternalServerError, responseException.Response.StatusCode);
        }
    }

    public class CustomerLowLevelController : ApiController
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
    }
}
