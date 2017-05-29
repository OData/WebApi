using System;
using System.Web.Http;

namespace WebStack.QA.Common.WebHost
{
    public class BasicWebAPIOptionsProvider<TController> : IWebAppSetupOptionsProvider
    {
        public virtual WebAppSetupOptions GetSetupOptions()
        {
            var appSetupOptions = WebAppSetupOptions.GenerateDefaultOptions();
            appSetupOptions.AddWebApiAssemblies();
            appSetupOptions.AddAssemblyAndReferences(typeof(TController).Assembly);
            return appSetupOptions;
        }
    }

    public class SingleControllerWebAPIOptionsProvider<TController> : BasicWebAPIOptionsProvider<TController>
    {        
        public override WebAppSetupOptions GetSetupOptions()
        {
            var appSetupOptions = base.GetSetupOptions();

            string className = typeof(TController).Name;
            var index = className.LastIndexOf("Controller", StringComparison.OrdinalIgnoreCase);
            string controller = string.Empty;
            if (index > 0)
            {
                controller = className.Substring(0, index);
            }
            else
            {                
                throw new ArgumentException("Supplied type is not a valid Controller type.");
            }

            appSetupOptions.AddRoute(
               new WebAPIRouteSetup(
                   "Default",
                   "{controller}/{action}",
                   "new { controller = \"" + controller + "\", action = " + typeof(RouteParameter).FullName
                   + ".Optional }"));

            return appSetupOptions;
        }
    } 
}
