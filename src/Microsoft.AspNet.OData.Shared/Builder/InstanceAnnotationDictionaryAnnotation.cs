// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNet.OData.Common;
using Microsoft.OData.Edm;

namespace Microsoft.AspNet.OData.Builder
{
    /// <summary>
    /// This annotation indicates the mapping from a <see cref="IEdmStructuredType"/> to a <see cref="PropertyInfo"/>.
    /// The <see cref="IEdmStructuredType"/> is an open type and the <see cref="PropertyInfo"/> is the specific
    /// property which is used in an open type to save/retrieve the instance annotations.
    /// </summary>
    public class InstanceAnnotationDictionaryAnnotation
    {
        /// <summary>
        /// Initializes a new instance of <see cref="InstanceAnnotationDictionaryAnnotation"/> class.
        /// </summary>
        /// <param name="propertyInfo">The backing <see cref="PropertyInfo"/>.</param>
        public InstanceAnnotationDictionaryAnnotation(PropertyInfo propertyInfo)
        {
            if (propertyInfo == null)
            {
                throw Error.ArgumentNull("propertyInfo");
            }

            if (!typeof(IDictionary<string,IDictionary<string, object> >).IsAssignableFrom(propertyInfo.PropertyType))
            {
                throw Error.Argument("propertyInfo", SRResources.InvalidPropertyInfoForAnnotationPropertyAnnotation,
                    propertyInfo.PropertyType.Name,
                    "IDictionary<string, IDictionary<string, object>>");
            }

            PropertyInfo = propertyInfo;
        }

        /// <summary>
        /// Gets the <see cref="PropertyInfo"/> which backs the instance annotations of the open type.
        /// </summary>
        public PropertyInfo PropertyInfo
        {
            get;
            private set;
        }
    }
}
