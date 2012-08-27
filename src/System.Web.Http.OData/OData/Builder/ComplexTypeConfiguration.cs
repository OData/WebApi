// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Web.Http.OData.Builder
{
    public class ComplexTypeConfiguration : StructuralTypeConfiguration, IComplexTypeConfiguration
    {
        public ComplexTypeConfiguration(ODataModelBuilder modelBuilder, Type clrType)
            : base(modelBuilder, clrType)
        {
        }

        public override StructuralTypeKind Kind
        {
            get
            {
                return StructuralTypeKind.ComplexType;
            }
        }
    }
}
