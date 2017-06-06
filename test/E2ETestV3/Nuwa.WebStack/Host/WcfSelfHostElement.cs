using System;
using System.Data.SqlClient;
using System.Net;
using System.Net.Mail;
using System.Reflection;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.Security.Permissions;
using System.Security.Principal;
using System.Web;
using System.Web.Http;
using System.Web.Http.SelfHost;
using System.Web.Http.Tracing;
using Nuwa.WebStack.Descriptor;
using Nuwa.WebStack.Host;
using Nuwa.WebStack.Route;
using WebStack.QA.Common.WebHost;

namespace Nuwa.Sdk.Elements
{
    internal class WcfSelfHostElement : BaseHostElement
    {
        private static readonly string NormalBaseAddressTemplate = "http://localhost:{0}";
        private static readonly string SecureBaseAddressTemplate = "https://localhost:{0}";

        private static readonly string KeyIsSecuredServer = "WcfSelfHostElement_IsSecuredServer";
        private static readonly string KeyServerInitiator = "WcfSelfHostElement_ServerInitiator";
        private static readonly string KeySandbox = "WcfSelfHostElement_Sandbox";
        private static readonly string KeyReservedPort = "ReservedPort";

        private IPortArranger _portArranger;

        public WcfSelfHostElement(TestTypeDescriptor descriptor, IRouteFactory routeFactory, IPortArranger portArranger)
            : base(descriptor, routeFactory)
        {
            this.Name = "Wcf-Self-Host";
            _portArranger = portArranger;
        }

