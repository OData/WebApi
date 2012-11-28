// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Net.Http;
using System.Web.Http.Hosting;
using System.Web.Http.OData.Builder;
using System.Web.Http.OData.TestCommon.Models;
using System.Web.Http.Routing;
using Microsoft.Data.Edm;
using Microsoft.Data.OData;
using Microsoft.TestCommon;

namespace System.Web.Http.OData.Formatter.Serialization
{
    public class EntityTypeTest
    {
        [Fact]
        public void EntityTypeSerializesAsODataEntry()
        {
            ODataMediaTypeFormatter formatter = CreateFormatter();
            Employee employee = (Employee)TypeInitializer.GetInstance(SupportedTypes.Employee);
            ObjectContent<Employee> content = new ObjectContent<Employee>(employee, formatter);

            RegexReplacement replaceUpdateTime = new RegexReplacement("<updated>*.*</updated>", "<updated>UpdatedTime</updated>");
            Assert.Xml.Equal(BaselineResource.TestEntityTypeBasic, content.ReadAsStringAsync().Result, regexReplacements: replaceUpdateTime);
        }

        [Fact]
        public void ContentHeadersAreAddedForXmlMediaType()
        {
            ODataMediaTypeFormatter formatter = CreateFormatter();
            ObjectContent<Employee> content = new ObjectContent<Employee>(new Employee(0, new ReferenceDepthContext(7)), formatter);
            content.LoadIntoBufferAsync().Wait();

            Assert.Http.Contains(content.Headers, "DataServiceVersion", "3.0;");
            Assert.Http.Contains(content.Headers, "Content-Type", "application/atom+xml; type=entry; charset=utf-8");
        }

        [Fact]
        public void ContentHeadersAreAddedForJsonMediaType()
        {
            ODataMediaTypeFormatter formatter = CreateFormatter();
            HttpContent content = new ObjectContent<Employee>(new Employee(0, new ReferenceDepthContext(7)), formatter, "application/json");
            content.LoadIntoBufferAsync().Wait();

            Assert.Http.Contains(content.Headers, "DataServiceVersion", "3.0;");
            Assert.Http.Contains(content.Headers, "Content-Type", "application/json; odata=verbose; charset=utf-8");
        }

        private static ODataMediaTypeFormatter CreateFormatter()
        {
            ODataMediaTypeFormatter formatter = new ODataMediaTypeFormatter(GetSampleModel());
            formatter.Request = GetSampleRequest();
            return formatter;
        }

        private static HttpRequestMessage GetSampleRequest()
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/employees");
            HttpConfiguration config = new HttpConfiguration();
            config.Routes.MapHttpRoute(ODataRouteNames.GetById, "{controller}({id})");
            config.Routes.MapHttpRoute(ODataRouteNames.PropertyNavigation, "{controller}({parentId})/{navigationProperty}");
            request.Properties[HttpPropertyKeys.HttpConfigurationKey] = config;
            request.Properties[HttpPropertyKeys.HttpRouteDataKey] = new HttpRouteData(new HttpRoute());
            return request;
        }

        private static IEdmModel GetSampleModel()
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<Employee>("employees");
            builder.EntitySet<WorkItem>("workitems");
            return builder.GetEdmModel();
        }
    }
}
