// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.Web.Http.Data.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
            JToken metadata = GenerateMetadata(typeof(TestController));
            string s = metadata.ToString(Formatting.None);
            Assert.True(s.Contains("{\"range\":[-10.0,20.5]}"));
        }

        private static JToken GenerateMetadata(Type dataControllerType)
        {
            DataControllerDescription desc = DataControllerDescriptionTest.GetDataControllerDescription(dataControllerType);
            var metadata = DataControllerMetadataGenerator.GetMetadata(desc);

            JObject metadataValue = new JObject();
            foreach (var m in metadata)
            {
                metadataValue.Add(m.EncodedTypeName, m.ToJToken());
            }

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
