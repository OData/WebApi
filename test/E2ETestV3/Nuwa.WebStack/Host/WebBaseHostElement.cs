using Nuwa.Sdk;
using Nuwa.Sdk.Elements;
using Nuwa.WebStack.Descriptor;
using Nuwa.WebStack.Route;
using Owin;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Http;
using System.Web.Http.Hosting;
using System.Web.Http.Owin;
using System.Web.Http.Tracing;
using WebStack.QA.Common.FileSystem;
using WebStack.QA.Common.WebHost;

namespace Nuwa.WebStack.Host
{
    /// <summary>
    /// The element sets up web host. 
    /// During start up, it retrieves source, initializes deployement and host options, initilaizes configurations, 
    /// deploys, starts host.
    /// When shutdown, it stop and dispose host server and teardown configurations.
    /// </summary>
    public abstract class WebBaseHostElement : BaseHostElement
    {
        private const string SolutionDirAppSettingName = "QARoot";
        private static readonly string KeyWebHostContext = "WebHostElement_WebHostContext";

        protected ITemporaryDirectoryProvider _dirProvider;

        public WebBaseHostElement(TestTypeDescriptor typeDescriptor, IRouteFactory routeFactory, ITemporaryDirectoryProvider dirProvider)
            : base(typeDescriptor, routeFactory)
        {
            _dirProvider = dirProvider;
            // By default, use global.asax to register web api
            EnableDefaultOwinWebApiConfiguration = false;
            EnableGlobalAsax = true;
        }

        public bool EnableDefaultOwinWebApiConfiguration { get; set; }

        public bool EnableGlobalAsax { get; set; }

        public static void DefaultOwinWebApiConfiguration(IAppBuilder app)
        {
            HttpConfiguration configuration =
                new HttpConfiguration(new HttpRouteCollection(HttpRuntime.AppDomainAppVirtualPath));
            Type traceWriterType = null;
            var traceWriterTypeName = NuwaGlobalConfiguration.TraceWriterType;
            MethodInfo httpConfigure = null;
            var httpConfigureName = NuwaGlobalConfiguration.HttpConfigure;

            if (!string.IsNullOrEmpty(traceWriterTypeName))
            {
                var names = traceWriterTypeName.Split(',');
                if (names.Length == 2)
                {
                    var assembly = Assembly.Load(names[1].Trim());
                    traceWriterType = assembly.GetType(names[0].Trim());
                }
                else if (names.Length == 1)
                {
                    traceWriterType = Type.GetType(names[0].Trim());
                }
            }

            if (!string.IsNullOrEmpty(httpConfigureName))
            {
                var names = httpConfigureName.Split(',');
                if (names.Length == 2)
                {
                    var assembly = Assembly.Load(names[1].Trim());
                    var typeName = names[0].Substring(0, names[0].LastIndexOf('.')).Trim();
                    var methodName = names[0].Substring(typeName.Length + 1).Trim();
                    var type = assembly.GetType(typeName);
                    httpConfigure = type.GetMethod(methodName, BindingFlags.Static | BindingFlags.Public);
                }
                else
                {
                    var typeName = names[0].Substring(0, names[0].LastIndexOf('.')).Trim();
                    var methodName = names[0].Substring(typeName.Length + 1).Trim();
                    var type = Type.GetType(typeName);
                    httpConfigure = type.GetMethod(methodName, BindingFlags.Static | BindingFlags.Public);
                }
            }

            configuration.Routes.MapHttpRoute(
                "api default",
                "api/{controller}/{action}",
                new { action = RouteParameter.Optional });

            if (traceWriterType != null)
            {
                configuration.Services.Replace(typeof(ITraceWriter), Activator.CreateInstance(traceWriterType));
            }

            var httpServer = new HttpServer(configuration);
            configuration.SetHttpServer(httpServer);

            if (httpConfigure != null)
            {
                httpConfigure.Invoke(null, new object[] { configuration });
            }

            IHostBufferPolicySelector bufferPolicySelector = configuration.Services.GetHostBufferPolicySelector() ?? new OwinBufferPolicySelector();
            app.Use(typeof(HttpMessageHandlerAdapter), httpServer, bufferPolicySelector);
        }

        protected override bool InitializeServer(RunFrame frame)
        {
            var context = new WebHostContext()
            {
                TestType = TypeDescriptor,
                Deployment = new DeploymentDescriptor(TypeDescriptor.TestTypeInfo),
                Frame = frame
            };

            RetrieveSource(context);
            InitOptions(context);
            ConfigurationInitialize(context);

            if (context.DeploymentOptions != null)
            {
                context.DeploymentOptions.Deploy(context.Source);
            }

            if (context.HostOptions == null)
            {
                throw new NuwaHostSetupException("Host options is not specified.");
            }

            try
            {
                var baseAddress = context.HostOptions.Start().Replace("localhost", Environment.MachineName);
                frame.SetState(KeyBaseAddresss, baseAddress);
                frame.SetState(KeyWebHostContext, context);
            }
            catch (Exception e)
            {
                throw new NuwaHostSetupException("Failed to setup Host.", e);
            }

            return true;
        }

