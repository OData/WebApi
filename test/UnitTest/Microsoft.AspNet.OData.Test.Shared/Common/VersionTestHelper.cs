// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Reflection;
using Xunit;

namespace Microsoft.Test.AspNet.OData.Common
{
    public static class VersionTestHelper
    {
        // returns a version for an assembly by using a type from the assembly
        // also verifies that type wasn't moved to another assembly.
        public static Version GetVersionFromAssembly(string assemblyName, Type typeFromAssembly)
        {
            Assembly assembly = typeFromAssembly.Assembly;

            Assert.Equal(assemblyName, assembly.GetName().Name);
            return assembly.GetName().Version;
        }
    }
}
