// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Net.Http;
using System.Web.Http;
using System.Web.OData.Builder;
using System.Web.OData.Extensions;
using System.Web.OData.Routing;
using System.Web.OData.TestCommon.Models;
using Microsoft.OData.Core;
using Microsoft.OData.Edm;
using Microsoft.TestCommon;

namespace System.Web.OData.Formatter.Serialization
{
    public class EntityTypeTest
    {
        private IEdmModel _model = GetSampleModel();

        [Fact]
        public void EntityTypeSerializesAsODataEntry()
        {
            // Arrange
            ODataMediaTypeFormatter formatter = CreateFormatter();
            Employee employee = (Employee)TypeInitializer.GetInstance(SupportedTypes.Employee);
            ObjectContent<Employee> content = new ObjectContent<Employee>(employee, formatter,
                ODataMediaTypes.ApplicationJsonODataMinimalMetadata);

            // Act & Assert
            JsonAssert.Equal(Resources.EmployeeEntry, content.ReadAsStringAsync().Result);
        }

        private ODataMediaTypeFormatter CreateFormatter()
        {
            ODataMediaTypeFormatter formatter = new ODataMediaTypeFormatter(new ODataPayloadKind[] { ODataPayloadKind.Entry });
            formatter.Request = GetSampleRequest();
            formatter.SupportedMediaTypes.Add(ODataMediaTypes.ApplicationJsonODataMinimalMetadata);
            return formatter;
        }

        private HttpRequestMessage GetSampleRequest()
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/employees");
            request.ODataProperties().Model = _model;
            HttpConfiguration configuration = new HttpConfiguration();
            string routeName = "Route";
            configuration.MapODataServiceRoute(routeName, null, _model);
            request.SetConfiguration(configuration);
            IEdmEntitySet entitySet = _model.EntityContainer.FindEntitySet("employees");
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
