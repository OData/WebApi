//-----------------------------------------------------------------------------
// <copyright file="EdmTypeMap.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNet.OData.Query;
using Microsoft.OData.Edm;

namespace Microsoft.AspNet.OData.Builder
{
    internal class EdmTypeMap
    {
        public EdmTypeMap(
            Dictionary<Type, IEdmType> edmTypes,
            Dictionary<PropertyInfo, IEdmProperty> edmProperties,
            Dictionary<IEdmProperty, QueryableRestrictions> edmPropertiesRestrictions,
            Dictionary<IEdmProperty, ModelBoundQuerySettings> edmPropertiesQuerySettings,
            Dictionary<IEdmStructuredType, ModelBoundQuerySettings> edmStructuredTypeQuerySettings,
            Dictionary<Enum, IEdmEnumMember> enumMembers,
            Dictionary<IEdmStructuredType, PropertyInfo> openTypes,
            Dictionary<IEdmProperty, PropertyConfiguration> propertyConfigurations,
            Dictionary<IEdmStructuredType, PropertyInfo> instanceAnnotatableTypes )
        {
            EdmTypes = edmTypes;
            EdmProperties = edmProperties;
            EdmPropertiesRestrictions = edmPropertiesRestrictions;
            EdmPropertiesQuerySettings = edmPropertiesQuerySettings;
            EdmStructuredTypeQuerySettings = edmStructuredTypeQuerySettings;
            EnumMembers = enumMembers;
            OpenTypes = openTypes;
            EdmPropertyConfigurations = propertyConfigurations;
            InstanceAnnotatableTypes = instanceAnnotatableTypes;
        }

        public Dictionary<Type, IEdmType> EdmTypes { get; private set; }

        public Dictionary<PropertyInfo, IEdmProperty> EdmProperties { get; private set; }

        public Dictionary<IEdmProperty, QueryableRestrictions> EdmPropertiesRestrictions { get; private set; }

        public Dictionary<IEdmProperty, PropertyConfiguration> EdmPropertyConfigurations { get; private set; }

        public Dictionary<IEdmProperty, ModelBoundQuerySettings> EdmPropertiesQuerySettings { get; private set; }

        public Dictionary<IEdmStructuredType, ModelBoundQuerySettings> EdmStructuredTypeQuerySettings { get; private set; }

        public Dictionary<Enum, IEdmEnumMember> EnumMembers { get; private set; }

        public Dictionary<IEdmStructuredType, PropertyInfo> OpenTypes { get; private set; }

        public Dictionary<IEdmStructuredType, PropertyInfo> InstanceAnnotatableTypes { get; private set; }
    }
}
