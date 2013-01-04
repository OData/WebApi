// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Web.Http.Hosting;
using System.Web.Http.OData.Builder;
using System.Web.Http.OData.Routing;
using System.Web.Http.OData.TestCommon.Models;
using System.Web.Http.Routing;
using Microsoft.Data.Edm;
using Microsoft.Data.OData;
using Microsoft.TestCommon;

namespace System.Web.Http.OData.Formatter.Serialization
{
    public class FeedTest
    {
        private IEdmModel _model = GetSampleModel();

        [Fact]
        public void IEnumerableOfEntityTypeSerializesAsODataFeedForJsonLight()
        {
            IEnumerableOfEntityTypeSerializesAsODataFeed(Resources.FeedOfEmployeeInJsonLight, true);
        }

        [Fact]
        public void IEnumerableOfEntityTypeSerializesAsODataFeedForAtom()
        {
            IEnumerableOfEntityTypeSerializesAsODataFeed(Resources.FeedOfEmployeeInAtom, false);
        }

        private void IEnumerableOfEntityTypeSerializesAsODataFeed(string expectedContent, bool json)
        {
            ODataMediaTypeFormatter formatter = CreateFormatter();

            IEnumerable<Employee> collectionOfPerson = new Collection<Employee>() 
            {
                (Employee)TypeInitializer.GetInstance(SupportedTypes.Employee, 0),
                (Employee)TypeInitializer.GetInstance(SupportedTypes.Employee, 1),
            };

            ObjectContent<IEnumerable<Employee>> content = new ObjectContent<IEnumerable<Employee>>(collectionOfPerson,
                formatter, json ? ODataMediaTypes.ApplicationJsonODataMinimalMetadata :
                ODataMediaTypes.ApplicationAtomXmlTypeFeed);

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
            ODataMediaTypeFormatter formatter = new ODataMediaTypeFormatter(new ODataPayloadKind[] { ODataPayloadKind.Feed }, GetSampleRequest());
            formatter.SupportedMediaTypes.Add(ODataMediaTypes.ApplicationJsonODataMinimalMetadata);
            formatter.SupportedMediaTypes.Add(ODataMediaTypes.ApplicationAtomXmlTypeFeed);
            return formatter;
        }

        private HttpRequestMessage GetSampleRequest()
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/employees");
            HttpConfiguration config = new HttpConfiguration();
            config.MapODataRoute(GetSampleModel());
            request.Properties[HttpPropertyKeys.HttpConfigurationKey] = config;
            request.Properties[HttpPropertyKeys.HttpRouteDataKey] = new HttpRouteData(new HttpRoute());
            IEdmEntitySet entitySet = _model.EntityContainers().Single().FindEntitySet("employees");
            request.SetEdmModel(_model);
            request.SetODataPath(new ODataPath(new EntitySetPathSegment(entitySet)));
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
