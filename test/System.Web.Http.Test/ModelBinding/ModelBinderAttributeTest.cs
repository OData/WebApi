using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Controllers;
using System.Web.Http.ValueProviders;
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
            config.ServiceResolver.SetServices(typeof(ModelBinderProvider), new CustomModelBinderProvider());

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
        public void BinderType_From_ServiceResolver()
        {
            // To test ServiceResolver, the registered type and actual type should be different. 
            HttpConfiguration config = new HttpConfiguration();
            config.ServiceResolver.SetService(typeof(CustomModelBinderProvider), new SecondCustomModelBinderProvider());

            ModelBinderAttribute attr = new ModelBinderAttribute(typeof(CustomModelBinderProvider));

            ModelBinderProvider provider = attr.GetModelBinderProvider(config);
            Assert.IsType<SecondCustomModelBinderProvider>(provider);
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
