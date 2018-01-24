// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using Nop.Core.Configuration;

namespace Nop.Core.Domain.Customers
{
    public class ExternalAuthenticationSettings : ISettings
    {
        public ExternalAuthenticationSettings()
        {
            ActiveAuthenticationMethodSystemNames = new List<string>();
        }

        public bool AutoRegisterEnabled { get; set; }
        /// <summary>
        /// Gets or sets an system names of active payment methods
        /// </summary>
        public List<string> ActiveAuthenticationMethodSystemNames { get; set; }
    }
}