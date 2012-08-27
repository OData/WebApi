// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Web.Http.OData.Builder.Conventions
{
    /// <summary>
    /// Convention to process properties of <see cref="IStructuralTypeConfiguration"/>.
    /// </summary>
    /// <typeparam name="TPropertyConfiguration"></typeparam>
    public interface IEdmPropertyConvention<TPropertyConfiguration> : IEdmPropertyConvention where TPropertyConfiguration : PropertyConfiguration
    {
        /// <summary>
        /// Applies the convention.
        /// </summary>
        /// <param name="edmProperty">The property the convention is applied on.</param>
        /// <param name="structuralTypeConfiguration">The <see cref="IStructuralTypeConfiguration"/> the edmProperty belongs to.</param>
        void Apply(TPropertyConfiguration edmProperty, IStructuralTypeConfiguration structuralTypeConfiguration);
    }
}
