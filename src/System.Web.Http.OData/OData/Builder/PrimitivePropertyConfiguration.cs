// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Reflection;

namespace System.Web.Http.OData.Builder
{
    public class PrimitivePropertyConfiguration : StructuralPropertyConfiguration
    {
        private bool? _concurrencyToken;

        public PrimitivePropertyConfiguration(PropertyInfo property)
            : base(property)
        {
        }

        public bool ConcurrencyToken
        {
            get { return (bool)_concurrencyToken; }
        }

        public override PropertyKind Kind
        {
            get { return PropertyKind.Primitive; }
        }

        public override Type RelatedClrType
        {
            get { return PropertyInfo.PropertyType; }
        }

        public PrimitivePropertyConfiguration IsConcurrencyToken() 
        {
            _concurrencyToken = true;
            return this;
        }

        public PrimitivePropertyConfiguration IsConcurrencyToken(bool? concurrencyToken)
        {
            _concurrencyToken = concurrencyToken;
            return this;
        }

        public PrimitivePropertyConfiguration IsOptional() 
        {
            OptionalProperty = true;
            return this;
        }

        public PrimitivePropertyConfiguration IsRequired() 
        {
            OptionalProperty = false;
            return this;
        }
    }
}
