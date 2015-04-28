using System.Collections.Generic;
using Nuwa.Sdk;

namespace Nuwa.Perceiver
{
    internal class DefaultPerceiverList : IPerceiverList
    {
        public DefaultPerceiverList(
            HostPerceiver host,
            TracePerceiver trace,
            SecurityOptionPerceiver security,
            HttpClientConfigurationPerceiver client)
        {
            this.Perceivers = new List<IRunElementPerceiver>
            {
                host, trace, security, client
            };
        }

        public IList<IRunElementPerceiver> Perceivers
        {
            get;
            private set;
        }
    }
}