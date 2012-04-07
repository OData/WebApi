// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Controllers;
using System.Web.Http.Dependencies;
using System.Web.Http.ValueProviders;
using Moq;
using Xunit;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Web.Http.ModelBinding
{
    public class ModelBinderAttributeTest
    {
        [Fact]
        public void Empty_BinderType()
        {
            HttpConfiguration config = new HttpConfiguration();
            config.Services.Replace(typeof(ModelBinderProvider), new CustomModelBinderProvider());

            ModelBinderAttribute attr = new ModelBinderAttribute();

            ModelBinderProvider provider = attr.GetModelBinderProvider(config);
            Assert.IsType<CustomModelBinderProvider>(provider);
        }

        [Fact]
        public void Illegal_BinderType()
        {
            // Given an illegal type.
            // Constructor shouldn't throw. But trying to instantiate the model binder provider will throw.
            ModelBinderAttribute attr = new ModelBinderAttribute(typeof(object));

            Assert.Equal(typeof(object), attr.BinderType); // can still lookup illegal type
            Assert.Throws<InvalidOperationException>(
                () => attr.GetModelBinderProvider(new HttpConfiguration())
            );
        }

        [Fact]
        public void BinderType_Provided()
        {
            HttpConfiguration config = new HttpConfiguration();
            ModelBinderAttribute attr = new ModelBinderAttribute(typeof(CustomModelBinderProvider));

            ModelBinderProvider provider = attr.GetModelBinderProvider(config);
            Assert.IsType<CustomModelBinderProvider>(provider);
        }

        [Fact]
        public void BinderType_From_DependencyResolver()
        {
            // To test dependency resolver, the registered type and actual type should be different. 
            HttpConfiguration config = new HttpConfiguration();
            var mockDependencyResolver = new Mock<IDependencyResolver>();
            mockDependencyResolver.Setup(r => r.GetService(typeof(CustomModelBinderProvider)))
                               .Returns(new SecondCustomModelBinderProvider());
            config.DependencyResolver = mockDependencyResolver.Object;

            ModelBinderAttribute attr = new ModelBinderAttribute(typeof(CustomModelBinderProvider));

            ModelBinderProvider provider = attr.GetModelBinderProvider(config);
            Assert.IsType<SecondCustomModelBinderProvider>(provider);
        }

        [Fact]
        public void BinderType_From_DependencyResolver_ReleasedWhenConfigIsDisposed()
        {
            // Arrange
            HttpConfiguration config = new HttpConfiguration();
            var mockDependencyResolver = new Mock<IDependencyResolver>();
            SecondCustomModelBinderProvider provider = new SecondCustomModelBinderProvider();
            mockDependencyResolver.Setup(r => r.GetService(typeof(CustomModelBinderProvider))).Returns(provider);
            config.DependencyResolver = mockDependencyResolver.Object;
            ModelBinderAttribute attr = new ModelBinderAttribute(typeof(CustomModelBinderProvider));
            attr.GetModelBinderProvider(config);

            // Act
            config.Dispose();

            // Assert
            mockDependencyResolver.Verify(dr => dr.Dispose(), Times.Once());
        }

        [Fact]
        public void Set_ModelBinder_And_ValueProviders()
        {
            HttpConfiguration config = new HttpConfiguration();
            ModelBinderAttribute attr = new ValueProviderAttribute(typeof(CustomValueProviderFactory)) { BinderType = typeof(CustomModelBinderProvider) };
            IEnumerable<ValueProviderFactory> vpfs = attr.GetValueProviderFactories(config);

            Assert.IsType<CustomModelBinderProvider>(attr.GetModelBinderProvider(config));
            Assert.Equal(1, vpfs.Count());
            Assert.IsType<CustomValueProviderFactory>(vpfs.First());
        }

        private class CustomModelBinderProvider : ModelBinderProvider
        {
            public override IModelBinder GetBinder(HttpActionContext actionContext, ModelBindingContext bindingContext)
            {
                throw new NotImplementedException();
            }
        }

        private class SecondCustomModelBinderProvider : ModelBinderProvider
        {
            public override IModelBinder GetBinder(HttpActionContext actionContext, ModelBindingContext bindingContext)
            {
                throw new NotImplementedException();
            }
        }

        private class CustomValueProviderFactory : ValueProviderFactory
        {
            public override IValueProvider GetValueProvider(HttpActionContext actionContext)
            {
                throw new NotImplementedException();
            }
        }
    }
}
