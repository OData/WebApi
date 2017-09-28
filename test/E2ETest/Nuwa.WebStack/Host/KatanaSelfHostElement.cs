using System;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Threading;
using System.Web.Http;
using System.Web.Http.Tracing;
using Microsoft.Owin.Hosting;
using Nuwa.Sdk;
using Nuwa.Sdk.Elements;
using Nuwa.WebStack.Descriptor;
using Nuwa.WebStack.Route;
using Owin;
using WebStack.QA.Common.WebHost;

namespace Nuwa.WebStack.Host
{
    internal class KatanaSelfHostElement : BaseHostElement
    {
        private static readonly string NormalBaseAddressTemplate = "http://{0}:{1}";
        private static readonly string SecureBaseAddressTemplate = "https://{0}:{1}";

        private static readonly string KeyIsSecuredServer = "KatanaSelfHostElement_IsSecuredServer";
        private static readonly string KeyServerInitiator = "KatanaSelfHostElement_ServerInitiator";
        private static readonly string KeySandbox = "KatanaSelfHostElement_Sandbox";
        private static readonly string KeyReservedPort = "ReservedPort";

        private IPortArranger _portArranger;

        public KatanaSelfHostElement(TestTypeDescriptor descriptor, IRouteFactory routeFactory, IPortArranger portArranger)
            : base(descriptor, routeFactory)
        {
            this.Name = "Katana-Self-Host";
            _portArranger = portArranger;
        }

        protected override bool InitializeServer(RunFrame frame)
        {
            var sandbox = CreateFullTrustAppDomain();
            frame.SetState(KeySandbox, sandbox);

            // load test assemly into sandbox
            if (sandbox != null && TypeDescriptor.TestAssembly != null)
            {
                sandbox.Load(TypeDescriptor.TestAssembly.GetName());
            }

            SecurityHelper.AddIpListen();

            // retry three times using port scan with three port types.
            int repeat = 3;

            bool result = false;
            for (int i = 1; i <= repeat; ++i)
            {
                if (TrySetupServer(i, sandbox, frame))
                {
                    result = true;
                    break;
                }
            }

            if (result == false)
            {
                throw new Exception(string.Format("Cannot setup host server. See Event log for details"));
            }

            return true;
        }

        private bool TrySetupServer(int tryIndex, AppDomain sandbox, RunFrame frame)
        {
            string baseAddress;
            string port = _portArranger.Reserve();

            SecurityOptionElement securityElem = frame.GetFirstElement<SecurityOptionElement>();
            if (securityElem != null)
            {
                SetupSecureEnvironment(securityElem.Certificate, port);
                baseAddress = string.Format(SecureBaseAddressTemplate, Environment.MachineName, port);
                frame.SetState(KeyIsSecuredServer, true);
            }
            else
            {
                baseAddress = string.Format(NormalBaseAddressTemplate, Environment.MachineName, port);
                frame.SetState(KeyIsSecuredServer, false);
            }

            // looking into the RunFrames and search for TraceElement. if it exists
            // set the tracer's type to the configuration otherwise skip this step
            TraceElement traceElem = frame.GetFirstElement<TraceElement>();
            Type traceType = null;
            if (traceElem != null)
            {
                traceType = traceElem.TracerType;
            }

            KatanaSelfHostServerInitiator serverInitiator;
            // create initiator in the sandbox
            if (sandbox != null)
            {
                serverInitiator = sandbox.CreateInstanceAndUnwrap(
                    typeof(KatanaSelfHostServerInitiator).Assembly.FullName,
                    typeof(KatanaSelfHostServerInitiator).FullName)
                    as KatanaSelfHostServerInitiator;
            }
            else
            {
                serverInitiator = new KatanaSelfHostServerInitiator();
            }
            try
            {
                // set up the server
                serverInitiator.Setup(
                    baseAddress,
                    TypeDescriptor.GetDesignatedMethod<NuwaKatanaConfigurationAttribute>(),
                    TypeDescriptor.ConfigureMethod,
                    traceType, GetDefaultRouteTemplate());
            }
            catch (Exception ex)
            {
                EventLog appLog = new System.Diagnostics.EventLog();
                appLog.Source = "Nuwa Katana Self Host Test";
                appLog.WriteEntry(string.Format("try index: {0}\nbase address: {1}\n message: {2}\n stack trace: {3}\n", tryIndex, baseAddress, ex.Message, ex.StackTrace),
                    EventLogEntryType.Error);

                return false;
            }

            frame.SetState(KeyReservedPort, port);
            frame.SetState(KeyBaseAddresss, baseAddress);
            frame.SetState(KeyServerInitiator, serverInitiator);
            return true;
        }

