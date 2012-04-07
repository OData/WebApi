// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations;
using System.Data.Linq;
using System.Threading;
using Xunit;

namespace System.Web.Mvc.Test
{
    public class ModelBindersTest
    {
        [Fact]
        public void BindersPropertyIsNotNull()
        {
            // Arrange & Act
            ModelBinderDictionary binders = ModelBinders.Binders;

            // Assert
            Assert.NotNull(binders);
        }

        [Fact]
        public void DefaultModelBinders()
        {
            // Act
            ModelBinderDictionary binders = ModelBinders.Binders;

            // Assert
            Assert.Equal(4, binders.Count);
            Assert.True(binders.ContainsKey(typeof(byte[])));
            Assert.IsType<ByteArrayModelBinder>(binders[typeof(byte[])]);
            Assert.True(binders.ContainsKey(typeof(HttpPostedFileBase)));
            Assert.IsType<HttpPostedFileBaseModelBinder>(binders[typeof(HttpPostedFileBase)]);
            Assert.True(binders.ContainsKey(typeof(Binary)));
            Assert.IsType<LinqBinaryModelBinder>(binders[typeof(Binary)]);
            Assert.True(binders.ContainsKey(typeof(CancellationToken)));
            Assert.IsType<CancellationTokenModelBinder>(binders[typeof(CancellationToken)]);
        }

        [Fact]
        public void GetBindersFromAttributes_ReadsModelBinderAttributeFromBuddyClass()
        {
            // Act
            IModelBinder binder = ModelBinders.GetBinderFromAttributes(typeof(SampleModel), null);

            // Assert
            Assert.IsType<SampleModelBinder>(binder);
        }

        [MetadataType(typeof(SampleModel_Buddy))]
        private class SampleModel
        {
            [ModelBinder(typeof(SampleModelBinder))]
            private class SampleModel_Buddy
            {
            }
        }

        private class SampleModelBinder : IModelBinder
        {
            public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
            {
                throw new NotImplementedException();
            }
        }
    }
}
