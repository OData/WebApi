// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Web.Http.OData.TestCommon;
using System.Web.Http.OData.TestCommon.Models;
using Microsoft.TestCommon;
using Xunit;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Web.Http.OData.Formatter.Serialization
{
    public class FeedTest
    {
        [Fact(Skip = "Requires inheritance support in the odata formatter")]
        [Trait("Description", "ODataMediaTypeFormatter serailizes a feed in valid ODataMessageFormat")]
        public void IEnumerableOfEntityTypeSerializesAsODataFeed()
        {
            ODataMediaTypeFormatter formatter = new ODataMediaTypeFormatter();

            IEnumerable<Employee> collectionOfPerson = new Collection<Employee>() 
            {
                (Employee)TypeInitializer.GetInstance(SupportedTypes.Employee, 0),
                (Employee)TypeInitializer.GetInstance(SupportedTypes.Employee, 1),
            };

            ObjectContent<IEnumerable<Employee>> content = new ObjectContent<IEnumerable<Employee>>(collectionOfPerson, formatter);

            RegexReplacement replaceUpdateTime = new RegexReplacement("<updated>*.*</updated>", "<updated>UpdatedTime</updated>");
            Assert.Xml.Equal(BaselineResource.TestFeedOfEmployee, content.ReadAsStringAsync().Result, regexReplacements: replaceUpdateTime);
        }

        [Fact(Skip = "Requires inheritance support in the odata formatter")]
        [Trait("Description", "ODataMediaTypeFormatter sets required headers for a feed when serialized as ATOM.")]
        public void ContentHeadersAreAddedForXmlMediaType()
        {
            ODataMediaTypeFormatter formatter = new ODataMediaTypeFormatter();

            ObjectContent<IEnumerable<Employee>> content = new ObjectContent<IEnumerable<Employee>>(new Employee[] { new Employee(0, new ReferenceDepthContext(7)) }, formatter);
            content.LoadIntoBufferAsync().Wait();

            Assert.Http.Contains(content.Headers, "DataServiceVersion", "3.0;");
            Assert.Http.Contains(content.Headers, "Content-Type", "application/atom+xml; type=feed");
        }

        [Fact(Skip = "Requires inheritance support in the odata formatter")]
        [Trait("Description", "ODataMediaTypeFormatter sets required headers for a feed when serialized as JSON.")]
        public void ContentHeadersAreAddedForJsonMediaType()
        {
            ODataMediaTypeFormatter formatter = new ODataMediaTypeFormatter();

            HttpContent content = new ObjectContent<IEnumerable<Employee>>(new Employee[] { new Employee(0, new ReferenceDepthContext(7)) }, formatter, "application/json");
            content.LoadIntoBufferAsync().Wait();

            Assert.Http.Contains(content.Headers, "DataServiceVersion", "3.0;");
            Assert.Equal(content.Headers.ContentType.MediaType, "application/json");
        }
    }
}
