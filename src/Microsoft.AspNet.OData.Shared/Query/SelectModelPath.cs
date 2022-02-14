//-----------------------------------------------------------------------------
// <copyright file="SelectModelPath.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.AspNet.OData.Common;
using Microsoft.OData;
using Microsoft.OData.Edm;

namespace Microsoft.AspNet.OData.Query
{
    /// <summary>
    /// The select property path is a model path with the following restriction:
    /// 1) only include type case and structural property
    /// </summary>
    internal class SelectModelPath : List<IEdmElement>
    {
        private string _selectPath;

        /// <summary>
        /// Initializes a new instance of the <see cref="SelectModelPath" /> class.
        /// </summary>
        /// <param name="nodes">The segment nodes.</param>
        public SelectModelPath(IEnumerable<IEdmElement> nodes)
           : base(nodes)
        {
            ValidateAndCalculateElementPath();
        }

        /// <summary>
        /// Gets the select path.
        /// </summary>
        public string SelectPath => _selectPath;

        private void ValidateAndCalculateElementPath()
        {
            IList<string> identifiers = new List<string>();
            foreach (IEdmElement element in this)
            {
                if (element is IEdmStructuredType structuredType)
                {
                    identifiers.Add(structuredType.FullTypeName());
                }
                else if (element is IEdmStructuralProperty structuralProperty)
                {
                    identifiers.Add(structuralProperty.Name);
                }
                else
                {
                    throw new ODataException(Error.Format(SRResources.InvalidSegmentInSelectExpandPath, element.GetType().Name));
                }
            }

            _selectPath = string.Join("/", identifiers);
        }
    }
}