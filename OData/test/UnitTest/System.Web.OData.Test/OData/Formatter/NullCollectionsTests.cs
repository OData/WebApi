// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.OData.Builder;
using System.Web.OData.Builder.TestModels;
using System.Web.OData.Extensions;
using Microsoft.OData.Edm;
using Microsoft.TestCommon;
using Newtonsoft.Json.Linq;

namespace System.Web.OData.Formatter
{
    public class NullCollectionsTests
    {
        private HttpClient _client;
        private HttpConfiguration _config;

        public enum NullCollectionsTestMode
        {
            NormalPass_FromEntity,
            NormalFail_FromEntity,
            DoNotSerialize_FromEntity,
            SerializeAsEmpty_FromEntity,
            NormalPass_FromParentComplex,
            NormalFail_FromParentComplex,
            DoNotSerialize_FromParentComplex,
            SerializeAsEmpty_FromParentComplex,
        }

        public NullCollectionsTests()
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<NullCollectionsTestsModel>("NullCollectionsTests");
            builder.EntitySet<Vehicle>("vehicles");
            IEdmModel model = builder.GetEdmModel();

            _config = new[] { typeof(NullCollectionsTestsController) }.GetHttpConfiguration();
            _config.Formatters.Clear();
            _config.Formatters.AddRange(ODataMediaTypeFormatters.Create());
            _config.MapODataServiceRoute(model);

            HttpServer server = new HttpServer(_config);
            _client = new HttpClient(server);
        }

        [Theory]
        // -- "NormalPass_FromEntity" -- ensure existing behavior has not changed when serializing 
        //                               properties off the base entity
        [InlineData("Array", NullCollectionsTestMode.NormalPass_FromEntity)]
        [InlineData("IEnumerable", NullCollectionsTestMode.NormalPass_FromEntity)]
        [InlineData("ICollection", NullCollectionsTestMode.NormalPass_FromEntity)]
        [InlineData("IList", NullCollectionsTestMode.NormalPass_FromEntity)]
        [InlineData("List", NullCollectionsTestMode.NormalPass_FromEntity)]
        [InlineData("Collection", NullCollectionsTestMode.NormalPass_FromEntity)]
        [InlineData("CustomCollection", NullCollectionsTestMode.NormalPass_FromEntity)]
        [InlineData("NullableColors", NullCollectionsTestMode.NormalPass_FromEntity)]
        [InlineData("ComplexCollection", NullCollectionsTestMode.NormalPass_FromEntity)]
        public void NullCollectionProperties_NormalPass_FromEntity(string propertyName, NullCollectionsTestMode testMode)
        {
            // Arrange
            NullCollectionsTestsController.TestObject = new NullCollectionsTestsModel();

            // Act
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/NullCollectionsTests/");
            HttpResponseMessage response = _client.SendAsync(request).Result;

            // Assert
            response.EnsureSuccessStatusCode();
            string responseJson = response.Content.ReadAsStringAsync().Result;
            dynamic result = JToken.Parse(responseJson);
            if (propertyName == "NullableColors")
            {
                Assert.Equal(new[] {Color.Red.ToString()}, (IEnumerable<string>) result[propertyName]
                    .Values<string>());
            }
            else if (propertyName == "ComplexCollection")
            {
                Assert.Equal(new[] {42}, ((IEnumerable<JObject>) result[propertyName].Values<JObject>())
                    .Select(v => (int) v.Property("A")));
            }
            else
            {
                Assert.Equal(new[] {42}, (IEnumerable<int>) result[propertyName].Values<int>());
            }
        }

