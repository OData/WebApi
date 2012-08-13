// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Web.Http.OData.Builder.Conventions
{
    public interface IEdmPropertyConvention<TPropertyConfiguration> : IConvention where TPropertyConfiguration : PropertyConfiguration
    {
        void Apply(TPropertyConfiguration edmProperty, IStructuralTypeConfiguration structuralTypeConfiguration);
    }
}
