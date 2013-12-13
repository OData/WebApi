// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Reflection;
using Microsoft.OData.Edm;

namespace System.Web.Http.OData.Builder
{
    internal class EdmTypeMap
    {
        public EdmTypeMap(
            Dictionary<Type, IEdmStructuredType> edmTypes,
            Dictionary<PropertyInfo, IEdmProperty> edmProperties,
            Dictionary<IEdmProperty, QueryableRestrictions> edmPropertiesRestrictions)
        {
            EdmTypes = edmTypes;
            EdmProperties = edmProperties;
            EdmPropertiesRestrictions = edmPropertiesRestrictions;
        }

        public Dictionary<Type, IEdmStructuredType> EdmTypes { get; private set; }

        public Dictionary<PropertyInfo, IEdmProperty> EdmProperties { get; private set; }

        public Dictionary<IEdmProperty, QueryableRestrictions> EdmPropertiesRestrictions { get; private set; }
    }
}
