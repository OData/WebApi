// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData;
using Microsoft.TestCommon;
using ServiceLifetime = Microsoft.OData.ServiceLifetime;

namespace System.Web.OData
{
    public class DefaultContainerBuilderTests
    {
        [Fact]
        public void AddService_WithImplementationType()
        {
            IContainerBuilder builder = new DefaultContainerBuilder();
            builder.AddService<IFoo, Foo>(ServiceLifetime.Transient);
            IServiceProvider container = builder.BuildContainer();

            Assert.NotNull(container.GetService<IFoo>());
        }

        [Fact]
        public void AddService_WithImplementationFactory()
        {
            IContainerBuilder builder = new DefaultContainerBuilder();
            builder.AddService<IFoo>(ServiceLifetime.Transient, sp => new Foo());
            IServiceProvider container = builder.BuildContainer();

            Assert.NotNull(container.GetService<IFoo>());
        }

        [Fact]
        public void AddSingletonService_Works()
        {
            IContainerBuilder builder = new DefaultContainerBuilder();
            builder.AddService<IFoo, Foo>(ServiceLifetime.Singleton);
            IServiceProvider container = builder.BuildContainer();

            IFoo foo1 = container.GetService<IFoo>();
            IFoo foo2 = container.GetService<IFoo>();

            Assert.NotNull(foo1);
            Assert.Equal(foo1, foo2);
        }

        [Fact]
        public void AddTransientService_Works()
        {
            IContainerBuilder builder = new DefaultContainerBuilder();
            builder.AddService<IFoo, Foo>(ServiceLifetime.Transient);
            IServiceProvider container = builder.BuildContainer();

            IFoo foo1 = container.GetService<IFoo>();
            IFoo foo2 = container.GetService<IFoo>();

            Assert.NotNull(foo1);
            Assert.NotNull(foo2);
            Assert.NotEqual(foo1, foo2);
        }

        [Fact]
        public void AddScopedService_Works()
        {
            IContainerBuilder builder = new DefaultContainerBuilder();
            builder.AddService<IFoo, Foo>(ServiceLifetime.Scoped);
            IServiceProvider container = builder.BuildContainer();

            IServiceProvider scopedContainer1 = container.GetRequiredService<IServiceScopeFactory>()
                .CreateScope().ServiceProvider;
            IFoo foo11 = scopedContainer1.GetService<IFoo>();
            IFoo foo12 = scopedContainer1.GetService<IFoo>();

            Assert.NotNull(foo11);
            Assert.NotNull(foo12);
            Assert.Equal(foo11, foo12);

            IServiceProvider scopedContainer2 = container.GetRequiredService<IServiceScopeFactory>()
                .CreateScope().ServiceProvider;
            IFoo foo21 = scopedContainer2.GetService<IFoo>();
            IFoo foo22 = scopedContainer2.GetService<IFoo>();

            Assert.NotNull(foo21);
            Assert.NotNull(foo22);
            Assert.Equal(foo21, foo22);

            Assert.NotEqual(foo11, foo21);
        }

        private interface IFoo { }

        private class Foo : IFoo { }
    }
}
