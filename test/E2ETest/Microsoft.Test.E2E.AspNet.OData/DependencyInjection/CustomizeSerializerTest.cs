// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Formatter.Serialization;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.DependencyInjection
{
    public class CustomizeSerializerTest : WebHostTestBase<CustomizeSerializerTest>
    {
        private const string CustomerBaseUrl = "{0}/customserializer/Customers";

        public CustomizeSerializerTest(WebHostTestFixture<CustomizeSerializerTest> fixture)
            :base(fixture)
        {
        }

        protected static void UpdateConfigure(WebRouteConfiguration configuration)
        {
            configuration.AddControllers(typeof(CustomersController), typeof(OrdersController));
            configuration.JsonReferenceLoopHandling =
                Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            configuration.MapODataServiceRoute("customserializer", "customserializer", builder =>
                builder.AddService(ServiceLifetime.Singleton, sp => EdmModel.GetEdmModel(configuration))
                       .AddService<IEnumerable<IODataRoutingConvention>>(ServiceLifetime.Singleton, sp =>
                           ODataRoutingConventions.CreateDefaultWithAttributeRouting("customserializer", configuration))
                       .AddService<ODataSerializerProvider, MyODataSerializerProvider>(ServiceLifetime.Singleton)
                       .AddService<ODataResourceSerializer, AnnotatingEntitySerializer>(ServiceLifetime.Singleton));
        }

        [Fact]
        public async Task CutomizeSerializerProvider()
        {
            string queryUrl =
                string.Format(
                    CustomerBaseUrl + "/Default.EnumFunction()",
                    BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = new HttpClient();

            HttpResponseMessage response = await client.SendAsync(request);
            string result = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
            Assert.Contains(MyODataSerializerProvider.EnumNotSupportError, result);
        }

        [Fact]
        public async Task CutomizeSerializer()
        {
            string queryUrl =
                string.Format(
                    CustomerBaseUrl + "(1)",
                    BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            request.Headers.Add("Prefer", "odata.include-annotations=\"*\"");
            HttpClient client = new HttpClient();

            HttpResponseMessage response = await client.SendAsync(request);
            string result = await response.Content.ReadAsStringAsync();

            response.EnsureSuccessStatusCode();
            Assert.Contains("@dependency.injection.test\":1", result);
        }
    }

    public class MyODataSerializerProvider : DefaultODataSerializerProvider
    {
        public const string EnumNotSupportError = "Enum kind is not support.";

        public MyODataSerializerProvider(IServiceProvider rootContainer)
            : base(rootContainer)
        {
        }

        public override ODataEdmTypeSerializer GetEdmTypeSerializer(IEdmTypeReference edmType)
        {
            if (edmType.TypeKind() == EdmTypeKind.Enum)
            {
                throw new ArgumentException(EnumNotSupportError);
            }

            return base.GetEdmTypeSerializer(edmType);
        }
    }

    // A custom entity serializer that adds the score annotation to document entries.
    public class AnnotatingEntitySerializer : ODataResourceSerializer
    {
        public AnnotatingEntitySerializer(ODataSerializerProvider serializerProvider)
            : base(serializerProvider)
        {
        }

        public override ODataResource CreateResource(SelectExpandNode selectExpandNode, ResourceContext resourceContext)
        {
            ODataResource resource = base.CreateResource(selectExpandNode, resourceContext);
            Customer customer = resourceContext.ResourceInstance as Customer;
            if (customer != null)
            {
                resource.InstanceAnnotations.Add(new ODataInstanceAnnotation("dependency.injection.test",
                    new ODataPrimitiveValue(customer.Id)));
            }
            return resource;
        }
    }
}