// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Reflection;
using System.Web.Http.Services;
using Microsoft.TestCommon;
using Xunit;

namespace System.Web.Http.Dispatcher
{
    public class DefaultAssembliesResolverTest
    {
        [Fact]
        public void Constructor()
        {
            AssertEx.NotNull(new DefaultAssembliesResolver());
        }

        [Fact]
        public void GetAssemblies_ContainsCurrentAssembly()
        {
            IAssembliesResolver ar = new DefaultAssembliesResolver();
            Assembly currentAssembly = typeof (DefaultAssembliesResolverTest).Assembly;

            AssertEx.True(ar.GetAssemblies().Contains(currentAssembly));
        }

        [Fact]
        public void Class_IsDefaultIAssembliesResolver()
        {
            var serviceResolver = new DefaultServices(new HttpConfiguration());

            Assert.IsType<DefaultAssembliesResolver>(serviceResolver.GetService(typeof(IAssembliesResolver)));
        }
    }
}
