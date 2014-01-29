// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http.OData.Builder;
using System.Web.Http.OData.Extensions;
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
            _formatter = new ODataMediaTypeFormatter(new ODataPayloadKind[] { ODataPayloadKind.Collection });
            _formatter.Request = GetSampleRequest();
            _formatter.SupportedMediaTypes.Add(ODataMediaTypes.ApplicationJsonODataMinimalMetadata);
            _formatter.SupportedMediaTypes.Add(ODataMediaTypes.ApplicationXml);
        }

        [Fact]
        public void ArrayOfIntsSerializesAsODataForJsonLight()
        {
            ArrayOfIntsSerializesAsOData(Resources.ArrayOfInt32InJsonLight, true);
        }

        [Fact]
        public void ArrayOfIntsSerializesAsODataForAtom()
        {
            ArrayOfIntsSerializesAsOData(Resources.ArrayOfInt32InAtom, false);
        }

        private void ArrayOfIntsSerializesAsOData(string expectedContent, bool json)
        {
            ObjectContent<int[]> content = new ObjectContent<int[]>(new int[] { 10, 20, 30, 40, 50 }, _formatter,
                GetMediaType(json));

            AssertEqual(json, expectedContent, content.ReadAsStringAsync().Result);
        }

        [Fact]
        public void ArrayOfBoolsSerializesAsODataForJsonLight()
        {
            ArrayOfBoolsSerializesAsOData(Resources.ArrayOfBooleanInJsonLight, true);
        }

        [Fact]
        public void ArrayOfBoolsSerializesAsODataForAtom()
        {
            ArrayOfBoolsSerializesAsOData(Resources.ArrayOfBooleanInAtom, false);
        }

        private void ArrayOfBoolsSerializesAsOData(string expectedContent, bool json)
        {
            ObjectContent<bool[]> content = new ObjectContent<bool[]>(new bool[] { true, false, true, false },
                _formatter, GetMediaType(json));

            AssertEqual(json, expectedContent, content.ReadAsStringAsync().Result);
        }

        [Fact]
        public void ListOfStringsSerializesAsODataForJsonLight()
        {
            ListOfStringsSerializesAsOData(Resources.ListOfStringInJsonLight, true);
        }

        [Fact]
        public void ListOfStringsSerializesAsODataForAtom()
        {
            ListOfStringsSerializesAsOData(Resources.ListOfStringInAtom, false);
        }

        private void ListOfStringsSerializesAsOData(string expectedContent, bool json)
        {
            List<string> listOfStrings = new List<string>();
            listOfStrings.Add("Frank");
            listOfStrings.Add("Steve");
            listOfStrings.Add("Tom");
            listOfStrings.Add("Chandler");

            ObjectContent<List<string>> content = new ObjectContent<List<string>>(listOfStrings, _formatter,
                GetMediaType(json));

            AssertEqual(json, expectedContent, content.ReadAsStringAsync().Result);
        }

        [Fact]
        public void CollectionOfComplexTypeSerializesAsODataForJsonLight()
        {
            CollectionOfComplexTypeSerializesAsOData(Resources.CollectionOfPersonInJsonLight, true);
        }

        [Fact]
        public void CollectionOfComplexTypeSerializesAsODataForAtom()
        {
            CollectionOfComplexTypeSerializesAsOData(Resources.CollectionOfPersonInAtom, false);
        }

        private void CollectionOfComplexTypeSerializesAsOData(string expectedContent, bool json)
        {
            IEnumerable<Person> collectionOfPerson = new Collection<Person>() 
            {
                (Person)TypeInitializer.GetInstance(SupportedTypes.Person, 0),
                (Person)TypeInitializer.GetInstance(SupportedTypes.Person, 1),
                (Person)TypeInitializer.GetInstance(SupportedTypes.Person, 2)
            };

            ObjectContent<IEnumerable<Person>> content = new ObjectContent<IEnumerable<Person>>(collectionOfPerson,
                _formatter, GetMediaType(json));

            AssertEqual(json, expectedContent, content.ReadAsStringAsync().Result);
        }

        internal static void AssertEqual(bool json, string expected, string actual)
        {
            if (json)
            {
                JsonAssert.Equal(expected, actual);
            }
            else
            {
                Assert.Xml.Equal(expected, actual);
            }
        }

        internal static MediaTypeHeaderValue GetMediaType(bool json)
        {
            return json ? ODataMediaTypes.ApplicationJsonODataMinimalMetadata : ODataMediaTypes.ApplicationXml;
        }

        private static HttpRequestMessage GetSampleRequest()
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/property");
            request.ODataProperties().Model = GetSampleModel();
            HttpConfiguration configuration = new HttpConfiguration();
            configuration.Routes.MapFakeODataRoute();
            request.SetConfiguration(configuration);
            request.SetFakeODataRouteName();
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
