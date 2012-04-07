// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace System.Json
{
    public class JsonTypeTest
    {
        [Fact]
        public void JsonTypeValues()
        {
            string[] allJsonTypeExpectedValues = new string[] { "Array", "Boolean", "Default", "Number", "Object", "String" };
            JsonType[] allJsonTypeActualValues = (JsonType[])Enum.GetValues(typeof(JsonType));

            Assert.Equal(allJsonTypeExpectedValues.Length, allJsonTypeActualValues.Length);

            List<string> allJsonTypeActualStringValues = new List<string>(allJsonTypeActualValues.Select((x) => x.ToString()));
            allJsonTypeActualStringValues.Sort(StringComparer.Ordinal);

            for (int i = 0; i < allJsonTypeExpectedValues.Length; i++)
            {
                Assert.Equal(allJsonTypeExpectedValues[i], allJsonTypeActualStringValues[i]);
            }
        }
    }
}
