// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;

namespace Microsoft.Test.AspNet.OData.TestCommon
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class TraitAttribute : Xunit.TraitAttribute
    {
        public TraitAttribute(string name, string value)
            : base(name, value)
        {
        }
    }
}
