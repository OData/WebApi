// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using Xunit.Extensions;

namespace Microsoft.TestCommon
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class DefaultTimeoutTheoryAttribute : TheoryAttribute
    {
        public DefaultTimeoutTheoryAttribute()
        {
            Timeout = TimeoutConstant.DefaultTimeout;
        }
    }
}
