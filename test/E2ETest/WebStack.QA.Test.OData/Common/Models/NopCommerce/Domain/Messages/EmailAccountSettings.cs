// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Nop.Core.Configuration;

namespace Nop.Core.Domain.Messages
{
    public class EmailAccountSettings : ISettings
    {
        /// <summary>
        /// Gets or sets a store default email account identifier
        /// </summary>
        public int DefaultEmailAccountId { get; set; }

    }

}
