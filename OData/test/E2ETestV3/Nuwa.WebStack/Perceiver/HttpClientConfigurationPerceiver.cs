using System.Collections.Generic;
using Nuwa.Sdk;
using Nuwa.Sdk.Elements;
using Xunit.Sdk;

namespace Nuwa.Perceiver
{
    internal class HttpClientConfigurationPerceiver : IRunElementPerceiver
    {
        public IEnumerable<IRunElement> Perceive(ITestClassCommand ntcc)
        {
            var attr = ntcc.TypeUnderTest.GetFirstCustomAttribute<NuwaHttpClientConfigurationAttribute>();

            if (attr != null)
            {
                yield return new ClientConfigurationElement()
                {
                    MessageLog = attr.MessageLog,
                    UseProxy = attr.UseProxy
                };
            }
            else
            {
                yield return new ClientConfigurationElement();
            }
        }
    }
}