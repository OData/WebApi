// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.AspNet.Mvc.Facebook
{
    /// <summary>
    /// Describes the kind of permission for an entry in the <see cref="PermissionsStatus.Status"/> dictionary.
    /// </summary>
    public enum PermissionStatus
    {
        /// <summary>
        /// User granted permission.
        /// </summary>
        Granted,

        /// <summary>
        /// User declined permission.
        /// </summary>
        Declined,

        /// <summary>
        /// Unknown status of a permission.
        /// </summary>
        Unknown
    }
}
