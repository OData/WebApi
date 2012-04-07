// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using Xunit;

namespace System.Json
{
    /// <summary>
    /// Tests for the Linq extensions to the <see cref="JsonValue"/> types.
    /// </summary>
    public class JsonValueLinqExtensionsIntegrationTest
    {
        /// <summary>
        /// Test for the <see cref="JsonValueLinqExtensions.ToJsonArray"/> method.
        /// </summary>
        [Fact]
        public void ToJsonArrayTest()
        {
            string json = "{\"SearchResponse\":{\"Phonebook\":{\"Results\":[{\"name\":1,\"rating\":1}, {\"name\":2,\"rating\":2}, {\"name\":3,\"rating\":3}]}}}";
            string expected = "[{\"name\":2,\"rating\":2},{\"name\":3,\"rating\":3}]";

            JsonValue jv = JsonValue.Parse(json);
            double rating = 1;
            var jsonResult = from n in jv.ValueOrDefault("SearchResponse", "Phonebook", "Results")
                             where n.Value.ValueOrDefault("rating").ReadAs<double>(0.0) > rating
                             select n.Value;
            var ja = jsonResult.ToJsonArray();

            Assert.Equal(expected, ja.ToString());
        }

        /// <summary>
        /// Test for the <see cref="JsonValueLinqExtensions.ToJsonObject"/> method.
        /// </summary>
        [Fact]
        public void ToJsonObjectTest()
        {
            string json = "{\"Name\":\"Bill Gates\",\"Age\":23,\"AnnualIncome\":45340.45,\"MaritalStatus\":\"Single\",\"EducationLevel\":\"MiddleSchool\",\"SSN\":432332453,\"CellNumber\":2340393420}";
            string expected = "{\"AnnualIncome\":45340.45,\"SSN\":432332453,\"CellNumber\":2340393420}";

            JsonValue jv = JsonValue.Parse(json);
            decimal decVal;
            var jsonResult = from n in jv
                             where n.Value.TryReadAs<decimal>(out decVal) && decVal > 100
                             select n;
            var jo = jsonResult.ToJsonObject();

            Assert.Equal(expected, jo.ToString());
        }

        /// <summary>
        /// Test for the <see cref="JsonValueLinqExtensions.ToJsonObject"/> method where the origin is a <see cref="JsonArray"/>.
        /// </summary>
        [Fact]
        public void ToJsonObjectFromArrayTest()
        {
            string json = "{\"SearchResponse\":{\"Phonebook\":{\"Results\":[{\"name\":1,\"rating\":1}, {\"name\":2,\"rating\":2}, {\"name\":3,\"rating\":3}]}}}";
            string expected = "{\"1\":{\"name\":2,\"rating\":2},\"2\":{\"name\":3,\"rating\":3}}";

            JsonValue jv = JsonValue.Parse(json);
            double rating = 1;
            var jsonResult = from n in jv.ValueOrDefault("SearchResponse", "Phonebook", "Results")
                             where n.Value.ValueOrDefault("rating").ReadAs<double>(0.0) > rating
                             select n;
            var jo = jsonResult.ToJsonObject();

            Assert.Equal(expected, jo.ToString());
        }
    }
}
