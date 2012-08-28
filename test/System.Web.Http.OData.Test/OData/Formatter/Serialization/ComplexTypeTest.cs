// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Net.Http;
using System.Web.Http.OData.Builder;
using System.Web.Http.OData.TestCommon.Models;
using Microsoft.TestCommon;

namespace System.Web.Http.OData.Formatter.Serialization
{
    public class ComplexTypeTest
    {
        ODataMediaTypeFormatter _formatter;

        public ComplexTypeTest()
        {
            ODataModelBuilder model = new ODataModelBuilder();
            var person = model.ComplexType<Person>();
            person.Property(p => p.Age);
            person.Property(p => p.FirstName);
            person.ComplexProperty(p => p.FavoriteHobby);
            person.ComplexProperty(p => p.Gender);

            _formatter = new ODataMediaTypeFormatter(model.GetEdmModel());
        }

        [Fact(Skip = "Requires new functionality in the odata formatter")]
        [Trait("Description", "ODataMediaTypeFormatter writes out complex  types in valid ODataMessageFormat")]
        public void ComplexTypeSerializesAsOData()
        {
            ObjectContent<Person> content = new ObjectContent<Person>(new Person(0, new ReferenceDepthContext(7)), _formatter);

            Assert.Xml.Equal(BaselineResource.TestComplexTypePerson, content.ReadAsStringAsync().Result);
        }

        [Fact(Skip = "Requires new functionality in the odata formatter")]
        [Trait("Description", "ODataMediaTypeFormatter sets required headers for a complex type when serialized as XML.")]
        public void ContentHeadersAreAddedForXmlMediaType()
        {
            ObjectContent<Person> content = new ObjectContent<Person>(new Person(0, new ReferenceDepthContext(7)), _formatter);
            content.LoadIntoBufferAsync().Wait();

            Assert.Http.Contains(content.Headers, "DataServiceVersion", "3.0;");
            Assert.Http.Contains(content.Headers, "Content-Type", "application/xml");
        }

        [Fact(Skip = "Requires new functionality in the odata formatter")]
        [Trait("Description", "ODataMediaTypeFormatter sets required headers for a complex type when serialized as JSON.")]
        public void ContentHeadersAreAddedForJsonMediaType()
        {
            HttpContent content = new ObjectContent<Person>(new Person(0, new ReferenceDepthContext(7)), _formatter, "application/json");
            content.LoadIntoBufferAsync().Wait();

            Assert.Http.Contains(content.Headers, "DataServiceVersion", "3.0;");
            Assert.Equal(content.Headers.ContentType.MediaType, "application/json");
        }
    }
}
