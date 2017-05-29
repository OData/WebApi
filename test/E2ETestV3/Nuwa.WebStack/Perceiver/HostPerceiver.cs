using System;
using System.Collections.Generic;
using System.Linq;
using Nuwa.Sdk;
using Nuwa.Sdk.Elements;
using Nuwa.WebStack;
using Nuwa.WebStack.Descriptor;
using Nuwa.WebStack.Host;
using Nuwa.WebStack.Route;
using WebStack.QA.Common.WebHost;
using Xunit.Sdk;

namespace Nuwa.Perceiver
{
    internal class HostPerceiver : IRunElementPerceiver
    {
        private IRouteFactory _route;
        private IPortArranger _ports;
        private IEnumerable<HostType> _defaultHosts;
        private ITemporaryDirectoryProvider _dirProvider;

        public HostPerceiver(IRouteFactory route, IPortArranger ports, IEnumerable<HostType> defaultHosts, ITemporaryDirectoryProvider dirProvider)
        {
            _route = route;
            _ports = ports;
            _defaultHosts = defaultHosts;
            _dirProvider = dirProvider;
        }

        public IEnumerable<IRunElement> Perceive(ITestClassCommand ntcc)
        {
            var descriptor = new TestTypeDescriptor(ntcc.TypeUnderTest);

            var attrs = new HashSet<HostType>(
                ntcc.TypeUnderTest.GetCustomAttributes<NwHostAttribute>().Select(a => a.HostType));

            if (attrs.Count() == 0)
            {
                var defaultHostTypesSetting = NuwaGlobalConfiguration.DefaultHostTypes;
                if (!string.IsNullOrEmpty(defaultHostTypesSetting))
                {
                    var defaultHosts = new List<HostType>();
                    foreach (var type in defaultHostTypesSetting.Split(','))
                    {
                        defaultHosts.Add((HostType)Enum.Parse(typeof(HostType), type, true));
                    }
                    _defaultHosts = defaultHosts;
                }

                // fall back to default setting
                foreach (var host in _defaultHosts)
                {
                    attrs.Add(host);
                }
            }

            var retvals = new List<IRunElement>();

            if (attrs.Contains(HostType.WcfSelf))
            {
                var host = new WcfSelfHostElement(descriptor, _route, _ports);
                retvals.Add(host);
            }

            if (attrs.Contains(HostType.IIS))
            {
                var host = new IISHostElement(descriptor, _route, _dirProvider);
                retvals.Add(host);
            }

            if (attrs.Contains(HostType.IISExpress))
            {
                var host = new IISExpressHostElement(descriptor, _route, _dirProvider);
                retvals.Add(host);
            }

            if (attrs.Contains(HostType.AzureWebsite))
            {
                var host = new AzureWebsiteHostElement(descriptor, _route, _dirProvider);
                retvals.Add(host);
            }

            if (attrs.Contains(HostType.KatanaSelf))
            {
                var host = new KatanaSelfHostElement(descriptor, _route, _ports);
                retvals.Add(host);
            }

            if (attrs.Contains(HostType.IISKatana))
            {
                var host = new IISHostElement(descriptor, _route, _dirProvider);
                host.EnableDefaultOwinWebApiConfiguration = true;
                host.EnableGlobalAsax = false;
                retvals.Add(host);
            }

            if (attrs.Contains(HostType.IISExpressKatana))
            {
                var host = new IISExpressHostElement(descriptor, _route, _dirProvider);
                host.EnableDefaultOwinWebApiConfiguration = true;
                host.EnableGlobalAsax = false;
                retvals.Add(host);
            }

            if (attrs.Contains(HostType.AzureWebsiteKatana))
            {
                var host = new AzureWebsiteHostElement(descriptor, _route, _dirProvider);
                host.EnableDefaultOwinWebApiConfiguration = true;
                host.EnableGlobalAsax = false;
                retvals.Add(host);
            }

            return retvals;
        }
    }
}