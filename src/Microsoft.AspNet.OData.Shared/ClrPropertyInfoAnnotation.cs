//-----------------------------------------------------------------------------
// <copyright file="ClrPropertyInfoAnnotation.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Reflection;
using Microsoft.AspNet.OData.Common;
using Microsoft.OData.Edm;

namespace Microsoft.AspNet.OData
{
    /// <summary>
    /// Represents a mapping from an <see cref="IEdmProperty"/> to a CLR property info.
    /// </summary>
    public class ClrPropertyInfoAnnotation
    {
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
        /// Gets the backing CLR property info for the EDM property.
        /// </summary>
        public PropertyInfo ClrPropertyInfo { get; private set; }
    }
}
