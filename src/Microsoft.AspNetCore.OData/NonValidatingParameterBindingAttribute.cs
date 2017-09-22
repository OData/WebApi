// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;

namespace Microsoft.AspNet.OData
{
    /// <summary>
    /// An attribute to disable WebApi model validation for a particular type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    internal sealed partial class NonValidatingParameterBindingAttribute : Attribute
    {
    }
}
