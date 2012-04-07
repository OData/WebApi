// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;

namespace System.Web.WebPages
{
    // This class encapsulates the creation of objects from virtual paths.  The creation is either performed via BuildBanager API's, or
    // by using explicitly registered factories (which happens through ApplicationPart.Register).
    public class VirtualPathFactoryManager : IVirtualPathFactory
    {
        private static readonly Lazy<VirtualPathFactoryManager> _instance = new Lazy<VirtualPathFactoryManager>(() => new VirtualPathFactoryManager(new BuildManagerWrapper()));
        private readonly LinkedList<IVirtualPathFactory> _virtualPathFactories = new LinkedList<IVirtualPathFactory>();

        internal VirtualPathFactoryManager(IVirtualPathFactory defaultFactory)
        {
            _virtualPathFactories.AddFirst(defaultFactory);
        }

        // Get the VirtualPathFactoryManager singleton instance
        internal static VirtualPathFactoryManager Instance
        {
            get { return _instance.Value; }
        }

        internal IEnumerable<IVirtualPathFactory> RegisteredFactories
        {
            get { return _virtualPathFactories; }
        }

        public static void RegisterVirtualPathFactory(IVirtualPathFactory virtualPathFactory)
        {
            Instance.RegisterVirtualPathFactoryInternal(virtualPathFactory);
        }

        internal void RegisterVirtualPathFactoryInternal(IVirtualPathFactory virtualPathFactory)
        {
            _virtualPathFactories.AddBefore(_virtualPathFactories.Last, virtualPathFactory);
        }

        public bool Exists(string virtualPath)
        {
            return _virtualPathFactories.Any(factory => factory.Exists(virtualPath));
        }

        public object CreateInstance(string virtualPath)
        {
            return CreateInstanceOfType<object>(virtualPath);
        }

        internal T CreateInstanceOfType<T>(string virtualPath) where T : class
        {
            var virtualPathFactory = _virtualPathFactories.FirstOrDefault(f => f.Exists(virtualPath));
            if (virtualPathFactory != null)
            {
                return virtualPathFactory.CreateInstance<T>(virtualPath);
            }
            return null;
        }
    }
}
