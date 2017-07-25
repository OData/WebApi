using System;
using System.Globalization;
using System.Xml.Linq;

namespace WebStack.QA.Common.WebHost
{
    /// <summary>
    /// Represents the a web.config file content in xml
    /// </summary>
    public class WebConfigHelper
    {
        private XElement _configElement;

        private WebConfigHelper()
            : this("<configuration/>")
        {
        }

        private WebConfigHelper(string webConfig)
        {
            _configElement = XElement.Parse(webConfig);
        }

        private WebConfigHelper(XElement configElement)
        {
            // make a copy
            _configElement = XElement.Parse(configElement.ToString());
        }

        public XElement XElement
        {
            get { return _configElement; }
        }

        public static WebConfigHelper Load(string webConfig)
        {
            return new WebConfigHelper(webConfig);
        }

        public static WebConfigHelper New()
        {
            return new WebConfigHelper();
        }

        public WebConfigHelper ConfigureTrustLevel(string trustLevel)
        {
            _configElement.EnsureElement("system.web").EnsureElement("trust").EnsureAttribute("level", trustLevel);

            return this;
        }

        public WebConfigHelper ConfigureAuthenticationMode(string mode)
        {
            _configElement.EnsureElement("system.web").EnsureElement("authentication").EnsureAttribute("mode", mode);

            return this;
        }

        public WebConfigHelper ConfigureCustomError(string mode)
        {
            _configElement.EnsureElement("system.web").EnsureElement("customErrors").EnsureAttribute("mode", mode);

            return this;
        }

        /// <summary>
        /// Helper to add 'runAllManagedModulesForAllRequests' flag to 'system.webServer/modules' section
        /// </summary>
        /// <param name="runAllManagedModules"></param>
        /// <returns></returns>
        public WebConfigHelper AddRAMFAR(bool runAllManagedModules)
        {
            _configElement.EnsureElement("system.webServer").EnsureElement("modules").EnsureAttribute(
                "runAllManagedModulesForAllRequests", runAllManagedModules.ToString().ToLowerInvariant());

            return this;
        }

        public WebConfigHelper AddExtensionlessUrlHandlers()
        {
            var handlers = _configElement.EnsureElement("system.webServer").EnsureElement("handlers");
            handlers.RemoveAll();
            handlers.Add(new XElement("remove", new XAttribute("name", "ExtensionlessUrlHandler-Integrated-4.0")));
            handlers.Add(new XElement("remove", new XAttribute("name", "OPTIONSVerbHandler")));
            handlers.Add(new XElement("remove", new XAttribute("name", "TRACEVerbHandler")));
            handlers.Add(new XElement("add", new XAttribute("name", "ExtensionlessUrlHandler-Integrated-4.0"),
                new XAttribute("path", "*"), new XAttribute("verb", "*"),
                new XAttribute("type", "System.Web.Handlers.TransferRequestHandler"),
                new XAttribute("preCondition", "integratedMode,runtimeVersionv4.0")));

            return this;
        }

        public WebConfigHelper AddTargetFramework(string targetFramework)
        {
            _configElement.EnsureElement("system.web").EnsureElement("compilation").EnsureAttribute(
                "targetFramework", targetFramework);
            _configElement.EnsureElement("system.web").EnsureElement("httpRuntime").EnsureAttribute(
               "targetFramework", targetFramework).EnsureAttribute("requestPathInvalidCharacters", "");

            return this;
        }

        public WebConfigHelper AddAppSection(string key, string value)
        {
            _configElement.EnsureElement("appSettings").EnsureElementWithKeyValueAttribute("add", key, value);

            return this;
        }
        public WebConfigHelper ConfigureAllowDoubleEscaping()
        {
            _configElement.EnsureElement("system.webServer").EnsureElement("security").EnsureElement(
                   "requestFiltering").EnsureAttribute("allowDoubleEscaping", "true");
            return this;
        }