        protected override void ShutdownServer(RunFrame frame)
        {
            var context = frame.GetState(KeyWebHostContext) as WebHostContext;
            if (context != null)
            {
                try
                {
                    context.HostOptions.Stop();
                }
                finally
                {
                    ConfigurationTearDown(context);
                    context.HostOptions.Dispose();
                }
            }
            frame.SetState(KeyBaseAddresss, null);
        }

        protected virtual void RetrieveSource(WebHostContext context)
        {
            IDirectory source;
            switch (context.Deployment.DeploymentType)
            {
                case DeploymentType.Directory:
                    source = GetDiskSourceDirectory(context);
                    break;
                case DeploymentType.Assembly:
                case DeploymentType.Resource:
                    source = GetAssemblyAndResourceDirectory(context);
                    break;
                default:
                    throw new NotSupportedException(context.Deployment.DeploymentType.ToString());
            }

            context.Source = source;
        }

        protected abstract void InitOptions(WebHostContext context);

        private IDirectory GetDiskSourceDirectory(WebHostContext context)
        {
            var descriptor = context.Deployment;
            if (string.IsNullOrEmpty(descriptor.ScopePath))
            {
                throw new InvalidOperationException("Path can't be null or empty for directory deployment");
            }

            var solutionRootPath = GetSolutionDir();
            if (string.IsNullOrEmpty(solutionRootPath))
            {
                throw new InvalidOperationException(
                    string.Format(
                        "Configuration Setting:{0} can't be null or empty for directory deployment",
                        SolutionDirAppSettingName));
            }

            var sourceDirInfo = new DirectoryInfo(Path.Combine(solutionRootPath, descriptor.ScopePath));
            if (!sourceDirInfo.Exists)
            {
                throw new InvalidOperationException(
                    string.Format(
                        "Web app path:'{0}' doesn't exist",
                        sourceDirInfo.FullName));
            }

            var directory = new MemoryDirectory("root", null);
            directory.CopyFromDisk(sourceDirInfo);

            return directory;
        }


