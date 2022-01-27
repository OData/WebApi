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

        /// <summary>
        /// Initializes a new instance of the <see cref="NavigationPath"/> class.
        /// </summary>        
        /// <param name="path">ODataPath</param>
        /// <param name="model">The IEdmModel</param>
        public NavigationPath(string path, IEdmModel model)
        {
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            ODataUriParser parser = new ODataUriParser(model, new Uri(path, UriKind.Relative));

            ODataPath odataPath = parser.ParsePath();
            ParseODataId(odataPath);
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
    }
}
