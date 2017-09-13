// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Web.Http.Dispatcher;

namespace WebStack.QA.Test.OData.Common
{
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
