// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Nop.Core.Domain.Security
{
    /// <summary>
    /// Represents a default permission record
    /// </summary>
    public class DefaultPermissionRecord
    {
        public DefaultPermissionRecord()
        {
            this.PermissionRecords = new List<PermissionRecord>();
        }

        /// <summary>
        /// Gets or sets the customer role system name
        /// </summary>
        public string CustomerRoleSystemName { get; set; }

        /// <summary>
        /// Gets or sets the permissions
        /// </summary>
        public IEnumerable<PermissionRecord> PermissionRecords { get; set; }
    }
}
