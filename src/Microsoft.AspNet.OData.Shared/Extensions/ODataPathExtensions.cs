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
        internal static Dictionary<string, object> GetKeys(this ODataPath path)
        {
            if (path == null)
            {
                throw Error.ArgumentNull(nameof(path));
            }

            List<ODataPathSegment> pathSegments = path.AsList();

            if (pathSegments.Count == 0)
            {
                return null;
            }

            int position = path.Count - 1;

            while (position >= 0 && !(pathSegments[position] is KeySegment))
            {
                position--;
            }

            if (position >= 0 && pathSegments[position] is KeySegment keySegment)
            {
                return ODataPathHelper.KeySegmentAsDictionary(keySegment);
            }
            else
            {
                return null;
            }
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

            while (position >= 0 && (pathSegments[position] is TypeSegment || pathSegments[position] is KeySegment))
            {
                --position;
            }

            if (position < 0)
            {
                return null;
            }

            return pathSegments[position];
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
