// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Web.Mvc;

namespace Microsoft.Web.Mvc
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
    public sealed class DeserializeAttribute : CustomModelBinderAttribute
    {
        public DeserializeAttribute()
            : this(MvcSerializer.DefaultSerializationMode)
        {
        }

        public DeserializeAttribute(SerializationMode mode)
        {
            Mode = mode;
        }

        public SerializationMode Mode { get; private set; }

        internal MvcSerializer Serializer { get; set; }

        public override IModelBinder GetBinder()
        {
            return new DeserializingModelBinder(Mode, Serializer);
        }

        private sealed class DeserializingModelBinder : IModelBinder
        {
            private readonly SerializationMode _mode;
            private readonly MvcSerializer _serializer;

            public DeserializingModelBinder(SerializationMode mode, MvcSerializer serializer)
            {
                _mode = mode;
                _serializer = serializer ?? new MvcSerializer();
            }

            [SuppressMessage("Microsoft.Globalization", "CA1304:SpecifyCultureInfo", MessageId = "System.Web.Mvc.ValueProviderResult.ConvertTo(System.Type)", Justification = "The target object should make the correct culture determination, not this method.")]
            public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
            {
                if (bindingContext == null)
                {
                    throw new ArgumentNullException("bindingContext");
                }

                ValueProviderResult valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
                if (valueProviderResult == null)
                {
                    // nothing found
                    return null;
                }

                string serializedValue = (string)valueProviderResult.ConvertTo(typeof(string));
                return _serializer.Deserialize(serializedValue, _mode);
            }
        }
    }
}
