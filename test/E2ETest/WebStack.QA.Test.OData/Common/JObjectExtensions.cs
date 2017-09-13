// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace WebStack.QA.Test.OData.Common
{
    public static class JObjectExtensions
    {
        public static bool IsSpecialValue(this JObject self)
        {
            if (self == null)
            {
                throw new ArgumentNullException("self");
            }

            return self.Properties().Any(p =>
            {
                var value = (string)p.Value;
                return p.Name == "value" &&
                (value.Equals("INF", StringComparison.InvariantCulture)
                || value.Equals("-INF", StringComparison.InvariantCulture)
                || value.Equals("NaN", StringComparison.InvariantCulture));
            });
        }
    }
}