        [Theory]
        // -- "SerializeAsEmpty_FromEntity" -- ensure null collections are serialized as if they were 
        //                                     empty collections when serializing properties off the base entity
        [InlineData("Array", NullCollectionsTestMode.SerializeAsEmpty_FromEntity)]
        [InlineData("IEnumerable", NullCollectionsTestMode.SerializeAsEmpty_FromEntity)]
        [InlineData("ICollection", NullCollectionsTestMode.SerializeAsEmpty_FromEntity)]
        [InlineData("IList", NullCollectionsTestMode.SerializeAsEmpty_FromEntity)]
        [InlineData("List", NullCollectionsTestMode.SerializeAsEmpty_FromEntity)]
        [InlineData("Collection", NullCollectionsTestMode.SerializeAsEmpty_FromEntity)]
        [InlineData("CustomCollection", NullCollectionsTestMode.SerializeAsEmpty_FromEntity)]
        [InlineData("NullableColors", NullCollectionsTestMode.SerializeAsEmpty_FromEntity)]
        [InlineData("ComplexCollection", NullCollectionsTestMode.SerializeAsEmpty_FromEntity)]
        public void NullCollectionProperties_SerializeAsEmpty_FromEntity(string propertyName, NullCollectionsTestMode testMode)
        {
            // Arrange
            NullCollectionsTestsModel testObject = new NullCollectionsTestsModel();
            testObject.GetType().GetProperty(propertyName).SetValue(testObject, null);
            NullCollectionsTestsController.TestObject = testObject;

            // Act
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/NullCollectionsTests/");
            HttpResponseMessage response = _client.SendAsync(request).Result;

            // Assert
            response.EnsureSuccessStatusCode();
            string responseJson = response.Content.ReadAsStringAsync().Result;
            dynamic result = JToken.Parse(responseJson);
            Assert.NotNull(result[propertyName]);
            IEnumerable<JObject> collection = result[propertyName].Values<JObject>();
            Assert.Empty(collection);
        }

        [Theory]
        // -- "NormalPass_FromParentComplex" -- ensure existing behavior has not changed when serializing 
        //                                      properties off a complex attached to the entity
        [InlineData("Array", NullCollectionsTestMode.NormalPass_FromParentComplex)]
        [InlineData("IEnumerable", NullCollectionsTestMode.NormalPass_FromParentComplex)]
        [InlineData("ICollection", NullCollectionsTestMode.NormalPass_FromParentComplex)]
        [InlineData("IList", NullCollectionsTestMode.NormalPass_FromParentComplex)]
        [InlineData("List", NullCollectionsTestMode.NormalPass_FromParentComplex)]
        [InlineData("Collection", NullCollectionsTestMode.NormalPass_FromParentComplex)]
        [InlineData("CustomCollection", NullCollectionsTestMode.NormalPass_FromParentComplex)]
        [InlineData("NullableColors", NullCollectionsTestMode.NormalPass_FromParentComplex)]
        [InlineData("ComplexCollection", NullCollectionsTestMode.NormalPass_FromParentComplex)]
        public void NullCollectionProperties_NormalPass_FromParentComplex(string propertyName, NullCollectionsTestMode testMode)
        {
            // Arrange
            NullCollectionsTestsController.TestObject = new NullCollectionsTestsModel();

            // Act
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/NullCollectionsTests/");
            HttpResponseMessage response = _client.SendAsync(request).Result;

            // Assert
            response.EnsureSuccessStatusCode();
            string responseJson = response.Content.ReadAsStringAsync().Result;
            dynamic result = JToken.Parse(responseJson);
            var parent = result["ParentComplex"];
            if (propertyName == "NullableColors")
            {
                Assert.Equal(new[] {Color.Red.ToString()}, (IEnumerable<string>) parent[propertyName]
                    .Values<string>());
            }
            else if (propertyName == "ComplexCollection")
            {
                Assert.Equal(new[] {42}, ((IEnumerable<JObject>) parent[propertyName].Values<JObject>())
                    .Select(v => (int) v.Property("A")));
            }
            else
            {
                Assert.Equal(new[] {42}, (IEnumerable<int>) parent[propertyName].Values<int>());
            }
        }

