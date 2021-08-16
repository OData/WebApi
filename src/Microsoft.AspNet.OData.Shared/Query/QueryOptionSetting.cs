//-----------------------------------------------------------------------------
// <copyright file="QueryOptionSetting.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.AspNet.OData.Query
{
    /// <summary>
    /// Represents the setting of a query option.
    /// </summary>
    public enum QueryOptionSetting
    {
        /// <summary>
        /// Allowed to be applied.
        /// </summary>
        Allowed,

        /// <summary>
        /// Disallowed to be applied.
        /// </summary>
        Disabled
    }
}
