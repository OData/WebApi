using System.Collections.Generic;
using Nuwa.Sdk;
using Nuwa.WebStack.Descriptor;
using WebStack.QA.Common.FileSystem;
using WebStack.QA.Common.WebHost;

namespace Nuwa.WebStack.Host
{
    public class WebHostContext
    {
        public WebHostContext()
        {
            Properties = new Dictionary<string, object>();
        }

        /// <summary>
        /// The descriptor for test class type
        /// </summary>
        public TestTypeDescriptor TestType { get; set; }

        /// <summary>
        /// The descriptor for deployment strategy
        /// </summary>
        public DeploymentDescriptor Deployment { get; set; }

        public RunFrame Frame { get; set; }

        public IDirectory Source { get; set; }

        public DeploymentOptions DeploymentOptions { get; set; }

        public HostOptions HostOptions { get; set; }

        public Dictionary<string, object> Properties { get; set; }
    }
}
