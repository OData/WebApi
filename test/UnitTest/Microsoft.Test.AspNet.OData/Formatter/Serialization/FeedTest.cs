// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Microsoft.Test.AspNet.OData.TestCommon;
using Microsoft.Test.AspNet.OData.TestCommon.Models;
using ODataPath = Microsoft.AspNet.OData.Routing.ODataPath;

namespace Microsoft.Test.AspNet.OData.Formatter.Serialization
{
    public class FeedTest
    {
        private IEdmModel _model = GetSampleModel();

        [Fact]
        public void IEnumerableOfEntityTypeSerializesAsODataFeed()
        {
            // Arrange
            ODataMediaTypeFormatter formatter = CreateFormatter();

            IEnumerable<Employee> collectionOfPerson = new Collection<Employee>()
            {
                (Employee)TypeInitializer.GetInstance(SupportedTypes.Employee, 0),
                (Employee)TypeInitializer.GetInstance(SupportedTypes.Employee, 1),
            };

            ObjectContent<IEnumerable<Employee>> content = new ObjectContent<IEnumerable<Employee>>(collectionOfPerson,
                formatter, MediaTypeHeaderValue.Parse(ODataMediaTypes.ApplicationJsonODataMinimalMetadata));

            // Act & Assert
            JsonAssert.Equal(Resources.FeedOfEmployee, content.ReadAsStringAsync().Result);
        }

        private ODataMediaTypeFormatter CreateFormatter()
        {
            ODataMediaTypeFormatter formatter = new ODataMediaTypeFormatter(new ODataPayloadKind[] { ODataPayloadKind.ResourceSet });
            formatter.Request = GetSampleRequest();
            formatter.SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse(ODataMediaTypes.ApplicationJsonODataMinimalMetadata));
            return formatter;
        }

        private HttpRequestMessage GetSampleRequest()
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/employees");
            HttpConfiguration configuration = new HttpConfiguration();
            string routeName = "Route";
            configuration.MapODataServiceRoute(routeName, null, GetSampleModel());
            request.SetConfiguration(configuration);
            IEdmEntitySet entitySet = _model.EntityContainer.FindEntitySet("employees");
            request.ODataProperties().Path = new ODataPath(new EntitySetSegment(entitySet));
            request.EnableODataDependencyInjectionSupport(routeName);
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
