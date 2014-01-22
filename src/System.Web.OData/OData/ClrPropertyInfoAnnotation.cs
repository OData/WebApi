// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Reflection;
using System.Web.Http;
using Microsoft.OData.Edm;

namespace System.Web.OData
{
    /// <summary>
    /// Represents a mapping from an <see cref="IEdmProperty"/> to a CLR property info.
    /// </summary>
    public class ClrPropertyInfoAnnotation
    {
        private PropertyInfo _clrPropertyInfo;

        /// <summary>
        /// Initializes a new instance of <see cref="ClrPropertyInfoAnnotation"/> class.
        /// </summary>
        /// <param name="clrPropertyInfo">The backing CLR property info for the EDM property.</param>
        public ClrPropertyInfoAnnotation(PropertyInfo clrPropertyInfo)
        {
            if (clrPropertyInfo == null)
            {
                throw Error.ArgumentNull("clrPropertyInfo");
            }

            ClrPropertyInfo = clrPropertyInfo;
        }

        /// <summary>
        /// Gets or sets the backing CLR property info for the EDM property.
        /// </summary>
        public PropertyInfo ClrPropertyInfo
        {
            get
            {
                return _clrPropertyInfo;
            }
            set
            {
                if (value == null)
                {
                    throw Error.PropertyNull();
                }

                _clrPropertyInfo = value;
            }
        }
    }
}
