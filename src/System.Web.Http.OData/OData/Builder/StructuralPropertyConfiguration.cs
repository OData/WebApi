// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace System.Web.Http.OData.Builder
{
    public abstract class StructuralPropertyConfiguration : PropertyConfiguration
    {
        protected StructuralPropertyConfiguration(PropertyInfo property) : base(property)
        {
        }

        public virtual bool OptionalProperty 
        { 
            get; 
            set; 
        }
    }
}
