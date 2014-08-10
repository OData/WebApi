// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Web.Http;
using System.Web.OData.Builder;
using System.Web.OData.Extensions;
using System.Web.OData.TestCommon.Models;
using Microsoft.OData.Core;
using Microsoft.OData.Edm;
using Microsoft.TestCommon;

namespace System.Web.OData.Formatter.Serialization
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
        public void ArrayOfIntsSerializesAsOData()
        {
            // Arrange
            ObjectContent<int[]> content = new ObjectContent<int[]>(new int[] { 10, 20, 30, 40, 50 }, _formatter,
                ODataMediaTypes.ApplicationJsonODataMinimalMetadata);

            // Act & Assert
            JsonAssert.Equal(Resources.ArrayOfInt32, content.ReadAsStringAsync().Result);
        }

        [Fact]
        public void ArrayOfBooleansSerializesAsOData()
        {
            // Arrange
            ObjectContent<bool[]> content = new ObjectContent<bool[]>(new bool[] { true, false, true, false },
                _formatter, ODataMediaTypes.ApplicationJsonODataMinimalMetadata);

            // Act & Assert
            JsonAssert.Equal(Resources.ArrayOfBoolean, content.ReadAsStringAsync().Result);
        }

        [Fact]
        public void ListOfStringsSerializesAsOData()
        {
            // Arrange
            List<string> listOfStrings = new List<string>();
            listOfStrings.Add("Frank");
            listOfStrings.Add("Steve");
            listOfStrings.Add("Tom");
            listOfStrings.Add("Chandler");

            ObjectContent<List<string>> content = new ObjectContent<List<string>>(listOfStrings, _formatter,
                ODataMediaTypes.ApplicationJsonODataMinimalMetadata);

            // Act & Assert
            JsonAssert.Equal(Resources.ListOfString, content.ReadAsStringAsync().Result);
        }

        [Fact]
        public void CollectionOfComplexTypeSerializesAsOData()
        {
            // Arrange
            IEnumerable<Person> collectionOfPerson = new Collection<Person>() 
            {
                (Person)TypeInitializer.GetInstance(SupportedTypes.Person, 0),
                (Person)TypeInitializer.GetInstance(SupportedTypes.Person, 1),
                (Person)TypeInitializer.GetInstance(SupportedTypes.Person, 2)
            };

            ObjectContent<IEnumerable<Person>> content = new ObjectContent<IEnumerable<Person>>(collectionOfPerson,
                _formatter, ODataMediaTypes.ApplicationJsonODataMinimalMetadata);

            // Act & Assert
            JsonAssert.Equal(Resources.CollectionOfPerson, content.ReadAsStringAsync().Result);
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

            // Employee is derived from Person. Employee has a property named manager it's Employee type.
            // It's not allowed to build inheritance complex type because a recursive loop of complex types is not allowed.
            builder.Ignore<Employee>();
            return builder.GetEdmModel();
        }
    }
}
