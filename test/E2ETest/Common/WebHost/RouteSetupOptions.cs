using System;
using System.Reflection;

namespace WebStack.QA.Common.WebHost
{
    /// <summary>
    /// AbstractRouteSetup represents a route setup configuration. It
    /// is the base class to two derivation, MVC and Web RouteSetup.
    /// </summary>
    public abstract class AbstactRouteSetup
    {
        public AbstactRouteSetup(string name, string path, string defaults)
        {
            this.Name = name;
            this.Path = path;
            this.Defaults = defaults;
        }

        public string Defaults { get; private set; }

        public string Name { get; private set; }

        public string Path { get; private set; }

        /// <summary>
        /// Return the string of a route mapping function call. The 
        /// string will be used in T4 template. The string is in the
        /// form list RouteCollectionExtensions.MapHttpRoute.
        /// 
        /// The method string is difference in MVC and WebAPI mapping.
        /// </summary>
        public abstract string RouteMapFunctionCall { get; }
    }

    public class WebAPIRouteSetup : AbstactRouteSetup
    {
        private const string MethodName = "MapHttpRoute";

        public WebAPIRouteSetup(string name, string path, string defaults)
            : base(name, path, defaults)
        {
        }

        public override string RouteMapFunctionCall
        {
            get
            {
                Type target = typeof(System.Web.Http.RouteCollectionExtensions);
                if (target == null)
                {
                    throw new InvalidOperationException("Can't find RouteCollectionExtensions");
                }

                MethodInfo method = target.GetMethod(
                    MethodName,
                    new Type[]
                    {
                        typeof(System.Web.Routing.RouteCollection),
                        typeof(string),
                        typeof(string),
                        typeof(object)
                    });

                if (method == null)
                {
                    throw new InvalidOperationException("Can't find MapHttpRoute method");
                }

                string instanceName = target.FullName;

                return string.Format("{0}.{1}", instanceName, method.Name);
            }
        }
    }
}