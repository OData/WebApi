//-----------------------------------------------------------------------------
// <copyright file="NavigationPath.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNet.OData
{
    /// <summary>
    /// Navigation Path of an OData ID
    /// </summary>
    public class NavigationPath
    {
        private string _navigationPathName;
        private ReadOnlyCollection<ODataPathSegment> _pathSegments;
        PathItem[] _pathItems;

        /// <summary>
        /// Initializes a new instance of the <see cref="NavigationPath"/> class.
        /// </summary>        
        /// <param name="pathSegments">Pathsegment collection</param>
        public NavigationPath(ReadOnlyCollection<ODataPathSegment> pathSegments)
        {
             Debug.Assert(pathSegments.Count > 0);

            _pathSegments = pathSegments;
            _navigationPathName = pathSegments.First().Identifier;
        }

        
        /// <summary>
        /// Gets the NavigationPath name
        /// </summary>
        public string NavigationPathName { get { return _navigationPathName; } }

        /// <summary>
        /// To Get ODataId in Parsed format
        /// </summary>
        /// <returns>Array of PathItems</returns>
        public PathItem[] GetNavigationPathItems()
        {            
            if(_pathItems == null && _pathSegments != null)
            {
                _pathItems = ParseODataId();
            }

            return _pathItems;
        }

        private PathItem[] ParseODataId()
        {
            List<PathItem> pathItems = new List<PathItem>();
            PathItem currentPathItem = null;

            foreach (ODataPathSegment segment in _pathSegments)
            {
                if (segment is EntitySetSegment || segment is NavigationPropertySegment || segment is PropertySegment)
                {
                    pathItems.Add(new PathItem());
                    currentPathItem = pathItems.Last();
                    currentPathItem.Name = segment.Identifier;
                }
                else if(segment is TypeSegment)
                {
                    currentPathItem.IsCastType = true;
                    currentPathItem.CastTypeName = segment.Identifier;
                }
                else if (segment is KeySegment keySegment)
                {
                    currentPathItem.KeyProperties = new Dictionary<string, object>();

                    foreach(KeyValuePair<string, object> key in keySegment.Keys)
                    {
                        currentPathItem.KeyProperties.Add(key.Key, key.Value); 
                    }
                }
                
            }

            return pathItems.ToArray();
        }
    }
}
