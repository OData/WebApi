using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Reflection;
using System.Resources;
using System.Web.Http;
using System.Web.Http.Tracing;
using WebStack.QA.Common.FileSystem;

namespace WebStack.QA.Common.WebHost
{
    /// <summary>
    /// Setup options of Website
    /// </summary>
    public class WebAppSetupOptions
    {
        private string _virtualDirectory;
        private Type _typeTraceWriter;
        private MethodInfo _configureMethod;
        private List<AbstactRouteSetup> _routesOpts;
        private WebConfigHelper _webConfig;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebAppSetupOptions"/> class. 
        /// </summary>
        public WebAppSetupOptions()
        {
            this.TextFiles = new Dictionary<string, string>();
            this.BinaryFiles = new List<string>();
            this._routesOpts = new List<AbstactRouteSetup>();
            this._webConfig = null;
        }

        /// <summary>
        /// The list of the binary files need to be copied to the bin folder
        /// </summary>
        public List<string> BinaryFiles { get; set; }

        /// <summary>
        /// The action of adjusting the HttpConfiguration addtionally.
        /// 
        /// The given method reference must point to a static, public method which accept one 
        /// HttpConfiguration instnace as parameter. Please be noted that the method is excuted
        /// in a different AppDomain other than the one running test cases (under Web-Host test
        /// the server part is in IIS process, while the test codes is running in xUnit process
        /// play as client.) which means any external variables used in the method probably is 
        /// not there.
        /// </summary>
        public Action<HttpConfiguration> Configuration
        {
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("ConfigureAction");
                }

                if (value.Target != null)
                {
                    throw new ArgumentException(
                        "Configuration function must be a public static method, with a single parameter of type HttpConfiguration.");
                }

