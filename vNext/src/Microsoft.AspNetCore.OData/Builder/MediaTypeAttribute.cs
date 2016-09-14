// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;

namespace Microsoft.AspNetCore.OData.Builder
{
    /// <summary>
    /// Marks this entity type as media type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class MediaTypeAttribute : Attribute
    {
    }
}
