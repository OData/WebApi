// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Web.Http.Hosting;
using System.Web.Http.OData.Builder;
using System.Web.Http.OData.TestCommon.Models;
using Microsoft.Data.Edm;
using Microsoft.Data.OData;
using Microsoft.TestCommon;

namespace System.Web.Http.OData.Formatter.Serialization
{
    public class CollectionTest
    {
        private readonly ODataMediaTypeFormatter _formatter;

        public CollectionTest()
        {
            _formatter = new ODataMediaTypeFormatter(GetSampleModel(), GetSampleRequest());
        }

        /// <summary>
        /// Arrays the of ints serializes as O data.
        /// </summary>
        [Fact]
        public void ArrayOfIntsSerializesAsOData()
        {
            ObjectContent<int[]> content = new ObjectContent<int[]>(new int[] { 10, 20, 30, 40, 50 }, _formatter);

            Assert.Xml.Equal(BaselineResource.TestArrayOfInts, content.ReadAsStringAsync().Result);
        }

        [Fact]
        public void ArrayOfBoolsSerializesAsOData()
        {
            ObjectContent<bool[]> content = new ObjectContent<bool[]>(new bool[] { true, false, true, false }, _formatter);

            Assert.Xml.Equal(BaselineResource.TestArrayOfBools, content.ReadAsStringAsync().Result);
        }

        [Fact]
        public void ListOfStringsSerializesAsOData()
        {
            List<string> listOfStrings = new List<string>();
            listOfStrings.Add("Frank");
            listOfStrings.Add("Steve");
            listOfStrings.Add("Tom");
            listOfStrings.Add("Chandler");

            ObjectContent<List<string>> content = new ObjectContent<List<string>>(listOfStrings, _formatter);

            Assert.Xml.Equal(BaselineResource.TestListOfStrings, content.ReadAsStringAsync().Result);
        }

        [Fact]
        public void CollectionOfComplexTypeSerializesAsOData()
        {
            IEnumerable<Person> collectionOfPerson = new Collection<Person>() 
            {
                (Person)TypeInitializer.GetInstance(SupportedTypes.Person, 0),
                (Person)TypeInitializer.GetInstance(SupportedTypes.Person, 1),
                (Person)TypeInitializer.GetInstance(SupportedTypes.Person, 2)
            };

            ObjectContent<IEnumerable<Person>> content = new ObjectContent<IEnumerable<Person>>(collectionOfPerson, _formatter);

            Assert.Xml.Equal(BaselineResource.TestCollectionOfPerson, content.ReadAsStringAsync().Result);
        }

        [Fact]
        [Trait("Description", "ODataMediaTypeFormatter sets required headers for a complex type when serialized as XML.")]
        public void ContentHeadersAreAddedForXmlMediaType()
        {
            ObjectContent<IEnumerable<Person>> content = new ObjectContent<IEnumerable<Person>>(new Person[] { new Person(0, new ReferenceDepthContext(7)) }, _formatter);
            content.LoadIntoBufferAsync().Wait();

            Assert.Http.Contains(content.Headers, "DataServiceVersion", "3.0;");
            Assert.Http.Contains(content.Headers, "Content-Type", "application/xml; charset=utf-8");
        }

        [Fact]
        public void ContentHeadersAreAddedForJsonMediaType()
        {
            HttpContent content = new ObjectContent<Person[]>(new Person[] { new Person(0, new ReferenceDepthContext(7)) }, _formatter, "application/json");
            content.LoadIntoBufferAsync().Wait();

            Assert.Http.Contains(content.Headers, "DataServiceVersion", "3.0;");
            Assert.Equal(content.Headers.ContentType.MediaType, "application/json");
        }

        private static HttpRequestMessage GetSampleRequest()
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/property");
            HttpConfiguration config = new HttpConfiguration();
            request.Properties[HttpPropertyKeys.HttpConfigurationKey] = config;
            return request;
        }

        private static IEdmModel GetSampleModel()
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.ComplexType<Person>();
            return builder.GetEdmModel();
        }
    }
}
