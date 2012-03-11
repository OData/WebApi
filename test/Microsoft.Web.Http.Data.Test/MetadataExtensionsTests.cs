using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Json;
using System.Linq;
using Microsoft.Web.Http.Data.Helpers;
using Xunit;

namespace Microsoft.Web.Http.Data.Test
{
    public class MetadataExtensionsTests
    {
        /// <summary>
        /// Serialize metadata for a test controller exposing types with various
        /// metadata annotations.
        /// </summary>
        [Fact]
        public void TestMetadataSerialization()
        {
            JsonValue metadata = GenerateMetadata(typeof(TestController));
            string s = metadata.ToString();
            Assert.True(s.Contains("{\"range\":[-10,20.5]}"));
        }

        private static JsonValue GenerateMetadata(Type dataControllerType)
        {
            DataControllerDescription desc = DataControllerDescriptionTest.GetDataControllerDescription(dataControllerType);
            var metadata = DataControllerMetadataGenerator.GetMetadata(desc);

            var jsonData = metadata.Select(m => new KeyValuePair<string, JsonValue>(m.EncodedTypeName, m.ToJsonValue()));
            JsonValue metadataValue = new JsonObject(jsonData);

            return metadataValue;
        }
    }

    public class TestClass
    {
        [Key]
        public int ID { get; set; }

        [Required]
        public string Name { get; set; }

        [Range(-10.0, 20.5)]
        public double Number { get; set; }

        [StringLength(5)]
        public string Address { get; set; }
    }

    public class TestController : DataController
    {
        public TestClass GetTestClass(int id)
        {
            return null;
        }
    }
}