        [Theory]
        // -- "SerializeAsEmpty_FromParentComplex" -- ensure null collections are serialized as if they were empty 
        //                                            collections when serializing properties off a complex attached 
        //                                            to the entity
        [InlineData("Array", NullCollectionsTestMode.SerializeAsEmpty_FromParentComplex)]
        [InlineData("IEnumerable", NullCollectionsTestMode.SerializeAsEmpty_FromParentComplex)]
        [InlineData("ICollection", NullCollectionsTestMode.SerializeAsEmpty_FromParentComplex)]
        [InlineData("IList", NullCollectionsTestMode.SerializeAsEmpty_FromParentComplex)]
        [InlineData("List", NullCollectionsTestMode.SerializeAsEmpty_FromParentComplex)]
        [InlineData("Collection", NullCollectionsTestMode.SerializeAsEmpty_FromParentComplex)]
        [InlineData("CustomCollection", NullCollectionsTestMode.SerializeAsEmpty_FromParentComplex)]
        [InlineData("NullableColors", NullCollectionsTestMode.SerializeAsEmpty_FromParentComplex)]
        [InlineData("ComplexCollection", NullCollectionsTestMode.SerializeAsEmpty_FromParentComplex)]
        public void NullCollectionProperties_SerializeAsEmpty_FromParentComplex(string propertyName, NullCollectionsTestMode testMode)
        {
            // Arrange
            NullCollectionsTestsModel testObject = new NullCollectionsTestsModel();
            testObject.ParentComplex.GetType().GetProperty(propertyName).SetValue(testObject.ParentComplex, null);
            NullCollectionsTestsController.TestObject = testObject;

            // Act
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/NullCollectionsTests/");
            HttpResponseMessage response = _client.SendAsync(request).Result;

            // Assert
            response.EnsureSuccessStatusCode();
            string responseJson = response.Content.ReadAsStringAsync().Result;
            dynamic result = JToken.Parse(responseJson);
            var parent3 = result["ParentComplex"];
            Assert.NotNull(parent3[propertyName]);
            IEnumerable<JObject> collection2 = parent3[propertyName].Values<JObject>();
            Assert.Empty(collection2);
        }
    }

    public class NullCollectionsTestsController : ODataController
    {
        internal static NullCollectionsTestsModel TestObject { get; set; }

        public NullCollectionsTestsModel Get()
        {
            if (!ModelState.IsValid)
            {
                throw new HttpResponseException(HttpStatusCode.ExpectationFailed);
            }

            return TestObject;
        }
    }

    public class NullCollectionsTestsModel
    {
        public NullCollectionsTestsModel()
        {
            Array = new int[] { 42 };
            IEnumerable = new int[] { 42 };
            ICollection = new int[] { 42 };
            IList = new int[] { 42 };
            List = new List<int> { 42 };
            Collection = new Collection<int> { 42 };
            ComplexCollection = new NullComplexChild[] { new NullComplexChild { A = 42 } };
            ParentComplex = new NullComplexParent();
            CustomCollection = new CustomCollection_NullCollectionsTestsModel<int> { 42 };
            NullableColors = new Color?[] { Color.Red };
        }

        public int ID { get; set; }

        public int[] Array { get; set; }

        public IEnumerable<int> IEnumerable { get; set; }

        public IList<int> IList { get; set; }

        public ICollection<int> ICollection { get; set; }

        public Collection<int> Collection { get; set; }

        public List<int> List { get; set; }

        public IEnumerable<NullComplexChild> ComplexCollection { get; set; }

        public NullComplexParent ParentComplex { get; set; }

        public CustomCollection_NullCollectionsTestsModel<int> CustomCollection { get; set; }

        public IEnumerable<Color?> NullableColors { get; set; }
    }

    public class NullComplexChild
    {
        public int A { get; set; }
    }

    // we re-test all of the above properties on the entity (except ourself but including a different child complex 
    // collection) because the serializer handles each parent type (including entity vs complex) through different 
    // code paths
    public class NullComplexParent
    {
        public NullComplexParent()
        {
            Array = new int[] { 42 };
            IEnumerable = new int[] { 42 };
            ICollection = new int[] { 42 };
            IList = new int[] { 42 };
            List = new List<int> { 42 };
            Collection = new Collection<int> { 42 };
            ComplexCollection = new NullComplexChild[] { new NullComplexChild { A = 42 } };
            CustomCollection = new CustomCollection_NullCollectionsTestsModel<int> { 42 };
            NullableColors = new Color?[] { Color.Red };
        }

        public int[] Array { get; set; }

        public IEnumerable<int> IEnumerable { get; set; }

        public IList<int> IList { get; set; }

        public ICollection<int> ICollection { get; set; }

        public Collection<int> Collection { get; set; }

        public List<int> List { get; set; }

        public IEnumerable<NullComplexChild> ComplexCollection { get; set; }

        public CustomCollection_NullCollectionsTestsModel<int> CustomCollection { get; set; }

        public IEnumerable<Color?> NullableColors { get; set; }
    }

    public class CustomCollection_NullCollectionsTestsModel<T> : List<T>
    {
    }
}
