// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Reflection;
using System.Web.Http;
using System.Web.OData.Properties;
using Microsoft.OData.Edm;

namespace System.Web.OData.Builder
{
    /// <summary>
    /// This annotation indicates the mapping from a <see cref="IEdmStructuredType"/> to a <see cref="PropertyInfo"/>.
    /// The <see cref="IEdmStructuredType"/> is an open type and the <see cref="PropertyInfo"/> is the specific
    /// property which is used in an open type to save/retrieve the dynamic properties.
    /// </summary>
    public class DynamicPropertyDictionaryAnnotation
    {
        /// <summary>
        /// Initializes a new instance of <see cref="DynamicPropertyDictionaryAnnotation"/> class.
        /// </summary>
        /// <param name="propertyInfo">The backing <see cref="PropertyInfo"/>.</param>
        public DynamicPropertyDictionaryAnnotation(PropertyInfo propertyInfo)
        {
            if (propertyInfo == null)
            {
                throw Error.ArgumentNull("propertyInfo");
            }

            if (!typeof(IDictionary<string, object>).IsAssignableFrom(propertyInfo.PropertyType))
            {
                throw Error.Argument("propertyInfo", SRResources.InvalidPropertyInfoForDynamicPropertyAnnotation,
                    propertyInfo.PropertyType.Name,
                    "IDictionary<string, object>");
            }

            PropertyInfo = propertyInfo;
        }

        /// <summary>
        /// Gets the <see cref="PropertyInfo"/> which backs the dynamic properties of the open type.
        /// </summary>
        public PropertyInfo PropertyInfo
        {
            get;
            private set;
        }
    }
}
