//-----------------------------------------------------------------------------
// <copyright file="TypesInjectionAssembly.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Reflection;

namespace Microsoft.Test.E2E.AspNet.OData.Common
{
    internal sealed class TypesInjectionAssembly : Assembly
    {
        private Type[] _types;

        public TypesInjectionAssembly(params Type[] types)
        {
            _types = types;
        }

        public override Type[] GetTypes()
        {
            return _types;
        }
    }
}
