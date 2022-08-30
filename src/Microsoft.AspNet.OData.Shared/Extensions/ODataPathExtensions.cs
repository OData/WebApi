//-----------------------------------------------------------------------------
// <copyright file="ODataPathExtensions.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.AspNet.OData.Common;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNet.OData.Extensions
{
    /// <summary>
    /// Extensions method for <see cref="ODataPath"/>.
    /// </summary>
    public static class ODataPathExtensions
    {
        /// <summary>
        /// Get keys from the last <see cref="KeySegment"/>.
        /// </summary>
        /// <param name="path"><see cref="ODataPath"/>.</param>
        /// <returns>Dictionary of keys.</returns>
        public static Dictionary<string, object> GetKeys(this ODataPath path)
        {
            if (path == null)
            {
                throw Error.ArgumentNull(nameof(path));
            }

            List<ODataPathSegment> pathSegments = path.AsList();
            int position = path.Count - 1;
            ODataPathSegment pathSegment = pathSegments[position];

            while (!(pathSegment is KeySegment) && position >= 0)
            {
                pathSegment = pathSegments[--position];
            }

            return ODataPathHelper.GetKeysFromKeySegment(pathSegment as KeySegment);
        }

        /// <summary>
        /// Return the last segment in the path, which is not a <see cref="TypeSegment"/> or <see cref="KeySegment"/>.
        /// </summary>
        /// <param name="path">The <see cref="ODataPath"/>.</param>
        /// <returns>An <see cref="ODataPathSegment"/>.</returns>
        public static ODataPathSegment GetLastNonTypeNonKeySegment(this ODataPath path)
        {
            if (path == null)
            {
                throw Error.ArgumentNull(nameof(path));
            }

            if (path.Count == 1)
            {
                return path.LastSegment;
            }

            // If the path is Employees(2)/NewFriends(2)/Namespace.MyNewFriend where Namespace.MyNewFriend is a type segment,
            // This method will return NewFriends NavigationPropertySegment.

            List<ODataPathSegment> pathSegments = path.AsList();
            int position = path.Count - 1;
            ODataPathSegment pathSegment = pathSegments[position];

            while ((pathSegment is TypeSegment || pathSegment is KeySegment) && position>=0)
            {
                pathSegment = pathSegments[--position];
            }

            return pathSegment;
        }

        /// <summary>
        /// Returns a list of <see cref="ODataPathSegment"/> in an <see cref="ODataPath"/>.
        /// </summary>
        /// <param name="path">The <see cref="ODataPath"/>.</param>
        /// <returns>List of <see cref="ODataPathSegment"/>.</returns>
        public static List<ODataPathSegment> GetSegments(this ODataPath path)
        {
            return path.AsList();
        }
    }
}
