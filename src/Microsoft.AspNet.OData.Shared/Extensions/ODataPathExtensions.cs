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

            Dictionary<string, object> keys = new Dictionary<string, object>();

            // Books(1)/Authors(1000)/Namespace.SpecialAuthor
            if (path.LastSegment is TypeSegment)
            {
                ODataPath pathWithoutLastSegmentCastType = path.TrimEndingTypeSegment();

                if (pathWithoutLastSegmentCastType.LastSegment is KeySegment)
                {
                    keys = ODataPathHelper.GetKeysFromKeySegment(pathWithoutLastSegmentCastType.LastSegment as KeySegment);
                }
            }
            // Books(1)/Authors/Namespace.SpecialAuthor/(1000)
            else if (path.LastSegment is KeySegment)
            {
                keys = ODataPathHelper.GetKeysFromKeySegment(path.LastSegment as KeySegment);
            }

            return keys;
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

            while (path.LastSegment is TypeSegment || path.LastSegment is KeySegment)
            {
                if (path.LastSegment is TypeSegment)
                {
                    // We remove the last type segment from the path.
                    // E.g If the path is Employees(2)/NewFriends(2)/Namespace.MyNewFriend where Namespace.MyNewFriend,
                    // The updated path will be Employees(2)/NewFriends(2)
                    path = path.TrimEndingTypeSegment();
                }
                else if (path.LastSegment is KeySegment)
                {
                    // We remove the last key segment from the path.
                    // E.g If the path is Employees(2)/NewFriends(2),
                    // The updated path will be Employees(2)/NewFriends
                    path = path.TrimEndingKeySegment();
                }

                path.GetLastNonTypeNonKeySegment();
            }

            return path.LastSegment;
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
