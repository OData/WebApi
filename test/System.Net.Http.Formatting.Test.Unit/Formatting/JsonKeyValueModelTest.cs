using System.Json;
using Microsoft.TestCommon;
using Xunit.Extensions;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Net.Http.Formatting
{
    public class JsonKeyValueModelTest
    {
        [Theory]
        [PropertyData("JsonKeyValueModelBuildsCorrectlyData")]
        public void JsonKeyValueModelBuildsCorrectly(string json, string expectedKey, object expectedValue)
        {
            JsonKeyValueModel keyValueModel = new JsonKeyValueModel(JsonValue.Parse(json));

            object value;
            Assert.Contains(expectedKey, keyValueModel.Keys);
            Assert.True(keyValueModel.TryGetValue(expectedKey, out value));
            Assert.Equal(expectedValue, value);
        }

        public static TheoryDataSet<string, string, object> JsonKeyValueModelBuildsCorrectlyData
        {
            get
            {
                return new TheoryDataSet<string, string, object>
                {
                    { "{ \"input\" : [1, 2, 3] }", "input[0]", "1" }, // array inside dict
                    { "{ \"input\" : [1, 2, 3] }", "input[1]", "2" }, // array inside dict
                    { "{ \"input\" : [1, 2, 3] }", "input[2]", "3" }, // array inside dict
                    { "[1, 2, 3]" , "[0]", "1" }, // just a json array
                    { "{ \"foo\" : [ { \"bar\" : 1 }, { \"bar\" : 1 } ] }", "foo[1].bar", "1" }, // dict inside array inside dict
                    { "[ [1, 2, 3], [ 2, 3, 4] ]", "[1][2]", "4" }, // array of array
                    { "[ { \"foo\" : \"bar\" }, { \"foo1\" : \"bar1\" }]", "[1].foo1" , "bar1" } // array of dicts
                };
            }
        }
    }
}