        protected override bool InitializeServer(RunFrame frame)
        {
            string baseAddress;
            string port;

            AppDomain sandbox = CreateFullTrustAppDomain();
            frame.SetState(KeySandbox, sandbox);

            // load test assemly into sandbox
            if (sandbox != null && TypeDescriptor.TestAssembly != null)
            {
                sandbox.Load(TypeDescriptor.TestAssembly.GetName());
            }

            // setup security strategy and base address
            port = _portArranger.Reserve();
            SecurityOptionElement securityElem = frame.GetFirstElement<SecurityOptionElement>();
            if (securityElem != null)
            {
                SetupSecureEnvironment(securityElem.Certificate, port);
                baseAddress = string.Format(SecureBaseAddressTemplate, port);
                frame.SetState(KeyIsSecuredServer, true);
            }
            else
            {
                baseAddress = string.Format(NormalBaseAddressTemplate, port);
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

            // create initiator in the sandbox
            SelfHostServerInitiator serverInitiator;
            if (sandbox != null)
            {
                serverInitiator = sandbox.CreateInstanceAndUnwrap(
                    typeof(SelfHostServerInitiator).Assembly.FullName,
                    typeof(SelfHostServerInitiator).FullName)
                    as SelfHostServerInitiator;
            }
            else
            {
                serverInitiator = new SelfHostServerInitiator();
            }

            // set up the server
            serverInitiator.Setup(baseAddress, TypeDescriptor.ConfigureMethod, traceType, GetDefaultRouteTemplate());

            frame.SetState(KeyReservedPort, port);
            frame.SetState(KeyBaseAddresss, baseAddress);
            frame.SetState(KeyServerInitiator, serverInitiator);

            return true;
        }

        protected override void ShutdownServer(RunFrame frame)
        {
            var serverInitiator = frame.GetState(KeyServerInitiator) as SelfHostServerInitiator;
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

        public override void Recover(object testClass, NuwaTestCommand testCommand)
        {
            base.Recover(testClass, testCommand);
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

        private static AppDomain CreatePartialTrustAppDomain()
        {
            PermissionSet permissions = new PermissionSet(PermissionState.None);
            permissions.AddPermission(new AspNetHostingPermission(AspNetHostingPermissionLevel.Medium));
            permissions.AddPermission(new DnsPermission(PermissionState.Unrestricted));
            permissions.AddPermission(
                new EnvironmentPermission(EnvironmentPermissionAccess.Read, "TEMP;TMP;USERNAME;OS;COMPUTERNAME"));
            permissions.AddPermission(
                new FileIOPermission(FileIOPermissionAccess.AllAccess, AppDomain.CurrentDomain.BaseDirectory));
            permissions.AddPermission(
                new IsolatedStorageFilePermission(PermissionState.None)
                {
                    UsageAllowed = IsolatedStorageContainment.AssemblyIsolationByUser,
                    UserQuota = long.MaxValue
                });
            permissions.AddPermission(new SecurityPermission(SecurityPermissionFlag.Execution));
            permissions.AddPermission(new SecurityPermission(SecurityPermissionFlag.ControlThread));
            permissions.AddPermission(new SecurityPermission(SecurityPermissionFlag.ControlPrincipal));
            permissions.AddPermission(new SecurityPermission(SecurityPermissionFlag.RemotingConfiguration));
            permissions.AddPermission(new SmtpPermission(SmtpAccess.Connect));
            permissions.AddPermission(new SqlClientPermission(PermissionState.Unrestricted));
            permissions.AddPermission(new TypeDescriptorPermission(PermissionState.Unrestricted));
            permissions.AddPermission(new WebPermission(PermissionState.Unrestricted));
            permissions.AddPermission(new ReflectionPermission(ReflectionPermissionFlag.RestrictedMemberAccess));

            AppDomainSetup setup = new AppDomainSetup
            {
                ApplicationBase = AppDomain.CurrentDomain.BaseDirectory,

                // TODO: audit these assemblies
                PartialTrustVisibleAssemblies =
                    new string[]
                            {
                                "System.Web, PublicKey=002400000480000094000000060200000024000052534131000400000100010007d1fa57c4aed9f0a32e84aa0faefd0de9e8fd6aec8f87fb03766c834c99921eb23be79ad9d5dcc1dd9ad236132102900b723cf980957fc4e177108fc607774f29e8320e92ea05ece4e821c0a5efe8f1645c4c0c93c1ab99285d622caa652c1dfad63d745d6f2de5f17e5eaf0fc4963d261c8a12436518206dc093344d5ad293",
                                "System.Web.Extensions, PublicKey=0024000004800000940000000602000000240000525341310004000001000100b5fc90e7027f67871e773a8fde8938c81dd402ba65b9201d60593e96c492651e889cc13f1415ebb53fac1131ae0bd333c5ee6021672d9718ea31a8aebd0da0072f25d87dba6fc90ffd598ed4da35e44c398c454307e8e33b8426143daec9f596836f97c8f74750e5975c64e2189f45def46b2a2b1247adc3652bf5c308055da9",
                                "System.Web.Abstractions, PublicKey=0024000004800000940000000602000000240000525341310004000001000100b5fc90e7027f67871e773a8fde8938c81dd402ba65b9201d60593e96c492651e889cc13f1415ebb53fac1131ae0bd333c5ee6021672d9718ea31a8aebd0da0072f25d87dba6fc90ffd598ed4da35e44c398c454307e8e33b8426143daec9f596836f97c8f74750e5975c64e2189f45def46b2a2b1247adc3652bf5c308055da9",
                                "System.Web.Routing, PublicKey=0024000004800000940000000602000000240000525341310004000001000100b5fc90e7027f67871e773a8fde8938c81dd402ba65b9201d60593e96c492651e889cc13f1415ebb53fac1131ae0bd333c5ee6021672d9718ea31a8aebd0da0072f25d87dba6fc90ffd598ed4da35e44c398c454307e8e33b8426143daec9f596836f97c8f74750e5975c64e2189f45def46b2a2b1247adc3652bf5c308055da9",
                                "System.ComponentModel.DataAnnotations, PublicKey=0024000004800000940000000602000000240000525341310004000001000100b5fc90e7027f67871e773a8fde8938c81dd402ba65b9201d60593e96c492651e889cc13f1415ebb53fac1131ae0bd333c5ee6021672d9718ea31a8aebd0da0072f25d87dba6fc90ffd598ed4da35e44c398c454307e8e33b8426143daec9f596836f97c8f74750e5975c64e2189f45def46b2a2b1247adc3652bf5c308055da9",
                                "System.Web.DynamicData, PublicKey=0024000004800000940000000602000000240000525341310004000001000100b5fc90e7027f67871e773a8fde8938c81dd402ba65b9201d60593e96c492651e889cc13f1415ebb53fac1131ae0bd333c5ee6021672d9718ea31a8aebd0da0072f25d87dba6fc90ffd598ed4da35e44c398c454307e8e33b8426143daec9f596836f97c8f74750e5975c64e2189f45def46b2a2b1247adc3652bf5c308055da9",
                                "System.Web.DataVisualization, PublicKey=0024000004800000940000000602000000240000525341310004000001000100b5fc90e7027f67871e773a8fde8938c81dd402ba65b9201d60593e96c492651e889cc13f1415ebb53fac1131ae0bd333c5ee6021672d9718ea31a8aebd0da0072f25d87dba6fc90ffd598ed4da35e44c398c454307e8e33b8426143daec9f596836f97c8f74750e5975c64e2189f45def46b2a2b1247adc3652bf5c308055da9",
                                "System.Web.ApplicationServices, PublicKey=0024000004800000940000000602000000240000525341310004000001000100b5fc90e7027f67871e773a8fde8938c81dd402ba65b9201d60593e96c492651e889cc13f1415ebb53fac1131ae0bd333c5ee6021672d9718ea31a8aebd0da0072f25d87dba6fc90ffd598ed4da35e44c398c454307e8e33b8426143daec9f596836f97c8f74750e5975c64e2189f45def46b2a2b1247adc3652bf5c308055da9"
                            }
            };

            return AppDomain.CreateDomain("Partial Trust Sandbox", null, setup, permissions);
        }

        private static AppDomain CreateFullTrustAppDomain()
        {
            var retval = AppDomain.CreateDomain(
                "Full Trust Sandbox", null,
                new AppDomainSetup { ApplicationBase = AppDomain.CurrentDomain.BaseDirectory });

            return retval;
        }

        private class SelfHostServerInitiator : MarshalByRefObject, IDisposable
        {
            private HttpSelfHostServer _server;

            public void Setup(string baseAddress, MethodInfo config, Type traceWriterType, string defaultRouteTemplate)
            {
                var configuration = new HttpSelfHostConfiguration(baseAddress);

                // default map
                // TODO: centralize default route mapping in all host strategies
                configuration.Routes.MapHttpRoute(
                    "api default", defaultRouteTemplate, new { action = RouteParameter.Optional });

                // TODO: Add a sample educating user how to communicate to Tracer under partial trust.
                if (traceWriterType != null)
                {
                    /// TODO: improvement
                    /// There are other ways of initializing a trace writer, for example you can pass in
                    /// a factory method or an already-created instance.
                    configuration.Services.Replace(typeof(ITraceWriter), Activator.CreateInstance(traceWriterType));
                }

                _server = new HttpSelfHostServer(configuration);
                configuration.SetHttpServer(_server);

                if (config != null)
                {
                    config.Invoke(null, new object[] { configuration });
                }

                _server.OpenAsync().Wait();
            }

            public void Dispose()
            {
                if (_server != null)
                {
                    _server.CloseAsync().Wait();
                }
            }
        }
    }
}