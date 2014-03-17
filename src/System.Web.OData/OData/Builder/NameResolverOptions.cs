// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Web.OData.Builder
{
    /// <summary>
    /// Options for resolving property names.
    /// </summary>
    [Flags]
    public enum NameResolverOptions
    {
        /// <summary>
        /// Apply to property names which are not resolved by model aliasing.
        /// </summary>
        RespectModelAliasing = 1,

        /// <summary>
        /// Apply to property names which are not resolved explicitly.
        /// </summary>
        RespectExplicitProperties = 2,

        /// <summary>
        /// Apply to every property name.
        /// </summary>
        ApplyToAllProperties = 4,
    }
}
