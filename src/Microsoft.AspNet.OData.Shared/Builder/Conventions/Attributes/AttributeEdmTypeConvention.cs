﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.OData.Common;

namespace Microsoft.AspNet.OData.Builder.Conventions.Attributes
{
    /// <summary>
    /// Base class for all <see cref="IEdmTypeConvention"/>'s based on a attribute on the type.
    /// </summary>
    /// <typeparam name="TEdmTypeConfiguration">The kind of Edm type that this convention must be applied to.</typeparam>
    internal abstract class AttributeEdmTypeConvention<TEdmTypeConfiguration> : AttributeConvention, IEdmTypeConvention
        where TEdmTypeConfiguration : class, IEdmTypeConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AttributeEdmTypeConvention{TEdmTypeConfiguration}"/> class.
        /// </summary>
        /// <param name="attributeFilter">A function to test whether this convention applies to an attribute or not.</param>
        /// <param name="allowMultiple"><c>true</c> if the convention allows multiple attributes; otherwise, <c>false</c>.</param>
        protected AttributeEdmTypeConvention(Func<Attribute, bool> attributeFilter, bool allowMultiple)
            : base(attributeFilter, allowMultiple)
        {
        }

        /// <summary>
        /// Applies the convention.
        /// </summary>
        /// <param name="edmTypeConfiguration">The edm type to apply the convention to.</param>
        /// <param name="model">The model that this edm type belongs to.</param>
        public void Apply(IEdmTypeConfiguration edmTypeConfiguration, ODataConventionModelBuilder model)
        {
            TEdmTypeConfiguration type = edmTypeConfiguration as TEdmTypeConfiguration;
            if (type != null)
            {
                Apply(type, model);
            }
        }

        /// <summary>
        /// Applies the convention.
        /// </summary>
        /// <param name="edmTypeConfiguration">The edm type to apply the convention to.</param>
        /// <param name="model">The model that this edm type belongs to.</param>
        public void Apply(TEdmTypeConfiguration edmTypeConfiguration, ODataConventionModelBuilder model)
        {
            if (edmTypeConfiguration == null)
            {
                throw Error.ArgumentNull("edmTypeConfiguration");
            }

            foreach (Attribute attribute in GetAttributes(TypeHelper.AsMemberInfo(edmTypeConfiguration.ClrType)))
            {
                Apply(edmTypeConfiguration, model, attribute);
            }
        }

        /// <summary>
        /// Applies the convention.
        /// </summary>
        /// <param name="edmTypeConfiguration">The edm type to apply the convention to.</param>
        /// <param name="model">The model that this edm type belongs to.</param>
        /// <param name="attribute">The attribute found on this edm type.</param>
        public abstract void Apply(TEdmTypeConfiguration edmTypeConfiguration, ODataConventionModelBuilder model,
            Attribute attribute);
    }
}
