// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Reflection;

namespace System.Web.Http.OData.Builder
{
    public abstract class StructuralPropertyConfiguration : PropertyConfiguration
    {
        protected StructuralPropertyConfiguration(PropertyInfo property)
            : base(property)
        {
            OptionalProperty = IsNullable(property.PropertyType);
        }

        public bool OptionalProperty { get; set; }

        private static bool IsNullable(Type type)
        {
            return type.IsClass || Nullable.GetUnderlyingType(type) != null;
        }
    }
}
