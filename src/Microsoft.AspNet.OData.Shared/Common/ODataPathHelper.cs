//-----------------------------------------------------------------------------
// <copyright file="ODataPathHelper.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNet.OData.Common
{
    /// <summary>
    /// Helper methods for <see cref="ODataPath"/>.
    /// </summary>
    public static class ODataPathHelper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="keySegment"></param>
        /// <returns></returns>
        public static Dictionary<string, object> GetKeysFromKeySegment(KeySegment keySegment)
        {
            Dictionary<string, object> keys = new Dictionary<string, object>();

            foreach (KeyValuePair<string, object> kvp in keySegment.Keys)
            {
                keys.Add(kvp.Key, kvp.Value);
            }

            return keys;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pathSegments"></param>
        /// <param name="currentPosition"></param>
        /// <returns></returns>
        public static int GetNextKeySegmentPosition(List<ODataPathSegment> pathSegments, int currentPosition)
        {
            for (int i = currentPosition; i < pathSegments.Count; i++)
            {
                ODataPathSegment currentSegment = pathSegments[i];

                if (currentSegment is KeySegment)
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
