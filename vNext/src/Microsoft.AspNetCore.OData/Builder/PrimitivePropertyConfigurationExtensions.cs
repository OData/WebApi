// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNetCore.OData.Common;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Builder
{
    /// <summary>
    /// Extensions method for <see cref="PrimitivePropertyConfiguration"/>.
    /// </summary>
    public static class PrimitivePropertyConfigurationExtensions
    {
        /// <summary>
        /// If this primitive property is <see cref="System.DateTime"/>, this method will make the target
        /// Edm type kind as <see cref="Date"/>
        /// </summary>
        /// <param name="property">Reference to the calling primitive property configuration.</param>
        /// <returns>Returns itself so that multiple calls can be chained.</returns>
        public static PrimitivePropertyConfiguration AsDate(this PrimitivePropertyConfiguration property)
        {
            if (property == null)
            {
                throw Error.ArgumentNull("property");
            }

            if (!TypeHelper.IsDateTime(property.RelatedClrType))
            {
                throw Error.Argument("property", "TODO: "/*SRResources.MustBeDateTimeProperty, property.PropertyInfo.Name,
                    property.DeclaringType.FullName*/);
            }

            property.TargetEdmTypeKind = EdmPrimitiveTypeKind.Date;
            return property;
        }

        /// <summary>
        /// If this primitive property is <see cref="System.TimeSpan"/>, this method will make the target
        /// Edm type kind as <see cref="TimeOfDay"/>
        /// </summary>
        /// <param name="property">Reference to the calling primitive property configuration.</param>
        /// <returns>Returns itself so that multiple calls can be chained.</returns>
        public static PrimitivePropertyConfiguration AsTimeOfDay(this PrimitivePropertyConfiguration property)
        {
            if (property == null)
            {
                throw Error.ArgumentNull("property");
            }

            if (!TypeHelper.IsTimeSpan(property.RelatedClrType))
            {
                throw Error.Argument("property", "TODO: "/*SRResources.MustBeTimeSpanProperty, property.PropertyInfo.Name,
                    property.DeclaringType.FullName*/);
            }

            property.TargetEdmTypeKind = EdmPrimitiveTypeKind.TimeOfDay;
            return property;
        }
    }
}
