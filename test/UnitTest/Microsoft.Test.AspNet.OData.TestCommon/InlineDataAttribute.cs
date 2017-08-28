// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;

namespace Microsoft.Test.AspNet.OData.TestCommon
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class InlineDataAttribute : Xunit.Extensions.InlineDataAttribute
    {
        public InlineDataAttribute(params object[] dataValues)
            : base(dataValues)
        {
        }
    }
}
