//-----------------------------------------------------------------------------
// <copyright file="MessageTemplatesSettings.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Nop.Core.Configuration;

namespace Nop.Core.Domain.Messages
{
    public class MessageTemplatesSettings : ISettings
    {
        /// <summary>
        /// Gets or sets a value indicating whether to replace message tokens according to case invariant rules
        /// </summary>
        public bool CaseInvariantReplacement { get; set; }

        /// <summary>
        /// Gets or sets a color1 in  hex format ("#hhhhhh") to use in workflow message formatting
        /// </summary>
        public string Color1 { get; set; }

        /// <summary>
        /// Gets or sets a color2 in  hex format ("#hhhhhh") to use in workflow message formatting
        /// </summary>
        public string Color2 { get; set; }

        /// <summary>
        /// Gets or sets a color3 in  hex format ("#hhhhhh") to use in workflow message formatting
        /// </summary>
        public string Color3 { get; set; }

    }

}
