using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace WebStack.QA.Js.Utils
{
    public class ResourceLoader
    {
        public ResourceLoader()
        {
            LoadFrom = new List<Assembly>();
            DebugMode = false;
        }

        public List<Assembly> LoadFrom { get; set; }
        public bool DebugMode { get; set; }

        /// <summary>
        /// Read the resource as string content
        /// </summary>
        /// <param name="resourceName">Resource file name</param>
        /// <returns>The resource content in string format</returns>
        public Stream ReadAsStream(string resourceName)
        {
            var resource = GetResource(resourceName);
            return resource.ResourceStream;
        }

        /// <summary>
        /// Read the resource as string content
        /// </summary>
        /// <param name="type">Type that has the same namespace of the resource</param>
        /// <param name="resourceName">Resource relative name to the type</param>
        /// <returns>The resource content in string format</returns>
        public Stream ReadAsString(Type type, string resourceName)
        {
            return ReadAsStream(string.Format("{0}.{1}", type.Namespace, resourceName));
        }

        /// <summary>
        /// Save resource on file disk in the same folder as resource assembly
        /// </summary>
        /// <param name="resourceName">Resource file name</param>
        /// <returns>Saved file path</returns>
        public string SaveAsFile(string resourceName)
        {
            var resource = GetResource(resourceName);
            var path = Path.Combine(
                Path.GetDirectoryName(resource.ReferencedAssembly.Location),
                resource.ResourceName);
            if (DebugMode && File.Exists(path))
            {
                File.Delete(path);
            }

            if (!File.Exists(path))
            {
                using (Stream input = resource.ResourceStream)
                {
                    using (Stream output = File.OpenWrite(path))
                    {
                        input.CopyTo(output);
                    }
                }
            }

            return path;
        }

        /// <summary>
        /// Save resource on file disk in the same folder as resource assembly
        /// </summary>
        /// <param name="type">Type that has the same namespace of the resource</param>
        /// <param name="resourceName">Resource relative name to the type</param>
        /// <returns>Saved file path</returns>
        public string SaveAsFile(Type type, string resourceName)
        {
            return SaveAsFile(string.Format("{0}.{1}", type.Namespace, resourceName));
        }

        private ResourceInfo GetResource(string resourceName)
        {
            Stream resourceStream = null;
            foreach (var assembly in LoadFrom)
            {
                resourceStream = assembly.GetManifestResourceStream(resourceName);
                if (resourceStream != null)
                {
                    ResourceInfo resourceInfo = new ResourceInfo();
                    resourceInfo.ReferencedAssembly = assembly;
                    resourceInfo.ResourceStream = resourceStream;
                    resourceInfo.ResourceName = resourceName;
                    return resourceInfo;
                }
            }

            throw new FileNotFoundException(string.Format("{0} file is not found from LoadFrom assemblies.", resourceName));
        }

        public class ResourceInfo
        {
            public Assembly ReferencedAssembly { get; set; }
            public Stream ResourceStream { get; set; }
            public string ResourceName { get; set; }
        }
    }
}
