using System.Collections.Generic;
using Nuwa.Sdk;
using Nuwa.Sdk.Elements;
using Xunit.Sdk;

namespace Nuwa.Perceiver
{
    internal class SecurityOptionPerceiver : IRunElementPerceiver
    {
        public IEnumerable<IRunElement> Perceive(ITestClassCommand ntcc)
        {
            var securityAttribute = ntcc.TypeUnderTest.GetFirstCustomAttribute<NuwaServerCertificateAttribute>();
            if (securityAttribute != null)
            {
                var elem = new SecurityOptionElement(securityAttribute.Certificate);
                return this.ToArray(elem);
            }
            else
            {
                return this.Empty();
            }
        }
    }
}