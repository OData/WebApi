// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNet.OData
{
    /// <summary>
    /// Navigation Path of an OData ID
    /// </summary>
    public class NavigationPath
    {
        private string navigationPathName;
        private ReadOnlyCollection<ODataPathSegment> _pathSegments;
        private ConcurrentDictionary<string, PathItem[]> _pathItemCache = new ConcurrentDictionary<string, PathItem[]>();

        /// <summary>
        /// Constructor which takes and odataId and creates PathItems
        /// </summary>
        /// <param name="navigationPath">ODataId in string format</param>
        /// <param name="pathSegments">Pathsegment collection</param>
        public NavigationPath(string navigationPath, ReadOnlyCollection<ODataPathSegment> pathSegments)
        {
            navigationPathName = navigationPath;
            _pathSegments = pathSegments;           
        }

        
        /// <summary>
        /// NavigationPath/ODataId in string
        /// </summary>
        public string NavigationPathName { get { return navigationPathName; } }

        /// <summary>
        /// To Get ODataId in Parsed format
        /// </summary>
        /// <returns>Array of PathItems</returns>
        public PathItem[] GetNavigationPathItems()
        {
            PathItem[] pathItems;
            if(!_pathItemCache.TryGetValue(navigationPathName, out pathItems))
            {
                if (_pathSegments != null)
                {
                    pathItems = ParseODataId();
                    _pathItemCache.TryAdd(navigationPathName, pathItems);
                }
            }

            return pathItems;
        }

        private PathItem[] ParseODataId()
        {
            List<PathItem> pathItems = new List<PathItem>();
            PathItem currentPathItem = null;

            foreach (ODataPathSegment segment in _pathSegments)
            {
                if(segment is KeySegment keySegment)
                {
                    currentPathItem.KeyProperties = new Dictionary<string, object>();

                    foreach(KeyValuePair<string, object> key in keySegment.Keys)
                    {
                        currentPathItem.KeyProperties.Add(key.Key, key.Value); 
                    }
                }
                else
                {
                    pathItems.Add(new PathItem());
                    currentPathItem = pathItems.Last();                   
                    currentPathItem.Name = segment.Identifier;                    
                }
            }

            return pathItems.ToArray();
        }
    }
}
