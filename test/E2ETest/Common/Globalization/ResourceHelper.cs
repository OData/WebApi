using System.Reflection;
using System.Resources;

namespace WebStack.QA.Common.Globalization
{
    public static class ResourceHelper
    {
        public static string GetResourceString(
            Assembly assembly, string resourceName, string resourceKey, params object[] args)
        {
            ResourceManager resManager = new ResourceManager(resourceName, assembly);
            string resource = resManager.GetString(resourceKey);

            if (args != null && args.Length > 0)
            {
                // TODO check if resource is null
                resource = string.Format(resource, args);
            }

            return resource;
        }
    }
}