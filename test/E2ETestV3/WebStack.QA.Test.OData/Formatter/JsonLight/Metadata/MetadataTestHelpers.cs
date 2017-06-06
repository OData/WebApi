using System;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;
using WebStack.QA.Instancing;

namespace WebStack.QA.Test.OData.Formatter.JsonLight.Metadata
{
    public static class MetadataTestHelpers
    {
        public static JObject ReadAsJObject(this HttpContent element)
        {
            string content = element.ReadAsStringAsync().Result;
            return JObject.Parse(content);
        }

        public static void SetAcceptHeader(this HttpRequestMessage message, string acceptHeader)
        {
            message.Headers.Clear();
            message.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse(acceptHeader));
        }

        public static T CreateInstances<T>()
        {
            var results = InstanceCreator.CreateInstanceOf<T>(new Random(RandomSeedGenerator.GetRandomSeed()), new CreatorSettings { NullValueProbability = 0, AllowEmptyCollection = false });
            return results;
        }
    }
}
