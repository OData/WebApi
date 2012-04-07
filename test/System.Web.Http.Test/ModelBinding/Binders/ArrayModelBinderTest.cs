// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Web.Http.Controllers;
using System.Web.Http.Metadata.Providers;
using System.Web.Http.Util;
using Moq;
using Xunit;

namespace System.Web.Http.ModelBinding.Binders
{
    public class ArrayModelBinderTest
    {
        [Fact]
        public void BindModel()
        {
            // Arrange
            Mock<IModelBinder> mockIntBinder = new Mock<IModelBinder>();
            HttpActionContext context = ContextUtil.CreateActionContext();
            context.ControllerContext.Configuration.Services.Replace(typeof(ModelBinderProvider), new SimpleModelBinderProvider(typeof(int), mockIntBinder.Object));

            ModelBindingContext bindingContext = new ModelBindingContext
            {
                ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(null, typeof(int[])),
                ModelName = "someName",
                ValueProvider = new SimpleHttpValueProvider
                {
                    { "someName[0]", "42" },
                    { "someName[1]", "84" }
                }
            };
            mockIntBinder
                .Setup(o => o.BindModel(context, It.IsAny<ModelBindingContext>()))
                .Returns((HttpActionContext ec, ModelBindingContext mbc) =>
                {
                    mbc.Model = mbc.ValueProvider.GetValue(mbc.ModelName).ConvertTo(mbc.ModelType);
                    return true;
                });

            // Act
            bool retVal = new ArrayModelBinder<int>().BindModel(context, bindingContext);

            // Assert
            Assert.True(retVal);

            int[] array = bindingContext.Model as int[];
            Assert.Equal(new[] { 42, 84 }, array);
        }
    }
}
