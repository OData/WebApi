//-----------------------------------------------------------------------------
// <copyright file="NavigationPath.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNet.OData
{
    /// <summary>
    /// Navigation Path of an OData ID
    /// </summary>
    public class NavigationPath : List<PathItem>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NavigationPath"/> class.
        /// </summary>        
        /// <param name="path">ODataPath</param>
        public NavigationPath(ODataPath path)
        {
            ParseODataId(path);
        }
        
        private void ParseODataId(ODataPath path)
        {
            PathItem currentPathItem = null;

            foreach (ODataPathSegment segment in path)
            {
                if (segment is EntitySetSegment || segment is NavigationPropertySegment || segment is PropertySegment)
                {
                    this.Add(new PathItem());
                    currentPathItem = this.Last();
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
        }

        // todo: maybe make this a constructor overload
        /// <summary>
        /// Gets a NavigationPath for an OData path
        /// </summary>
        /// <param name="path">The OData path</param>
        /// <param name="model">The IEdmModel</param>
        /// <returns></returns>
        public static NavigationPath GetNavigationPath(string path, IEdmModel model)
        {
            if (string.IsNullOrEmpty(path))
            { 
                return null;
            }

            ODataUriParser parser = new ODataUriParser(model, new Uri(path, UriKind.Relative));

            ODataPath odataPath = parser.ParsePath();
            return new NavigationPath(odataPath);
        }
    }
}
