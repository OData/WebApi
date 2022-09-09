//-----------------------------------------------------------------------------
// <copyright file="ODataIdContainer.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.AspNet.OData
{
    /// <summary>
    /// The odata id container. It will be used by POCO objects as well as Delta{TStructuralType}.
    /// </summary>
    public class ODataIdContainer
    {
        /// <summary>
        /// Gets or set the odata id path string.
        /// </summary>
        public string ODataId { get; set; }
    }
}
