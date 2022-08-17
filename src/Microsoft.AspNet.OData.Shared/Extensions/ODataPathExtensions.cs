//-----------------------------------------------------------------------------
// <copyright file="ODataPathExtensions.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.AspNet.OData.Common;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNet.OData.Extensions
{
    internal static class ODataPathExtensions
    {
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
                    keys = GetKeysFromKeySegment(pathWithoutLastSegmentCastType.LastSegment as KeySegment);
                }
            }
            // Books(1)/Authors/Namespace.SpecialAuthor/(1000)
            else if (path.LastSegment is KeySegment)
            {
                keys = GetKeysFromKeySegment(path.LastSegment as KeySegment);
            }

            return keys;
        }

        public static ODataPathSegment GetLastNonKeySegment(this ODataPath path)
        {
            if (path == null)
            {
                throw Error.ArgumentNull(nameof(path));
            }

            if (path.Count == 1)
            {
                return path.LastSegment;
            }

            if (path.LastSegment is TypeSegment)
            {
                ODataPath modifiedPath = path.TrimEndingTypeSegment();

                return modifiedPath.LastSegment;
            }

            return path.LastSegment;
        }

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

            ODataPath modifiedPath = path;

            while (modifiedPath.LastSegment is TypeSegment || modifiedPath.LastSegment is KeySegment)
            {
                if (modifiedPath.LastSegment is TypeSegment)
                {
                    modifiedPath = modifiedPath.TrimEndingTypeSegment();
                }
                else if (modifiedPath.LastSegment is KeySegment)
                {
                    modifiedPath = modifiedPath.TrimEndingKeySegment();
                }

                modifiedPath.GetLastNonTypeNonKeySegment();
            }

            return modifiedPath.LastSegment;
        }

        public static List<ODataPathSegment> GetSegments(this ODataPath path)
        {
            return path.AsList();
        }

        private static Dictionary<string, object> GetKeysFromKeySegment(KeySegment keySegment)
        {
            Dictionary<string, object> keys = new Dictionary<string, object>();

            foreach (KeyValuePair<string, object> kvp in keySegment.Keys)
            {
                keys.Add(kvp.Key, kvp.Value);
            }

            return keys;
        }
    }
}
