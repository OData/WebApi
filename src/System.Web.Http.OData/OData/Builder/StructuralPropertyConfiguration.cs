// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Reflection;
using System.Web.Http.OData.Formatter;

namespace System.Web.Http.OData.Builder
{
    /// <summary>
    /// Base class for all structural property configurations.
    /// </summary>
    public abstract class StructuralPropertyConfiguration : PropertyConfiguration
    {
        private bool _optionalProperty;

        protected StructuralPropertyConfiguration(PropertyInfo property, StructuralTypeConfiguration declaringType)
            : base(property, declaringType)
        {
            _optionalProperty = EdmLibHelpers.IsNullable(property.PropertyType);
        }

        public bool OptionalProperty
        {
            get
            {
                return _optionalProperty;
            }

            set
            {
                _optionalProperty = value;
                IsOptionalPropertyExplicitlySet = true;
            }
        }

        internal bool IsOptionalPropertyExplicitlySet { get; set; }
    }
}
