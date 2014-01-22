// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Reflection;
using System.Web.Http.Dispatcher;

namespace System.Web.OData.TestCommon
{
    internal class TestAssemblyResolver : IAssembliesResolver
    {
        List<Assembly> _assemblies;

        public TestAssemblyResolver(MockAssembly assembly)
        {
            _assemblies = new List<Assembly>();
            _assemblies.Add(assembly);
        }

        public TestAssemblyResolver(params Type[] types)
            : this(new MockAssembly(types))
        {
        }

        public ICollection<Assembly> GetAssemblies()
        {
            return _assemblies;
        }
    }
}
