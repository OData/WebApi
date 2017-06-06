using Autofac;
using Nuwa.Perceiver;
using Nuwa.Sdk;
using Nuwa.WebStack.Factory;
using Nuwa.WebStack.Host;
using Nuwa.WebStack.Route;
using WebStack.QA.Common.WebHost;

namespace Nuwa.DI
{
    public class DependencyResolver
    {
        private static DependencyResolver s_instance;

        private DependencyResolver()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<DefaultPerceiverList>().As<IPerceiverList>();
            builder.RegisterType<WebStackRunFrameFactory>().As<AbstractRunFrameFactory>();
            builder.RegisterType<RunFrameBuilder>().As<IRunFrameBuilder>();
            builder.RegisterType<RouteFactory>().As<IRouteFactory>();
            builder.RegisterType<HostPerceiver>().WithParameter(new NamedParameter("defaultHosts", new HostType[] { HostType.KatanaSelf, HostType.IIS }));
            builder.RegisterType<TracePerceiver>();
            builder.RegisterType<SecurityOptionPerceiver>();
            builder.RegisterType<HttpClientConfigurationPerceiver>();
            builder.RegisterType<PortArranger>().As<IPortArranger>();

            builder.RegisterType<GuidDirectoryNameStrategy>().As<IDirectoryNameStrategy>();
            builder.RegisterType<TemporaryDirectoryProvider>().As<ITemporaryDirectoryProvider>();

            Container = builder.Build();
        }

        public static DependencyResolver Instance
        {
            get
            {
                if (s_instance == null)
                {
                    s_instance = new DependencyResolver();
                }

                return s_instance;
            }
        }

        public IContainer Container
        {
            get;
            private set;
        }
    }
}