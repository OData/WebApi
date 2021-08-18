//-----------------------------------------------------------------------------
// <copyright file="JsonAssert.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.Common
{
    public class JsonAssert
    {
        public static void PropertyEquals<T, TJobject>(T expectedValue, string propertyName, TJobject jsonObject) where TJobject : JObject
        {
            Func<KeyValuePair<string, JToken>, string> selector = x => x.Key;
            IEnumerable<string> keys = Enumerable.Select<KeyValuePair<string, JToken>, string>(jsonObject, selector);
            Assert.Contains(propertyName, keys);
            Assert.Equal(expectedValue, (T)(dynamic)jsonObject[propertyName]);
        }

        public static void ArrayLength(int expected, string jsonProperty, JObject actual)
        {
            Func<KeyValuePair<string, JToken>, string> selector = x => x.Key;
            IEnumerable<string> keys = Enumerable.Select<KeyValuePair<string, JToken>, string>(actual, selector);
            Assert.Contains(jsonProperty, keys);
            Assert.Equal(expected, ((JArray)actual[jsonProperty]).Count);
        }

        public static void DoesNotContainProperty(string expected, JObject actual)
        {
            Func<KeyValuePair<string, JToken>, string> selector = x => x.Key;
            IEnumerable<string> keys = Enumerable.Select<KeyValuePair<string, JToken>, string>(actual, selector);
            Assert.DoesNotContain(expected, keys, new ContainsEqualityComparer());
        }

        public static void ContainsProperty(string expected, JObject actual)
        {
            Func<KeyValuePair<string, JToken>, string> selector = x => x.Key;
            IEnumerable<string> keys = Enumerable.Select<KeyValuePair<string, JToken>, string>(actual, selector);
            Assert.Contains(expected, keys, new ContainsEqualityComparer());
        }

        private class ContainsEqualityComparer : IEqualityComparer<string>
        {
            public bool Equals(string x, string y)
            {
                if (x == null)
                {
                    throw new ArgumentNullException("x");
                }
                if (y == null)
                {
                    throw new ArgumentNullException("y");
                }
                return Regex.IsMatch(y, x);
            }

            public int GetHashCode(string obj)
            {
                throw new InvalidOperationException("Not supported on this comparer");
            }
        }
    }
}
