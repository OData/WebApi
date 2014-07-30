// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Web.OData.Builder
{
    /// <summary>
    /// Mark a navigation property as containment.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class ContainedAttribute : Attribute
    {
    }
}
