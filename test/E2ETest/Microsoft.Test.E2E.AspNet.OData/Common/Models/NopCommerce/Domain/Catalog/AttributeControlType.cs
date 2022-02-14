//-----------------------------------------------------------------------------
// <copyright file="AttributeControlType.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

namespace Nop.Core.Domain.Catalog
{
    /// <summary>
    /// Represents an attribute control type
    /// </summary>
    public enum AttributeControlType
    {
        /// <summary>
        /// Dropdown list
        /// </summary>
        DropdownList = 1,
        /// <summary>
        /// Radio list
        /// </summary>
        RadioList = 2,
        /// <summary>
        /// Checkboxes
        /// </summary>
        Checkboxes = 3,
        /// <summary>
        /// TextBox
        /// </summary>
        TextBox = 4,
        /// <summary>
        /// Multiline textbox
        /// </summary>
        MultilineTextbox = 10,
        /// <summary>
        /// Datepicker
        /// </summary>
        Datepicker = 20,
        /// <summary>
        /// File upload control
        /// </summary>
        FileUpload = 30,
    }
}
