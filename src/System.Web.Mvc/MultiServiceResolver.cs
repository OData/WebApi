// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;

namespace System.Web.Mvc
{
    internal static class MultiServiceResolver        
    {
        internal static TService[] GetCombined<TService>(IList<TService> items, IDependencyResolver resolver = null) where TService : class
        {           
            if (resolver == null)
            {
                resolver = DependencyResolver.Current;
            }
            IEnumerable<TService> services = resolver.GetServices<TService>();
            return services.Concat(items).ToArray();
        } 
    }
}
