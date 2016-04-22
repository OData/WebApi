// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.AspNetCore.OData.Formatter;

namespace Microsoft.AspNetCore.OData.Builder
{
    /// <summary>
    /// Base class for all structural property configurations.
    /// </summary>
    public abstract class StructuralPropertyConfiguration : PropertyConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StructuralPropertyConfiguration"/> class.
        /// </summary>
        /// <param name="property">The property of the configuration.</param>
        /// <param name="declaringType">The declaring type of the property.</param>
        protected StructuralPropertyConfiguration(PropertyInfo property, StructuralTypeConfiguration declaringType)
            : base(property, declaringType)
        {
            OptionalProperty = EdmLibHelpers.IsNullable(property.PropertyType);
        }

        /// <summary>
        /// Gets or sets a value indicating whether this property is optional or not.
        /// </summary>
        public bool OptionalProperty { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this property is a concurrency token or not.
        /// </summary>
        public bool ConcurrencyToken { get; set; }

        //protected Expression<Func<IValueProcessor>> Serializer { get; set; }
        //protected Expression<Func<IValueProcessor>> Deserializer { get; set; }

        //public void UseSerializer<TValueProvider>()
        //    where TValueProvider : IValueProcessor, new()
        //{
        //    UseSerializer(() => Activator.CreateInstance<TValueProvider>());
        //}

        //public void UseSerializer(IValueProcessor valueProvider)
        //{
        //    UseSerializer(() => valueProvider);
        //}

        //public void UseSerializer(Func<ValueInterceptor, bool> processor)
        //{
        //    UseSerializer(new InlineValueProcessor(processor));
        //}

        //public void UseSerializer(Expression<Func<IValueProcessor>> valueProviderResolver)
        //{
        //    Serializer = valueProviderResolver;
        //}

        //public void UseDeserializer<TValueProvider>()
        //    where TValueProvider : IValueProcessor, new()
        //{
        //    UseDeserializer(() => Activator.CreateInstance<TValueProvider>());
        //}

        //public void UseDeserializer(IValueProcessor valueProvider)
        //{
        //    UseDeserializer(() => valueProvider);
        //}

        //public void UseDeserializer(Func<ValueInterceptor, bool> processor)
        //{
        //    UseDeserializer(new InlineValueProcessor(processor));
        //}

        //public void UseDeserializer(Expression<Func<IValueProcessor>> valueProviderResolver)
        //{
        //    Deserializer = valueProviderResolver;
        //}
    }
}