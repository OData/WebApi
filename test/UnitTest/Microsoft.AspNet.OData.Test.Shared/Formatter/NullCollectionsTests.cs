//-----------------------------------------------------------------------------
// <copyright file="NullCollectionsTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

#if NETCORE
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Test.Builder.TestModels;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.OData.Edm;
using Newtonsoft.Json.Linq;
using Xunit;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Test.Abstraction;
#else
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNet.OData.Test.Builder.TestModels;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.OData.Edm;
using Newtonsoft.Json.Linq;
using Xunit;
#endif

namespace Microsoft.AspNet.OData.Test.Formatter
{
    public class NullCollectionsTests
    {
        private HttpClient _client;

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
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.EntitySet<NullCollectionsTestsModel>("NullCollectionsTests");
            builder.EntitySet<Vehicle>("vehicles");
            IEdmModel model = builder.GetEdmModel();

            var controllers = new[] { typeof(NullCollectionsTestsController) };
            var server = TestServerFactory.Create(controllers, (config) =>
            {
                config.MapODataServiceRoute("IgnoredRouteName", null, model);
            });

            _client = TestServerFactory.CreateClient(server);

        }

        [Theory]
        // -- "NormalPass_FromEntity" -- ensure existing behavior has not changed when serializing 
        //                               properties off the base entity
        [InlineData("Array")]
        [InlineData("IEnumerable")]
        [InlineData("ICollection")]
        [InlineData("IList")]
        [InlineData("List")]
        [InlineData("Collection")]
        [InlineData("CustomCollection")]
        [InlineData("NullableColors")]
        [InlineData("ComplexCollection")]
        public async Task NullCollectionProperties_NormalPass_FromEntity(string propertyName)
        {
            // Arrange
            NullCollectionsTestsController.TestObject = new NullCollectionsTestsModel();

            // Act
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/NullCollectionsTests/");
            HttpResponseMessage response = await _client.SendAsync(request);

            // Assert
            ExceptionAssert.DoesNotThrow(() => response.EnsureSuccessStatusCode());
            string responseJson = await response.Content.ReadAsStringAsync();
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
        [InlineData("Array")]
        [InlineData("IEnumerable")]
        [InlineData("ICollection")]
        [InlineData("IList")]
        [InlineData("List")]
        [InlineData("Collection")]
        [InlineData("CustomCollection")]
        [InlineData("NullableColors")]
        [InlineData("ComplexCollection")]
        public async Task NullCollectionProperties_SerializeAsEmpty_FromEntity(string propertyName)
        {
            // Arrange
            NullCollectionsTestsModel testObject = new NullCollectionsTestsModel();
            testObject.GetType().GetProperty(propertyName).SetValue(testObject, null);
            NullCollectionsTestsController.TestObject = testObject;

            // Act
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/NullCollectionsTests/");
            HttpResponseMessage response = await _client.SendAsync(request);

            // Assert
            ExceptionAssert.DoesNotThrow(() => response.EnsureSuccessStatusCode());
            string responseJson = await response.Content.ReadAsStringAsync();
            dynamic result = JToken.Parse(responseJson);
            Assert.NotNull(result[propertyName]);
            IEnumerable<JObject> collection = result[propertyName].Values<JObject>();
            Assert.Empty(collection);
        }

        [Theory]
        // -- "NormalPass_FromParentComplex" -- ensure existing behavior has not changed when serializing 
        //                                      properties off a complex attached to the entity
        [InlineData("Array")]
        [InlineData("IEnumerable")]
        [InlineData("ICollection")]
        [InlineData("IList")]
        [InlineData("List")]
        [InlineData("Collection")]
        [InlineData("CustomCollection")]
        [InlineData("NullableColors")]
        [InlineData("ComplexCollection")]
        public async Task NullCollectionProperties_NormalPass_FromParentComplex(string propertyName)
        {
            // Arrange
            NullCollectionsTestsController.TestObject = new NullCollectionsTestsModel();

            // Act
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/NullCollectionsTests/");
            HttpResponseMessage response = await _client.SendAsync(request);

            // Assert
            ExceptionAssert.DoesNotThrow(() => response.EnsureSuccessStatusCode());
            string responseJson = await response.Content.ReadAsStringAsync();
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
        [InlineData("Array")]
        [InlineData("IEnumerable")]
        [InlineData("ICollection")]
        [InlineData("IList")]
        [InlineData("List")]
        [InlineData("Collection")]
        [InlineData("CustomCollection")]
        [InlineData("NullableColors")]
        [InlineData("ComplexCollection")]
        public async Task NullCollectionProperties_SerializeAsEmpty_FromParentComplex(string propertyName)
        {
            // Arrange
            NullCollectionsTestsModel testObject = new NullCollectionsTestsModel();
            testObject.ParentComplex.GetType().GetProperty(propertyName).SetValue(testObject.ParentComplex, null);
            NullCollectionsTestsController.TestObject = testObject;

            // Act
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/NullCollectionsTests/");
            HttpResponseMessage response = await _client.SendAsync(request);

            // Assert
            ExceptionAssert.DoesNotThrow(() => response.EnsureSuccessStatusCode());
            string responseJson = await response.Content.ReadAsStringAsync();
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
