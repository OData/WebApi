//-----------------------------------------------------------------------------
// <copyright file="Download.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;

namespace Nop.Core.Domain.Media
{
    /// <summary>
    /// Represents a download
    /// </summary>
    public partial class Download : BaseEntity
    {
        /// <summary>
        /// Gets a sets a GUID
        /// </summary>
        public virtual Guid DownloadGuid { get; set; }

        /// <summary>
        /// Gets a sets a value indicating whether DownloadUrl property should be used
        /// </summary>
        public virtual bool UseDownloadUrl { get; set; }

        /// <summary>
        /// Gets a sets a download URL
        /// </summary>
        public virtual string DownloadUrl { get; set; }

        /// <summary>
        /// Gets or sets the download binary
        /// </summary>
        public virtual byte[] DownloadBinary { get; set; }

        /// <summary>
        /// The mime-type of the download
        /// </summary>
        public virtual string ContentType { get; set; }

        /// <summary>
        /// The filename of the download
        /// </summary>
        public virtual string Filename { get; set; }

        /// <summary>
        /// Gets or sets the extension
        /// </summary>
        public virtual string Extension { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the download is new
        /// </summary>
        public virtual bool IsNew { get; set; }
    }

}
