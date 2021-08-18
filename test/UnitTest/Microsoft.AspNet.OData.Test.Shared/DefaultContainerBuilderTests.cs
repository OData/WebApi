//-----------------------------------------------------------------------------
// <copyright file="DefaultContainerBuilderTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData;
using Xunit;
using ServiceLifetime = Microsoft.OData.ServiceLifetime;

namespace Microsoft.AspNet.OData.Test
{
    public class DefaultContainerBuilderTests
    {
        [Fact]
        public void AddService_WithImplementationType()
        {
            IContainerBuilder builder = new DefaultContainerBuilder();
            builder.AddService<ITestService, TestService>(ServiceLifetime.Transient);
            IServiceProvider container = builder.BuildContainer();

            Assert.NotNull(container.GetService<ITestService>());
        }

        [Fact]
        public void AddService_WithImplementationFactory()
        {
            IContainerBuilder builder = new DefaultContainerBuilder();
            builder.AddService<ITestService>(ServiceLifetime.Transient, sp => new TestService());
            IServiceProvider container = builder.BuildContainer();

            Assert.NotNull(container.GetService<ITestService>());
        }

        [Fact]
        public void AddSingletonService_Works()
        {
            IContainerBuilder builder = new DefaultContainerBuilder();
            builder.AddService<ITestService, TestService>(ServiceLifetime.Singleton);
            IServiceProvider container = builder.BuildContainer();

            ITestService o1 = container.GetService<ITestService>();
            ITestService o2 = container.GetService<ITestService>();

            Assert.NotNull(o1);
            Assert.Equal(o1, o2);
        }

        [Fact]
        public void AddTransientService_Works()
        {
            IContainerBuilder builder = new DefaultContainerBuilder();
            builder.AddService<ITestService, TestService>(ServiceLifetime.Transient);
            IServiceProvider container = builder.BuildContainer();

            ITestService o1 = container.GetService<ITestService>();
            ITestService o2 = container.GetService<ITestService>();

            Assert.NotNull(o1);
            Assert.NotNull(o2);
            Assert.NotEqual(o1, o2);
        }

        [Fact]
        public void AddScopedService_Works()
        {
            IContainerBuilder builder = new DefaultContainerBuilder();
            builder.AddService<ITestService, TestService>(ServiceLifetime.Scoped);
            IServiceProvider container = builder.BuildContainer();

            IServiceProvider scopedContainer1 = container.GetRequiredService<IServiceScopeFactory>()
                .CreateScope().ServiceProvider;
            ITestService o11 = scopedContainer1.GetService<ITestService>();
            ITestService o12 = scopedContainer1.GetService<ITestService>();

            Assert.NotNull(o11);
            Assert.NotNull(o12);
            Assert.Equal(o11, o12);

            IServiceProvider scopedContainer2 = container.GetRequiredService<IServiceScopeFactory>()
                .CreateScope().ServiceProvider;
            ITestService o21 = scopedContainer2.GetService<ITestService>();
            ITestService o22 = scopedContainer2.GetService<ITestService>();

            Assert.NotNull(o21);
            Assert.NotNull(o22);
            Assert.Equal(o21, o22);

            Assert.NotEqual(o11, o21);
        }

        private interface ITestService { }

        private class TestService : ITestService { }
    }
}
