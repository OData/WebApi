// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Web.Http;
using System.Web.Http.Dispatcher;
using System.Web.OData.TestCommon;

namespace System.Web.OData
{
    internal static class TypeExtensions
    {
        public static HttpConfiguration GetHttpConfiguration(this Type[] controllers)
        {
            var resolver = new TestAssemblyResolver(new MockAssembly(controllers));
            var configuration = new HttpConfiguration();
            configuration.Services.Replace(typeof(IAssembliesResolver), resolver);
            return configuration;
        }
    }
}
