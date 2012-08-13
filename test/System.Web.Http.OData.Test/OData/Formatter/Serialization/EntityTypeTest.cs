// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Net.Http;
using System.Web.Http.OData.TestCommon;
using System.Web.Http.OData.TestCommon.Models;
using Microsoft.TestCommon;
using Xunit;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Web.Http.OData.Formatter.Serialization
{
    public class EntityTypeTest
    {
        [Fact(Skip = "Requires inheritance support in the odata formatter")]
        [Trait("Description", "ODataMediaTypeFormatter serailizes an entity type in valid ODataMessageFormat")]
        public void EntityTypeSerializesAsODataEntry()
        {
            ODataMediaTypeFormatter formatter = new ODataMediaTypeFormatter();

            Employee employee = (Employee)TypeInitializer.GetInstance(SupportedTypes.Employee);
            ObjectContent<Employee> content = new ObjectContent<Employee>(employee, formatter);

            RegexReplacement replaceUpdateTime = new RegexReplacement("<updated>*.*</updated>", "<updated>UpdatedTime</updated>");
            Assert.Xml.Equal(BaselineResource.TestEntityTypeBasic, content.ReadAsStringAsync().Result, regexReplacements: replaceUpdateTime);
        }

        [Fact(Skip = "Requires inheritance support in the odata formatter")]
        [Trait("Description", "ODataMediaTypeFormatter serailizes an entity type with multiple keys in valid ODataMessageFormat")]
        public void EntityTypeWithMultipleKeysSerializesAsODataEntry()
        {
            ODataMediaTypeFormatter formatter = new ODataMediaTypeFormatter();

            MultipleKeyEmployee multipleKeyEmployee = (MultipleKeyEmployee)TypeInitializer.GetInstance(SupportedTypes.MultipleKeyEmployee);
            ObjectContent<MultipleKeyEmployee> content = new ObjectContent<MultipleKeyEmployee>(multipleKeyEmployee, formatter);

            RegexReplacement replaceUpdateTime = new RegexReplacement("<updated>*.*</updated>", "<updated>UpdatedTime</updated>");
            Assert.Xml.Equal(BaselineResource.TestEntityTypeWithMultipleKeys, content.ReadAsStringAsync().Result, regexReplacements: replaceUpdateTime);
        }

        [Fact(Skip = "Requires inheritance support in the odata formatter")]
        [Trait("Description", "ODataMediaTypeFormatter sets required headers for an entity type when serialized as XML.")]
        public void ContentHeadersAreAddedForXmlMediaType()
        {
            ODataMediaTypeFormatter formatter = new ODataMediaTypeFormatter();

            ObjectContent<Employee> content = new ObjectContent<Employee>(new Employee(0, new ReferenceDepthContext(7)), formatter);
            content.LoadIntoBufferAsync().Wait();

            Assert.Http.Contains(content.Headers, "DataServiceVersion", "3.0;");
            Assert.Http.Contains(content.Headers, "Content-Type", "application/atom+xml; type=entry");
        }

        [Fact(Skip = "Requires inheritance support in the odata formatter")]
        [Trait("Description", "ODataMediaTypeFormatter sets required headers for an entity type when serialized as JSON.")]
        public void ContentHeadersAreAddedForJsonMediaType()
        {
            ODataMediaTypeFormatter formatter = new ODataMediaTypeFormatter();

            HttpContent content = new ObjectContent<Employee>(new Employee(0, new ReferenceDepthContext(7)), formatter, "application/json");
            content.LoadIntoBufferAsync().Wait();

            Assert.Http.Contains(content.Headers, "DataServiceVersion", "3.0;");
            Assert.Equal(content.Headers.ContentType.MediaType, "application/json");
        }
    }
}
