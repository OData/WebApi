using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Web.Http;
using System.Web.Http.Tracing;
using System.Web.Routing;

namespace WebStack.QA.Common.WebHost
{
    /// <summary>
    /// GlobalAsaxTemplate is used to generate Global.asax file in test runtime.
    /// The file is generated based on the T4 template.
    /// /// 
    /// Reference:
    /// http://msdn.microsoft.com/en-us/library/bb126445.aspx
    /// </summary>
    public partial class GlobalAsaxTemplate
    {
        /// <summary>
        /// all types refereed in global.asax
        /// </summary>
        private List<Type> _refTypes;

        /// <summary>
        /// Routes to be added
        /// </summary>
        private IEnumerable<AbstactRouteSetup> _routes;

        /// <summary>
        /// Trace writer
        /// </summary>
        private Type _traceWriterType;

        /// <summary>
        /// In addition configuration adjustment
        /// </summary>
        private MethodInfo _configureMethod;

        /// <summary>
        /// Constructs a template based on a given WebAppSetupOptions
        /// </summary>
        /// <param name="options"></param>
        public GlobalAsaxTemplate(WebAppSetupOptions options)
            : this(options.Routes, options.TraceWriterType, options.ConfigureMethod)
        {
        }

        /// <summary>
        /// Constructs a template based on a routes collection
        /// </summary>
        /// <param name="routes"></param>
        public GlobalAsaxTemplate(IEnumerable<AbstactRouteSetup> routes)
            : this(routes, null, null)
        {
        }

        /// <summary>
        /// Constructs a template based on routes, trace writer and configuration adjustment
        /// </summary>
        /// <param name="routes"></param>
        /// <param name="trace"></param>
        /// <param name="config"></param>
        public GlobalAsaxTemplate(IEnumerable<AbstactRouteSetup> routes,
                                  Type trace,
                                  MethodInfo config)
        {
            this._routes = routes;
            this._traceWriterType = trace;
            this._configureMethod = config;

            this._refTypes = new List<Type>();
            _refTypes.Add(typeof(RouteCollection));
            _refTypes.Add(typeof(GlobalConfiguration));
            _refTypes.Add(typeof(ITraceWriter));
            _refTypes.Add(typeof(RouteCollectionExtensions));

            if (this._traceWriterType != null)
            {
                _refTypes.Add(this._traceWriterType);
            }
        }

        /// <summary>
        /// Returns a non-duplicate namespace set of all refereed type
        /// </summary>
        private HashSet<string> Namespaces
        {
            get
            {
                var retval = new HashSet<string>();

                foreach (var type in _refTypes)
                {
                    retval.Add(type.Namespace);
                }

                return retval;
            }
        }

        /// <summary>
        /// Transform the template and save to given path
        /// </summary>
        /// <param name="path">path to the save file</param>
        public void TransformAndSave(string path)
        {
            File.WriteAllText(path, this.TransformText());
        }
    }
}