        protected override void ShutdownServer(RunFrame frame)
        {
            var serverInitiator = frame.GetState(KeyServerInitiator) as KatanaSelfHostServerInitiator;
            if (serverInitiator != null)
            {
                serverInitiator.Dispose();
                frame.SetState(KeyServerInitiator, null);
            }

            var sandbox = frame.GetState(KeySandbox) as AppDomain;
            if (sandbox != null)
            {
                AppDomain.Unload(sandbox);
                frame.SetState(KeySandbox, null);
            }

            var securedServer = frame.GetState(KeyIsSecuredServer) ?? false;
            if ((bool)securedServer)
            {
                var port = frame.GetState(KeyReservedPort) as string;
                TeardownSecureEnvironment(port);
                frame.SetState(KeyIsSecuredServer, null);
            }
        }

        private static AppDomain CreateFullTrustAppDomain()
        {
            var retval = AppDomain.CreateDomain(
                "Full Trust Sandbox", null,
                new AppDomainSetup { ApplicationBase = AppDomain.CurrentDomain.BaseDirectory });

            return retval;
        }

        private static void SetupSecureEnvironment(X509Certificate2 certificate, string port)
        {
            SecurityHelper.PrepareEnvironment("serverCert.pfx", port, WindowsIdentity.GetCurrent().Name, "security");

            ServicePointManager.ServerCertificateValidationCallback +=
                (sender, cert, chan, sslPolicyErrors) => cert.GetCertHashString() == certificate.GetCertHashString();
        }

        private static void TeardownSecureEnvironment(string port)
        {
            SecurityHelper.CleanupEnvironment("serverCert.pfx", port, "security");
        }

        private class KatanaSelfHostServerInitiator : MarshalByRefObject, IDisposable
        {
            private IDisposable _katanaSelfHostServer = null;

            private Type _traceWriterType;
            private string _defaultRouteTemplate;
            private MethodInfo _httpConfigure;

            public void Setup(string baseAddress, MethodInfo katanaConfigure, MethodInfo httpConfigure, Type traceWriterType, string defaultRouteTemplate)
            {
                _httpConfigure = httpConfigure;
                _traceWriterType = traceWriterType;
                _defaultRouteTemplate = defaultRouteTemplate;

                Action<IAppBuilder> katanaConfigureAction;
                if (katanaConfigure == null)
                {
                    katanaConfigureAction = DefaultKatanaConfigure;
                }
                else
                {
                    katanaConfigureAction = (Action<IAppBuilder>)Delegate.CreateDelegate(typeof(Action<IAppBuilder>), katanaConfigure);
                }

                _katanaSelfHostServer = WebApp.Start(baseAddress, katanaConfigureAction);
            }

            public void Dispose()
            {
                if (_katanaSelfHostServer != null)
                {
                    _katanaSelfHostServer.Dispose();
                }
            }

            private void DefaultKatanaConfigure(IAppBuilder app)
            {
                // Set default principal to avoid OWIN selfhost bug with VS debugger
                app.Use(async (context, next) =>
                {
                    Thread.CurrentPrincipal = null;
                    await next();
                });

                var configuration = new HttpConfiguration();

                // default map
                configuration.Routes.MapHttpRoute(
                    "api default", _defaultRouteTemplate, new { action = RouteParameter.Optional });

                if (_traceWriterType != null)
                {
                    configuration.Services.Replace(typeof(ITraceWriter), Activator.CreateInstance(_traceWriterType));
                }

                var httpServer = new HttpServer(configuration);
                configuration.SetHttpServer(httpServer);
                configuration.Properties["Nuwa.HostType"] = HostType.KatanaSelf;
                if (_httpConfigure != null)
                {
                    _httpConfigure.Invoke(null, new object[] { configuration });
                }

                app.UseWebApi(httpServer: httpServer);
            }
        }
    }
}
