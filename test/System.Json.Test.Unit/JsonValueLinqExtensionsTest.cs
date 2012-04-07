// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace System.Json
{
    public class JsonValueLinqExtensionsTest
    {
        [Fact]
        public void ToJsonArrayTest()
        {
            var target = (new List<int>(new[] { 1, 2, 3 }).Select(i => (JsonValue)i).ToJsonArray());
            Assert.Equal("[1,2,3]", target.ToString());
        }

        [Fact]
        public void ToJsonObjectTest()
        {
            JsonValue jv = new JsonObject { { "one", 1 }, { "two", 2 }, { "three", 3 } };

            var result = from n in jv
                         where n.Value.ReadAs<int>() > 1
                         select n;
            Assert.Equal("{\"two\":2,\"three\":3}", result.ToJsonObject().ToString());
        }

        [Fact]
        public void ToJsonObjectFromArray()
        {
            JsonArray ja = new JsonArray("first", "second");
            JsonObject jo = ja.ToJsonObject();
            Assert.Equal("{\"0\":\"first\",\"1\":\"second\"}", jo.ToString());
        }
    }
}
