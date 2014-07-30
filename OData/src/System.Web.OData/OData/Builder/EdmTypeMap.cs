// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Reflection;
using Microsoft.OData.Edm;

namespace System.Web.OData.Builder
{
    internal class EdmTypeMap
    {
        public EdmTypeMap(
            Dictionary<Type, IEdmType> edmTypes,
            Dictionary<PropertyInfo, IEdmProperty> edmProperties,
            Dictionary<IEdmProperty, QueryableRestrictions> edmPropertiesRestrictions,
            Dictionary<Enum, IEdmEnumMember> enumMembers,
            Dictionary<IEdmStructuredType, PropertyInfo> openTypes)
        {
            EdmTypes = edmTypes;
            EdmProperties = edmProperties;
            EdmPropertiesRestrictions = edmPropertiesRestrictions;
            EnumMembers = enumMembers;
            OpenTypes = openTypes;
        }

        public Dictionary<Type, IEdmType> EdmTypes { get; private set; }

        public Dictionary<PropertyInfo, IEdmProperty> EdmProperties { get; private set; }

        public Dictionary<IEdmProperty, QueryableRestrictions> EdmPropertiesRestrictions { get; private set; }

        public Dictionary<Enum, IEdmEnumMember> EnumMembers { get; private set; }

        public Dictionary<IEdmStructuredType, PropertyInfo> OpenTypes { get; private set; }
    }
}
