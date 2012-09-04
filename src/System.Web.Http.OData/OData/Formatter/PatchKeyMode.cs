// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Web.Http.OData.Formatter
{
    /// <summary>
    /// Specifies the behavior for PATCHing key properties.
    /// </summary>
    public enum PatchKeyMode
    {
        /// <summary>
        /// Ignore key properties in the incoming message. Do not patch them.
        /// This is the default value for <see cref="PatchKeyMode"/>.
        /// </summary>
        Ignore = 0,

        /// <summary>
        /// Patch key properties.
        /// </summary>
        Patch,

        /// <summary>
        /// Throw if key properties are present in the incoming message.
        /// </summary>
        Throw
    }
}
