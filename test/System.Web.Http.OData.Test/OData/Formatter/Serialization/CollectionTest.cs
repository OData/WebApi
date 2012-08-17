// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Web.Http.OData.TestCommon;
using System.Web.Http.OData.TestCommon.Models;
using Microsoft.Data.OData;
using Xunit;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Web.Http.OData.Formatter.Serialization
{
    public class CollectionTest
    {
        ODataMediaTypeFormatter _formatter = new ODataMediaTypeFormatter() { IsClient = true };

        /// <summary>
        /// Arrays the of ints serializes as O data.
        /// </summary>
        [Fact(Skip = "Requires new functionality in the odata formatter")]
        [Trait("Description", "ODataMediaTypeFormatter writes out array of ints in valid ODataMessageFormat")]
        public void ArrayOfIntsSerializesAsOData()
        {
            ObjectContent<int[]> content = new ObjectContent<int[]>(new int[] { 10, 20, 30, 40, 50 }, _formatter);

            Assert.Xml.Equal(BaselineResource.TestArrayOfInts, content.ReadAsStringAsync().Result);
        }

        [Fact(Skip = "Requires new functionality in the odata formatter")]
        [Trait("Description", "ODataMediaTypeFormatter writes out array of bool in valid ODataMessageFormat")]
        public void ArrayOfBoolsSerializesAsOData()
        {
            ObjectContent<bool[]> content = new ObjectContent<bool[]>(new bool[] { true, false, true, false }, _formatter);

            Assert.Xml.Equal(BaselineResource.TestArrayOfBools, content.ReadAsStringAsync().Result);
        }

        [Fact(Skip = "Requires new functionality in the odata formatter")]
        [Trait("Description", "ODataMediaTypeFormatter writes out List of strings in valid ODataMessageFormat")]
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

        [Fact(Skip = "Requires new functionality in the odata formatter")]
        [Trait("Description", "ODataMediaTypeFormatter throws while writing out non-homogenous collection of objects")]
        public void CollectionOfObjectsSerializesAsOData()
        {
            Collection<object> collectionOfObjects = new Collection<object>();
            collectionOfObjects.Add(1);
            collectionOfObjects.Add("Frank");
            collectionOfObjects.Add(TypeInitializer.GetInstance(SupportedTypes.Person, 2));
            collectionOfObjects.Add(TypeInitializer.GetInstance(SupportedTypes.Employee, 3));

            ObjectContent<Collection<object>> content = new ObjectContent<Collection<object>>(collectionOfObjects, _formatter);

            Assert.Throws<ODataException>(() => content.ReadAsStringAsync().Result);
        }

        [Fact(Skip = "Requires new functionality in the odata formatter")]
        [Trait("Description", "ODataMediaTypeFormatter writes out Collection of complex types in valid ODataMessageFormat")]
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

        [Fact(Skip = "Requires new functionality in the odata formatter")]
        [Trait("Description", "ODataMediaTypeFormatter writes out Dictionary type in valid ODataMessageFormat")]
        public void DictionarySerializesAsOData()
        {
            Dictionary<int, string> dictionary = new Dictionary<int, string>();
            dictionary.Add(1, "Frank");
            dictionary.Add(2, "Steve");
            dictionary.Add(3, "Tom");
            dictionary.Add(4, "Chandler");

            ObjectContent<Dictionary<int, string>> content = new ObjectContent<Dictionary<int, string>>(dictionary, _formatter);

            Assert.Xml.Equal(BaselineResource.TestDictionary, content.ReadAsStringAsync().Result);
        }

        [Fact(Skip = "Requires new functionality in the odata formatter")]
        [Trait("Description", "ODataMediaTypeFormatter sets required headers for a complex type when serialized as XML.")]
        public void ContentHeadersAreAddedForXmlMediaType()
        {
            ObjectContent<IEnumerable<Person>> content = new ObjectContent<IEnumerable<Person>>(new Person[] { new Person(0, new ReferenceDepthContext(7)) }, _formatter);
            content.LoadIntoBufferAsync().Wait();

            Assert.Http.Contains(content.Headers, "DataServiceVersion", "3.0;");
            Assert.Http.Contains(content.Headers, "Content-Type", "application/xml");
        }

        [Fact(Skip = "Requires new functionality in the odata formatter")]
        [Trait("Description", "ODataMediaTypeFormatter sets required headers for a complex type when serialized as JSON.")]
        public void ContentHeadersAreAddedForJsonMediaType()
        {
            HttpContent content = new ObjectContent<Person[]>(new Person[] { new Person(0, new ReferenceDepthContext(7)) }, _formatter, "application/json");
            content.LoadIntoBufferAsync().Wait();

            Assert.Http.Contains(content.Headers, "DataServiceVersion", "3.0;");
            Assert.Equal(content.Headers.ContentType.MediaType, "application/json");
        }
    }
}
