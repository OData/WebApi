//-----------------------------------------------------------------------------
// <copyright file="JObjectExtensions.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Microsoft.Test.E2E.AspNet.OData.Common
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
