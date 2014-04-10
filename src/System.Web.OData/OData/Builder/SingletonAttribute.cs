// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Web.OData.Builder
{
    /// <summary>
    /// Represents an <see cref="Attribute"/> that can be placed on a property to specify
    /// that the property must bind to a singleton. It's used in convention model builder.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class SingletonAttribute : Attribute
    {
    }
}