        public WebConfigHelper ConfigureHttpRuntime(
            int? maxQueryStringLength = null, int? maxUrlLength = null, int? maxRequestLength = null)
        {
            XElement httpRuntime = _configElement.EnsureElement("system.web").EnsureElement("httpRuntime");

            XElement requestLimits =
                _configElement.EnsureElement("system.webServer").EnsureElement("security").EnsureElement(
                    "requestFiltering").EnsureElement("requestLimits");

            if (maxQueryStringLength.HasValue)
            {
                string value = maxQueryStringLength.Value.ToString(CultureInfo.InvariantCulture);
                httpRuntime.EnsureAttribute("maxQueryStringLength", value);
                requestLimits.EnsureAttribute("maxQueryString", value);
            }

            if (maxUrlLength.HasValue)
            {
                string value = maxUrlLength.Value.ToString(CultureInfo.InvariantCulture);
                httpRuntime.EnsureAttribute("maxUrlLength", value);
                requestLimits.EnsureAttribute("maxUrl", value);
            }

            if (maxRequestLength.HasValue)
            {
                string value = maxRequestLength.Value.ToString(CultureInfo.InvariantCulture);
                httpRuntime.EnsureAttribute("maxRequestLength", value);
                requestLimits.EnsureAttribute("maxAllowedContentLength", value);
            }

            return this;
        }

        public WebConfigHelper ClearConnectionStrings()
        {
            _configElement.EnsureElement("connectionStrings").RemoveNodes();
            return this;
        }

        public WebConfigHelper AddConnectionString(string name, string connectionString, string providerName)
        {
            var add = _configElement.EnsureElement("connectionStrings").EnsureElement("add");
            add.EnsureAttribute("name", name);
            add.EnsureAttribute("connectionString", connectionString);
            add.EnsureAttribute("providerName", providerName);

            return this;
        }

        public WebConfigHelper ReplaceWithDefaultMVC5Config()
        {
            _configElement = XElement.Parse(@"<?xml version=""1.0"" encoding=""utf-8""?>
<!--
  For more information on how to configure your ASP.NET application, please visit
  http://go.microsoft.com/fwlink/?LinkId=169433
  -->
<configuration>
  <appSettings>
    <add key=""webpages:Version"" value=""3.0.0.0"" />
    <add key=""webpages:Enabled"" value=""false"" />
    <add key=""PreserveLoginUrl"" value=""true"" />
    <add key=""ClientValidationEnabled"" value=""true"" />
    <add key=""UnobtrusiveJavaScriptEnabled"" value=""true"" />
  </appSettings>
  <system.web>
    <compilation debug=""true"" targetFramework=""4.5"" />
    <httpRuntime targetFramework=""4.5"" />
    <pages>
      <namespaces>
        <add namespace=""System.Web.Helpers"" />
        <add namespace=""System.Web.Mvc"" />
        <add namespace=""System.Web.Mvc.Ajax"" />
        <add namespace=""System.Web.Mvc.Html"" />
        <add namespace=""System.Web.Routing"" />
        <add namespace=""System.Web.WebPages"" />
      </namespaces>     
    </pages>
  </system.web>
  <system.webServer>
    <validation validateIntegratedModeConfiguration=""false"" />
    <handlers>
      <remove name=""ExtensionlessUrlHandler-Integrated-4.0"" />
      <remove name=""OPTIONSVerbHandler"" />
      <remove name=""TRACEVerbHandler"" />
      <add name=""ExtensionlessUrlHandler-Integrated-4.0"" path=""*."" verb=""*"" type=""System.Web.Handlers.TransferRequestHandler"" preCondition=""integratedMode,runtimeVersionv4.0"" />
    </handlers>
  </system.webServer>
</configuration>");
            return this;
        }

        public override string ToString()
        {
            return _configElement.ToString();
        }

        public WebConfigHelper AddAssemblyRedirection(string name, string publicKeyToken, string oldVersionRange, string newVersion)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (publicKeyToken == null)
            {
                throw new ArgumentNullException("publicKeyToken");
            }

            if (oldVersionRange == null)
            {
                throw new ArgumentNullException("oldVersionRange");
            }

            if (newVersion == null)
            {
                throw new ArgumentNullException("newVersion");
            }

            string dependentAssemblyTemplate = @"<dependentAssembly xmlns=""urn:schemas-microsoft-com:asm.v1"">
                                                    <assemblyIdentity name=""{0}"" publicKeyToken=""{1}"" />
                                                    <bindingRedirect oldVersion=""{2}"" newVersion=""{3}"" />
                                                 </dependentAssembly>";
            XElement runtime = _configElement.EnsureElement("runtime");
            XElement assemblyBinding = runtime.Element("assemblyBinding");
            if (assemblyBinding == null)
            {
                assemblyBinding = XElement.Parse(@"<assemblyBinding xmlns=""urn:schemas-microsoft-com:asm.v1""></assemblyBinding>");
                runtime.Add(assemblyBinding);
            }
            XElement bindingRedirect = XElement.Parse(string.Format(dependentAssemblyTemplate, name, publicKeyToken, oldVersionRange, newVersion));
            assemblyBinding.Add(bindingRedirect);
            return this;
        }
    }
}