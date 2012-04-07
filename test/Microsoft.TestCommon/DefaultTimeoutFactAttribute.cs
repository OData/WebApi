// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using Xunit;

namespace Microsoft.TestCommon
{
    /// <summary>
    /// An override of <see cref="FactAttribute"/> that provides a default timeout.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class DefaultTimeoutFactAttribute : FactAttribute
    {
        public DefaultTimeoutFactAttribute()
        {
            Timeout = TimeoutConstant.DefaultTimeout;
        }
    }
}