                ConfigureMethod = value.Method;
            }
        }

        /// <summary>
        /// The method info of the method which will adjust the HttpConfiguration addtionally.
        /// 
        /// The given method reference must point to a static, public method which accept one 
        /// HttpConfiguration instnace as parameter. Please be noted that the method is excuted
        /// in a different AppDomain other than the one running test cases (under Web-Host test
        /// the server part is in IIS process, while the test codes is running in xUnit process
        /// play as client.) which means any external variables used in the method probably is 
        /// not there.
        /// </summary>
        public MethodInfo ConfigureMethod
        {
            get
            {
                return _configureMethod;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("ConfigureMethod");
                }

                if (!value.IsStatic || !value.IsPublic || value.GetParameters().Length != 1 || value.GetParameters()[0].ParameterType != typeof(HttpConfiguration))
                {
                    throw new ArgumentException(
                        "Configuration function must be a public static method, with a single parameter of type HttpConfiguration.");
                }

                this._configureMethod = value;
            }
        }

        public List<AbstactRouteSetup> Routes
        {
            get { return _routesOpts; }
        }

        /// <summary>
        /// The type of the trace writer to be set.
        /// 
        /// This property can be null, which means no trace is to set. Therefore no codes will
        /// be added to global.aspx.
        /// 
        /// The given type must implement ITraceWriter, of course.
        /// 
        /// The TraceWriter must have a default constructor (without parameters), because 
        /// global.aspx is running in an AppDomain other than the one test is running in, so
        /// that parameters can't be feed.
        /// 
        /// The dll contains the trace writer will be also copied to bin folder.
        /// </summary>
        public Type TraceWriterType
        {
            get { return _typeTraceWriter; }
            set
            {
                if (value == null)
                {
                    _typeTraceWriter = null;
                    return;
                }

                if (value.GetInterface(typeof(ITraceWriter).FullName) == null)
                {
                    throw new ArgumentException("The given type " + value.FullName + " doesn't implement " + typeof(ITraceWriter).FullName);
                }

                var defaultCtor = value.GetConstructor(new Type[] { });
                if (defaultCtor == null)
                {
                    throw new ArgumentException("The given type " + value.FullName + " doesn't have a default constructor.");
                }

                _typeTraceWriter = value;
                AddAssemblyAndReferences(_typeTraceWriter.Assembly);
            }
        }

        /// <summary>
        /// Text files used in setting up iis web site. these files are usually
        /// configuration files such as web.config and global.aspx
        /// 
        /// The key of dictionary is the file name.
        /// The value is the content of the file.
        /// 
        /// REVIEW: what if files encoding in different code page?
        /// </summary>
        public Dictionary<string, string> TextFiles { get; set; }

        /// <summary>
        /// Virual directory for web application
        /// </summary>
        public string VirtualDirectory
        {
            get
            {
                if (string.IsNullOrEmpty(this._virtualDirectory))
                {
                    this._virtualDirectory = Guid.NewGuid().ToString("N");
                }

                return this._virtualDirectory;
            }

            set
            {
                this._virtualDirectory = value;
            }
        }

        /// <summary>
        /// Generate a default setup option.
        /// </summary>
        /// <returns>
        /// default iis setting options
        /// </returns>
        public static WebAppSetupOptions GenerateDefaultOptions()
        {
            WebAppSetupOptions result = new WebAppSetupOptions();

            result.UpdateWebConfig(CreateDefaultWebConfig());

            return result;
        }

        /// <summary>
        /// Add a route setup option. Please be noted that it is not going to add
        /// a route right way. The set up will be traslated into codes in global.asax 
        /// file which is placed under bin folder. IIS will consume the global.asax.
        /// </summary>
        public void AddRoute(AbstactRouteSetup route)
        {
            this._routesOpts.Add(route);
        }

        public void AddTextFileFromFolder(string path, SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            if (searchOption != SearchOption.TopDirectoryOnly)
            {
                throw new NotImplementedException("Copying recursively not implemented yet");
            }

            foreach (var file in Directory.GetFiles(path, "*", searchOption))
            {
                this.TextFiles.Add(Path.GetFileName(file), File.ReadAllText(file));
            }
        }

        /// <summary>
        /// Add text files from assembly resources under specified scope.
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="scope"></param>
        public void AddTextFilesFromResources(Assembly assembly, string scope)
        {
            Contract.Assert(assembly != null, "assembly is required to have a value.");
            Contract.Assert(scope != null, "scope is required to have a value.");

            scope = scope.ToLower();
            string resourceName = string.Format("{0}.g.resources", assembly.GetName().Name);

            Stream resourceStream = assembly.GetManifestResourceStream(resourceName);
            if (resourceStream == null)
            {
                throw new FileNotFoundException(string.Format("Resource file {0} is not found from assembly {1}",
                    resourceName, assembly.GetName().Name));
            }

            using (ResourceReader rr = new ResourceReader(resourceStream))
            {
                var filesToDeploy = rr.Cast<DictionaryEntry>().Where(entry =>
                     {
                         string key = entry.Key as string;
                         if (string.IsNullOrEmpty(key))
                         {
                             return false;
                         }

                         return key.StartsWith(scope) && (entry.Value is Stream);
                     }).Select(entry =>
                         {
                             string key = entry.Key as string;
                             key = key.Substring(scope.Length);
                             if (key.StartsWith("/"))
                             {
                                 key = key.Substring(1);
                             }
                             return Tuple.Create<string, Stream>(key, entry.Value as Stream);
                         });
                if (filesToDeploy.Count() == 0)
                {
                    throw new FileNotFoundException(string.Format("No file is found under path {0}", scope));
                }

                foreach (var fileEntry in filesToDeploy)
                {
                    using (StreamReader sr = new StreamReader(fileEntry.Item2))
                    {
                        if (this.TextFiles.ContainsKey(fileEntry.Item1))
                        {
                            this.TextFiles[fileEntry.Item1] = sr.ReadToEnd();
                        }
                        else
                        {
                            this.TextFiles.Add(fileEntry.Item1, sr.ReadToEnd());
                        }
                    }
                }
            }
        }

        /// <summary>
        /// This method should contain the assemblies which will be used for website 
        /// to build the pages. Even the assembly is in the GAC, asp.net still need 
        /// that assembly in bin folder to build the page or global.asax
        /// </summary>
        public void AddWebApiAssemblies()
        {
            Type[] webApiAssemblyTypes = new Type[]
            {
                typeof(HttpRequestMessage),  // System.Net.Http.dll
                typeof(MediaTypeFormatter),  // System.Net.Http.Formatting.dll
                typeof(ApiController),       // System.Web.Http.dll
                typeof(GlobalConfiguration), // System.Web.Http.WebHost.dll
            };

            foreach (Type type in webApiAssemblyTypes)
            {
                Assembly assembly = type.Assembly;
                AddAssemblyAndReferences(assembly);
            }
        }


        public void AddAllAssembliesInBinFolder()
        {
            DirectoryInfo di = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            foreach (var file in di.GetFiles("*.dll"))
            {
                this.BinaryFiles.Add(file.FullName);
            }
        }

        /// <summary>
        /// This method adds the assembly and its references to BinaryFiles list
        /// recursively. It won't copy assembly in GAC or already in the BinaryFiles
        /// list.
        /// </summary>
        public void AddAssemblyAndReferences(Assembly assembly)
        {
            if (!this.BinaryFiles.Contains(assembly.Location))
            {
                this.BinaryFiles.Add(assembly.Location);
                var references = assembly.GetReferencedAssemblies();
                if (references != null)
                {
                    foreach (var reference in references)
                    {
                        try
                        {
                            var refAssembly = Assembly.Load(reference);
                            if (refAssembly.GlobalAssemblyCache)
                            {
                                continue;
                            }

                            AddAssemblyAndReferences(refAssembly);
                        }
                        catch (FileNotFoundException)
                        {
                            Console.WriteLine("Reference {0} couldn't be found.", reference.FullName);
                        }
                        catch (FileLoadException)
                        {
                            Console.WriteLine("Reference {0} couldn't be loaded.", reference.FullName);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Create the global.asax file
        /// </summary>
        public void GenerateGlobalAsaxForCS()
        {
            var template = new GlobalAsaxTemplate(this);

            this.TextFiles.Add("global.asax", template.TransformText());
        }

        public bool NeedsGlobalAsax()
        {
            return this._routesOpts.Count > 0 || this.ConfigureMethod != null || this.TraceWriterType != null;
        }

        /// <summary>
        /// Update the WebConfig file.
        /// 
        /// Accept a action of update XElement representing the WebConfig. If there is no 
        /// web.config exists by the time this method is called, a default web.config xml dom 
        /// will be created. After update action is excuted the web.config represent in
        /// TextFiles will be updated.
        /// 
        /// If the action is null, no exception will be thrown but the web.config will be 
        /// always updated.
        /// </summary>
        public void UpdateWebConfig(Action<WebConfigHelper> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }

            if (_webConfig == null)
            {
                _webConfig = CreateDefaultWebConfig();
            }

            action(this._webConfig);
            this.TextFiles["web.config"] = _webConfig.ToString();
        }

        /// <summary>
        /// Update the WebConfig file
        /// 
        /// Accecpt a WebConfigHelper instance to replace the exist one. TextFiles 
        /// dictionary will be updated accordingly.
        /// </summary>
        public void UpdateWebConfig(WebConfigHelper webConfig)
        {
            if (webConfig == null)
            {
                throw new ArgumentNullException("webConfig");
            }

            this._webConfig = webConfig;
            this.TextFiles["web.config"] = _webConfig.ToString();
        }

        private static WebConfigHelper CreateDefaultWebConfig()
        {
            return WebConfigHelper.New().AddTargetFramework("4.0").AddExtensionlessUrlHandlers();
        }

        public static string GenericTypeFullNameToString(Type type)
        {
            if (!type.IsGenericType)
            {
                return type.FullName;
            }

            string value = type.FullName.Substring(0, type.FullName.IndexOf('`')) + "<";
            Type[] genericArgs = type.GetGenericArguments();
            List<string> list = new List<string>();

            for (int i = 0; i < genericArgs.Length; i++)
            {
                value += "{" + i + "},";
                string s = GenericTypeFullNameToString(genericArgs[i]);
                list.Add(s);
            }

            value = value.TrimEnd(',');
            value += ">";
            value = string.Format(value, list.ToArray());

            return value;
        }

        // TODO: convert this class to extension methods for IDirectory
        /// <summary>
        /// This method is used to convert legacy code to IDirectory source.
        /// </summary>
        /// <returns></returns>
        public IDirectory ToDirectory()
        {
            var root = new MemoryDirectory("root", null);
            var bin = root.CreateDirectory("bin");

            // generated all text based files. they are mostly configuration files.                
            if (TextFiles != null)
            {
                foreach (var textFileName in TextFiles.Keys)
                {
                    root.CreateFileFromText(textFileName, TextFiles[textFileName]);
                }
            }

            var currentAssemblyLocation = Assembly.GetExecutingAssembly().Location;
            if (!BinaryFiles.Contains(currentAssemblyLocation))
            {
                BinaryFiles.Add(currentAssemblyLocation);
            }

            // copy all required binary files
            if (BinaryFiles != null)
            {
                foreach (var file in BinaryFiles)
                {
                    var fileInfo = new FileInfo(file);
                    if (fileInfo.Exists)
                    {
                        bin.CreateFileFromDisk(fileInfo.Name, fileInfo);
                    }
                }
            }

            return root;
        }
    }
}
