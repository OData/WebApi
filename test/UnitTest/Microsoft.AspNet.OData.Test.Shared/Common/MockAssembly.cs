//-----------------------------------------------------------------------------
// <copyright file="MockAssembly.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Microsoft.AspNet.OData.Test.Common
{
    public sealed class MockAssembly : Assembly
    {
        Type[] _types;

        public MockAssembly(params Type[] types)
        {
            _types = types;
        }

        public MockAssembly(params MockType[] types)
        {
            foreach (var type in types)
            {
                type.SetupGet(t => t.Assembly).Returns(this);
            }
            _types = types.Select(t => t.Object).ToArray();
        }

        /// <remarks>
        /// AspNet uses GetTypes as opposed to DefinedTypes()
        /// </remarks>
        public override Type[] GetTypes()
        {
            return _types;
        }

        /// <remarks>
        /// AspNetCore uses DefinedTypes as opposed to GetTypes()
        /// </remarks>
        public override IEnumerable<TypeInfo> DefinedTypes
        {
            get { return _types.AsEnumerable().Select(a => a.GetTypeInfo()); }
        }
    }
}
