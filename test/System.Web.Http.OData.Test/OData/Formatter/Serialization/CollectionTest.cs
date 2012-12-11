// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Web.Http.Hosting;
using System.Web.Http.OData.Builder;
using System.Web.Http.OData.Routing;
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
            _formatter = new ODataMediaTypeFormatter(GetSampleModel(),
                new ODataPayloadKind[] { ODataPayloadKind.Collection }, GetSampleRequest());
            _formatter.SupportedMediaTypes.Add(ODataMediaTypes.ApplicationJsonODataMinimalMetadata);
        }

        /// <summary>
        /// Arrays the of ints serializes as O data.
        /// </summary>
        [Fact]
        public void ArrayOfIntsSerializesAsOData()
        {
            ObjectContent<int[]> content = new ObjectContent<int[]>(new int[] { 10, 20, 30, 40, 50 }, _formatter);

            JsonAssert.Equal(BaselineResource.ArrayOfInt32InJsonLight, content.ReadAsStringAsync().Result);
        }

        [Fact]
        public void ArrayOfBoolsSerializesAsOData()
        {
            ObjectContent<bool[]> content = new ObjectContent<bool[]>(new bool[] { true, false, true, false }, _formatter);

            JsonAssert.Equal(BaselineResource.ArrayOfBooleanInJsonLight, content.ReadAsStringAsync().Result);
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

            JsonAssert.Equal(BaselineResource.ListOfStringInJsonLight, content.ReadAsStringAsync().Result);
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

            JsonAssert.Equal(BaselineResource.CollectionOfPersonInJsonLight, content.ReadAsStringAsync().Result);
        }

        private static HttpRequestMessage GetSampleRequest()
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/property");
            HttpConfiguration config = new HttpConfiguration();
            config.AddFakeODataRoute();
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
