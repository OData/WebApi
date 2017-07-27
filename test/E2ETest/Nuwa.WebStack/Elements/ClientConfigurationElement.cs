using System;
using System.Linq;
using Nuwa.Client;

namespace Nuwa.Sdk.Elements
{
    internal class ClientConfigurationElement : AbstractRunElement
    {
        protected static readonly string KeyClientStrategy = "HostElement_ClientStrategy";

        public ClientConfigurationElement()
        {
            this.Name = "ClientConfig";
        }

        /// <summary>
        /// Setting log request and response message during runtime. It is off by default.
        /// 
        /// If this property is set to true, client will print the request and 
        /// response to Console.
        /// </summary>
        public bool MessageLog { get; set; }

        /// <summary>
        /// Enable proxy when sending request. Off by default
        /// 
        /// If this property is true, every request send by the HttpClient will be redirected to
        /// a proxy server to simulate a cross machine request.
        /// </summary>
        public bool UseProxy { get; set; }

        public override void Initialize(RunFrame frame)
        {
            /// Create a client strategy which is associated with this host strategy.
            /// Client strategy is rather a component relatively indepdent from 
            /// host strategy is because not like host strategy, the client
            /// strategy can be different on test method basis. For example the
            /// security credential could be different. So a HttpClient is created
            /// on basis of eacy test method (in the domain of xunit, is every 
            /// TestCommand).
            var clientStrategy = new DefaultClientStrategy
            {
                MessageLog = MessageLog,
                UseProxy = UseProxy
            };

            frame.SetState(KeyClientStrategy, clientStrategy);
        }

        public override void Recover(object testClass, NuwaTestCommand testCommand)
        {
            SetHttpclient(testClass, testCommand);
        }

        /// <summary>
        /// Create an http client according to configuration and host strategy
        /// </summary>
        /// <param name="testClass">the test class under test</param>
        protected void SetHttpclient(object testClass, NuwaTestCommand testCommand)
        {
            Type testClassType = testClass.GetType();

            // set the HttpClient if necessary
            var clientPrpt = testClassType.GetProperties()
                .Where(prop => { return prop.GetCustomAttributes(typeof(NuwaHttpClientAttribute), false).Length == 1; })
                .FirstOrDefault();

            if (clientPrpt == null || NuwaHttpClientAttribute.Verify(clientPrpt) == false)
            {
                return;
            }

            // create a client strategy from the host strategy
            var strategy = testCommand.Frame.GetState(KeyClientStrategy) as IClientStrategy;
            if (strategy == null)
            {
                return;
            }

            // check the network crediential
            if (testCommand.TestMethod != null)
            {
                var credAttr = testCommand.TestMethod.GetFirstCustomAttribute<NuwaNetworkCredentialAttribute>();
                if (credAttr != null)
                {
                    strategy.Credentials = credAttr.Credential;
                }
            }

            // create client assign to property
            if (strategy != null)
            {
                strategy.MessageLog = this.MessageLog;
                clientPrpt.SetValue(testClass, strategy.CreateClient(), null);
            }
            else
            {
                // TODO: send warning
            }
        }
    }
}
