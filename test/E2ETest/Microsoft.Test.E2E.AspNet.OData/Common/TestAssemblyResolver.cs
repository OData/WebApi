//-----------------------------------------------------------------------------
// <copyright file="TestAssemblyResolver.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

#if NETFX // This class is only used in the AspNet version.
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Web.Http.Dispatcher;

namespace Microsoft.Test.E2E.AspNet.OData.Common
{
    /// <summary>
    /// This class is used in AspNet to add controllers as an IAssembliesResolver for discovery.
    /// </summary>
    internal class TestAssemblyResolver : IAssembliesResolver
    {
        private List<Assembly> _assemblies;

        public TestAssemblyResolver(TypesInjectionAssembly assembly)
        {
            _assemblies = new List<Assembly>();
            _assemblies.Add(assembly);
        }

        public TestAssemblyResolver(params Type[] types)
            : this(new TypesInjectionAssembly(types))
        {
        }

        public ICollection<Assembly> GetAssemblies()
        {
            return _assemblies;
        }
    }
}
#endif
