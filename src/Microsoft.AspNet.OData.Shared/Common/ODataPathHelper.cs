//-----------------------------------------------------------------------------
// <copyright file="ODataPathHelper.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNet.OData.Common
{
    /// <summary>
    /// Helper methods for <see cref="ODataPath"/>.
    /// </summary>
    internal static class ODataPathHelper
    {
        /// <summary>
        /// Get the keys from a <see cref="KeySegment"/>.
        /// </summary>
        /// <param name="keySegment">The <see cref="KeySegment"/> to extract the keys.</param>
        /// <returns>Dictionary of keys.</returns>
        public static Dictionary<string, object> KeySegmentAsDictionary(KeySegment keySegment)
        {
            if (keySegment == null)
            {
                throw Error.ArgumentNull(nameof(keySegment));
            }

            return keySegment.Keys.ToDictionary(d => d.Key, d => d.Value);
        }

        /// <summary>
        /// Get the position of the next <see cref="KeySegment"/> in a list of <see cref="ODataPathSegment"/>.
        /// </summary>
        /// <param name="pathSegments">List of <see cref="ODataPathSegment"/>.</param>
        /// <param name="currentPosition">Current position in the list of <see cref="ODataPathSegment"/>.</param>
        /// <returns>Position of the next <see cref="KeySegment"/> if it exists, or -1 otherwise.</returns>
        public static int GetNextKeySegmentPosition(IReadOnlyList<ODataPathSegment> pathSegments, int currentPosition)
        {
            if (pathSegments == null)
            {
                throw Error.ArgumentNull(nameof(pathSegments));
            }

            if (currentPosition < 0 || currentPosition >= pathSegments.Count)
            {
                return -1;
            }

            if (pathSegments[currentPosition] is KeySegment)
            {
                currentPosition++;
            }

            for (int i = currentPosition; i < pathSegments.Count; i++)
            {
                if (pathSegments[i] is KeySegment)
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
