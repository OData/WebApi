// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Data.Edm;

namespace System.Web.Http.OData.Builder
{
    public class ComplexTypeConfiguration : StructuralTypeConfiguration, IComplexTypeConfiguration
    {
        public ComplexTypeConfiguration(ODataModelBuilder modelBuilder, Type clrType)
            : base(modelBuilder, clrType)
        {
        }

        public override EdmTypeKind Kind
        {
            get
            {
                return EdmTypeKind.Complex;
            }
        }
    }
}
