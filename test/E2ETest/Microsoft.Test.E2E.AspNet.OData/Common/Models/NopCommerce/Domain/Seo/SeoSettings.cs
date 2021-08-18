//-----------------------------------------------------------------------------
// <copyright file="SeoSettings.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using Nop.Core.Configuration;

namespace Nop.Core.Domain.Seo
{
    public class SeoSettings : ISettings
    {
        public string PageTitleSeparator { get; set; }
        public PageTitleSeoAdjustment PageTitleSeoAdjustment { get; set; }
        public string DefaultTitle { get; set; }
        public string DefaultMetaKeywords { get; set; }
        public string DefaultMetaDescription { get; set; }

        public bool ConvertNonWesternChars { get; set; }
        public bool AllowUnicodeCharsInUrls { get; set; }

        public bool CanonicalUrlsEnabled { get; set; }

        /// <summary>
        /// Slugs (sename) reserved for some other needs
        /// </summary>
        public List<string> ReservedUrlRecordSlugs { get; set; }
    }
}
