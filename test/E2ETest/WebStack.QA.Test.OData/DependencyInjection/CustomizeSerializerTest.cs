// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Formatter.Serialization;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Nuwa;
using WebStack.QA.Test.OData.Common;
using Xunit;

namespace WebStack.QA.Test.OData.DependencyInjection
{
    public class CustomizeSerializerTest : ODataTestBase
    {
        private const string CustomerBaseUrl = "{0}/dependencyinjection/Customers";

        [NuwaConfiguration]
        public static void UpdateConfiguration(HttpConfiguration configuration)
        {
            configuration.Services.Replace(
                typeof(IAssembliesResolver),
                new TestAssemblyResolver(typeof(CustomersController), typeof(OrdersController)));
            configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            configuration.Formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling =
                Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            configuration.MapODataServiceRoute("dependencyinjection", "dependencyinjection", builder =>
                builder.AddService(ServiceLifetime.Singleton, sp => EdmModel.GetEdmModel())
                       .AddService<IEnumerable<IODataRoutingConvention>>(ServiceLifetime.Singleton, sp =>
                           ODataRoutingConventions.CreateDefaultWithAttributeRouting("dependencyinjection", configuration))
                       .AddService<ODataSerializerProvider, MyODataSerializerProvider>(ServiceLifetime.Singleton)
                       .AddService<ODataResourceSerializer, AnnotatingEntitySerializer>(ServiceLifetime.Singleton));
        }

        [Fact]
        public void CutomizeSerializerProvider()
        {
            string queryUrl =
                string.Format(
                    CustomerBaseUrl + "/Default.EnumFunction()",
                    BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = new HttpClient();

            HttpResponseMessage response = client.SendAsync(request).Result;
            string result = response.Content.ReadAsStringAsync().Result;

            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
            Assert.Contains(MyODataSerializerProvider.EnumNotSupportError, result);
        }

        [Fact]
        public void CutomizeSerializer()
        {
            string queryUrl =
                string.Format(
                    CustomerBaseUrl + "(1)",
                    BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            request.Headers.Add("Prefer", "odata.include-annotations=\"*\"");
            HttpClient client = new HttpClient();

            HttpResponseMessage response = client.SendAsync(request).Result;
            string result = response.Content.ReadAsStringAsync().Result;

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