        private static string GetSolutionDir()
        {
            try
            {
                ExeConfigurationFileMap configFileMap = new ExeConfigurationFileMap()
                {
                    ExeConfigFilename = Assembly.GetExecutingAssembly().GetName().Name + ".dll.config"
                };
                Configuration config = ConfigurationManager.OpenMappedExeConfiguration(configFileMap, ConfigurationUserLevel.None);

                KeyValueConfigurationElement setting = config.AppSettings.Settings[SolutionDirAppSettingName];
                if (setting != null)
                {
                    return setting.Value;
                }
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private IDirectory GetAssemblyAndResourceDirectory(WebHostContext context)
        {
            var descriptor = context.TestType;
            var frame = context.Frame;
            var options = WebAppSetupOptions.GenerateDefaultOptions();

            options.AddWebApiAssemblies();
            options.AddRoute(
                new WebAPIRouteSetup(
                    "api default",
                    "api/{controller}/{action}",
                    "new { action = " + typeof(RouteParameter).FullName + ".Optional }"));

            var deploymentDescriptor = context.Deployment;
            if (deploymentDescriptor.ScopePath != null)
            {
                var resourceAssembly = deploymentDescriptor.ScopeResourceType != null ? deploymentDescriptor.ScopeResourceType.Assembly : descriptor.TestAssembly;
                options.AddAssemblyAndReferences(resourceAssembly);
                options.AddTextFilesFromResources(resourceAssembly, deploymentDescriptor.ScopePath);
            }

            if (options.TextFiles.ContainsKey("web.config"))
            {
                options.UpdateWebConfig(WebConfigHelper.Load(options.TextFiles["web.config"]));
            }

            if (!descriptor.TestControllerTypes.Any())
            {
                if (descriptor.TestAssembly == null)
                {
                    throw new InvalidOperationException(
                        "Neither Controller Types or test assembly is given to web-host strategy. " +
                        "That will cause issue in runtime, because the assemblies contain the controller " +
                        "will not be copied to IIS website bin folder. Please given Controller Type by " +
                        "NuwaControllerAttribute.");
                }

                options.AddAssemblyAndReferences(descriptor.TestAssembly);
            }
            else
            {
                foreach (var ct in descriptor.TestControllerTypes)
                {
                    options.AddAssemblyAndReferences(ct.Assembly);
                }
            }

            // set trace writer
            TraceElement traceElem = frame.GetFirstElement<TraceElement>();
            if (traceElem != null)
            {
                options.TraceWriterType = traceElem.TracerType;
            }

            // set configure action
            if (descriptor.ConfigureMethod != null)
            {
                options.ConfigureMethod = descriptor.ConfigureMethod;
                options.AddAssemblyAndReferences(options.ConfigureMethod.Module.Assembly);
            }

            // TODO: are they used in all situation?
            // setup katana integration pipeline
            if (descriptor.GetDesignatedMethod<NuwaKatanaConfigurationAttribute>() != null)
            {
                options.AddAssemblyAndReferences(Assembly.Load("Microsoft.Owin.Host.SystemWeb"));
                options.UpdateWebConfig(config =>
                {
                    var method = descriptor.GetDesignatedMethod<NuwaKatanaConfigurationAttribute>();
                    config.AddAppSection(
                        "owin:AppStartup",
                        string.Format("{0}.{1}, {2}",
                            method.DeclaringType.FullName,
                            method.Name,
                            method.DeclaringType.Assembly.GetName().Name));
                });
            }
            else if (EnableDefaultOwinWebApiConfiguration)
            {
                options.AddAssemblyAndReferences(Assembly.Load("Microsoft.Owin.Host.SystemWeb"));
                options.UpdateWebConfig(config =>
                {
                    config.AddAppSection(
                        "owin:AppStartup",
                        string.Format("{0}.{1}, {2}",
                            typeof(WebBaseHostElement).FullName,
                            "DefaultOwinWebApiConfiguration",
                            typeof(WebBaseHostElement).Assembly.GetName().Name));
                    if (options.TraceWriterType != null)
                    {
                        config.AddAppSection(
                            NuwaGlobalConfiguration.TraceWriterTypeKey,
                            string.Format("{0}, {1}",
                                options.TraceWriterType.FullName,
                                options.TraceWriterType.Assembly.GetName().Name));
                    }

                    if (options.ConfigureMethod != null)
                    {
                        config.AddAppSection(
                            NuwaGlobalConfiguration.HttpConfigureKey,
                            string.Format("{0}.{1}, {2}",
                                options.ConfigureMethod.DeclaringType.FullName,
                                options.ConfigureMethod.Name,
                                options.ConfigureMethod.DeclaringType.Assembly.GetName().Name));
                    }

                });
            }
            else
            {
                options.UpdateWebConfig(config =>
                {
                    config.AddAppSection(
                        "owin:AutomaticAppStartup",
                        "false");
                });
            }

            // update web.config
            if (descriptor.WebConfigMethod != null)
            {
                Action<WebConfigHelper> action = Delegate.CreateDelegate(
                    typeof(Action<WebConfigHelper>),
                    descriptor.WebConfigMethod)
                    as Action<WebConfigHelper>;

                if (action != null)
                {
                    options.UpdateWebConfig(action);
                }
            }

            // retrieve partial trust element
            options.UpdateWebConfig(webConfig => webConfig.ConfigureTrustLevel("Full"));

            var ramfarSetting = ConfigurationManager.AppSettings["runAllManagedModulesForAllRequests"];
            if (ramfarSetting != null && string.Equals(ramfarSetting, "true", StringComparison.InvariantCultureIgnoreCase))
            {
                options.UpdateWebConfig(webConfig => webConfig.AddRAMFAR(true));
            }

            // Update deployment options
            if (descriptor.WebDeployConfigMethod != null)
            {
                descriptor.WebDeployConfigMethod.Invoke(null, new object[] { options });
            }

            if (EnableGlobalAsax)
            {
                options.GenerateGlobalAsaxForCS();
            }

            doAdditionalAssemblyAndREferences();
            doAdditionalUpdateWebConfig();

            return options.ToDirectory();
        }

        protected virtual void doAdditionalUpdateWebConfig()
        {
            // do nothing
        }

        protected virtual void doAdditionalAssemblyAndREferences()
        {
            // do nothing
        }

        private void ConfigurationInitialize(WebHostContext context)
        {
            foreach (var configuration in GetWebConfigurations(context.TestType))
            {
                configuration.Initialize(context);
            }
        }

        private void ConfigurationTearDown(WebHostContext context)
        {
            foreach (var configuration in GetWebConfigurations(context.TestType).Reverse<IWebHostConfiguration>())
            {
                configuration.TearDown(context);
            }
        }

        private static IEnumerable<IWebHostConfiguration> GetWebConfigurations(TestTypeDescriptor descriptor)
        {
            var type = descriptor.TestTypeInfo.Type;

            var retval = type.GetCustomAttributes()
                             .Where(one => one is IWebHostConfiguration)
                             .OfType<IWebHostConfiguration>();

            return retval;
        }
    }
}
