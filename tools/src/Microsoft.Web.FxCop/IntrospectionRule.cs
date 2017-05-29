// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.FxCop.Sdk;

namespace Microsoft.Web.FxCop
{
    public abstract class IntrospectionRule : BaseIntrospectionRule
    {
        protected IntrospectionRule(string name)
            : base(name, "Microsoft.Web.FxCop.Rules", typeof(IntrospectionRule).Assembly)
        {
        }
    }
}
