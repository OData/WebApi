//-----------------------------------------------------------------------------
// <copyright file="ClrEnumMemberAnnotation.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.AspNet.OData.Common;
using Microsoft.OData.Edm;

namespace Microsoft.AspNet.OData
{
    /// <summary>
    /// Represents a mapping betwwen an <see cref="IEdmEnumMember"/> and a CLR Enum member.
    /// </summary>
    public class ClrEnumMemberAnnotation
    {
        private IDictionary<Enum, IEdmEnumMember> _map;
        private IDictionary<IEdmEnumMember, Enum> _reverseMap;

        /// <summary>
        /// Initializes a new instance of <see cref="ClrEnumMemberAnnotation"/> class.
        /// </summary>
        /// <param name="map">The mapping between CLR Enum member and the EDM <see cref="IEdmEnumMember"/>.</param>
        public ClrEnumMemberAnnotation(IDictionary<Enum, IEdmEnumMember> map)
        {
            if (map == null)
            {
                throw Error.ArgumentNull("map");
            }

            _map = map;
            _reverseMap = new Dictionary<IEdmEnumMember, Enum>();
            foreach (var item in map)
            {
                _reverseMap.Add(item.Value, item.Key);
            }
        }

        /// <summary>
        /// Gets the <see cref="IEdmEnumMember"/> for the CLR Enum member.
        /// </summary>
        /// <param name="clrEnumMemberInfo">The backing CLR Enum member info.</param>
        /// <returns>The Edm <see cref="IEdmEnumMember"/>.</returns>
        public IEdmEnumMember GetEdmEnumMember(Enum clrEnumMemberInfo)
        {
            IEdmEnumMember value;
            _map.TryGetValue(clrEnumMemberInfo, out value);
            return value;
        }

        /// <summary>
        /// Gets the CLR Enum member for <see cref="IEdmEnumMember"/>.
        /// </summary>
        /// <param name="edmEnumMember">The Edm <see cref="IEdmEnumMember"/>.</param>
        /// <returns>The backing CLR Enum member info.</returns>
        public Enum GetClrEnumMember(IEdmEnumMember edmEnumMember)
        {
            Enum value;
            _reverseMap.TryGetValue(edmEnumMember, out value);
            return value;
        }
    }
}
