// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

#if NETCORE
using System;
using System.Reflection;

namespace Microsoft.Test.AspNet.OData.TestCommon
{
    /// <summary>
    /// This class is used in AspNetCore to add controllers as an assembly part for discovery.
    /// </summary>
    internal sealed class TestAssembly : Assembly
    {
        Type[] _types;

        public TestAssembly(params Type[] types)
        {
            _types = types;
        }

        public override Type[] GetTypes()
        {
            return _types;
        }
    }
}
#endif
