// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using System.Net.Http;
using System.Web.Http.OData.Builder;
using System.Web.Http.OData.Extensions;
using System.Web.Http.OData.Routing;
using System.Web.Http.OData.TestCommon.Models;
using Microsoft.Data.Edm;
using Microsoft.Data.OData;
using Microsoft.TestCommon;

namespace System.Web.Http.OData.Formatter.Serialization
{
    public class EntityTypeTest
    {
        private IEdmModel _model = GetSampleModel();

        [Fact]
        public void EntityTypeSerializesAsODataEntryForJsonLight()
        {
            EntityTypeSerializesAsODataEntry(Resources.EmployeeEntryInJsonLight, true);
        }

        [Fact]
        public void EntityTypeSerializesAsODataEntryForAtom()
        {
            EntityTypeSerializesAsODataEntry(Resources.EmployeeEntryInAtom, false);
        }

        private void EntityTypeSerializesAsODataEntry(string expectedContent, bool json)
        {
            ODataMediaTypeFormatter formatter = CreateFormatter();
            Employee employee = (Employee)TypeInitializer.GetInstance(SupportedTypes.Employee);
            ObjectContent<Employee> content = new ObjectContent<Employee>(employee, formatter, json ?
                ODataMediaTypes.ApplicationJsonODataMinimalMetadata : ODataMediaTypes.ApplicationAtomXmlTypeEntry);

            string actualContent = content.ReadAsStringAsync().Result;

            if (json)
            {
                JsonAssert.Equal(expectedContent, actualContent);
            }
            else
            {
                RegexReplacement replaceUpdateTime = new RegexReplacement(
                    "<updated>*.*</updated>", "<updated>UpdatedTime</updated>");
                Assert.Xml.Equal(expectedContent, actualContent, replaceUpdateTime);
            }
        }

        private ODataMediaTypeFormatter CreateFormatter()
        {
            ODataMediaTypeFormatter formatter = new ODataMediaTypeFormatter(new ODataPayloadKind[] { ODataPayloadKind.Entry });
            formatter.Request = GetSampleRequest();
            formatter.SupportedMediaTypes.Add(ODataMediaTypes.ApplicationJsonODataMinimalMetadata);
            formatter.SupportedMediaTypes.Add(ODataMediaTypes.ApplicationAtomXmlTypeEntry);
            return formatter;
        }

        private HttpRequestMessage GetSampleRequest()
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/employees");
            request.ODataProperties().Model = _model;
            HttpConfiguration configuration = new HttpConfiguration();
            string routeName = "Route";
            configuration.Routes.MapODataServiceRoute(routeName, null, _model);
            request.SetConfiguration(configuration);
            IEdmEntitySet entitySet = _model.EntityContainers().Single().FindEntitySet("employees");
            request.ODataProperties().Path = new ODataPath(new EntitySetPathSegment(entitySet));
            request.ODataProperties().RouteName = routeName;
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
