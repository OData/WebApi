// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Reflection;

namespace System.Web.Http.OData.Builder
{
    public class ComplexPropertyConfiguration : StructuralPropertyConfiguration
    {
        public ComplexPropertyConfiguration(PropertyInfo property, IStructuralTypeConfiguration declaringType)
            : base(property, declaringType)
        {
        }

        public override PropertyKind Kind
        {
            get { return PropertyKind.Complex; }
        }

        public override Type RelatedClrType
        {
            get { return PropertyInfo.PropertyType; }
        }

        public ComplexPropertyConfiguration IsOptional()
        {
            OptionalProperty = true;
            return this;
        }

        public ComplexPropertyConfiguration IsRequired()
        {
            OptionalProperty = false;
            return this;
        }
    }
}
