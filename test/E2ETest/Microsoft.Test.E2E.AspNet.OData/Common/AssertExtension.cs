//-----------------------------------------------------------------------------
// <copyright file="AssertExtension.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.Common
{
    public static class AssertExtension
    {
        public static void PrimitiveEqual(object expected, object actual)
        {
            if (expected == null)
            {
                Assert.Null(actual);
                return;
            }
            else
            {
                Assert.NotNull(actual);
            }

            Assert.Equal(expected.GetType(), actual.GetType());
            if (expected.GetType().IsPrimitive)
            {
                if (expected.GetType() == typeof(double))
                {
                    DoubleEqual((double)expected, (double)actual);
                    return;
                }
                if (expected.GetType() == typeof(float))
                {
                    SingleEqual((float)expected, (float)actual);
                    return;
                }

                Assert.Equal(expected, actual);
                return;
            }

            if (expected.GetType() == typeof(string))
            {
                Assert.Equal(expected, actual);
                return;
            }

            var props = expected.GetType().GetProperties().Where(p => p.PropertyType.IsPrimitive || p.PropertyType == typeof(string));
            foreach (var prop in props)
            {
                if (prop.PropertyType == typeof(double))
                {
                    DoubleEqual((double)prop.GetValue(expected, null), (double)prop.GetValue(actual, null));
                }
                else if (prop.PropertyType == typeof(float))
                {
                    SingleEqual((float)prop.GetValue(expected, null), (float)prop.GetValue(actual, null));
                }
                else
                {
                    Assert.Equal(prop.GetValue(expected, null), prop.GetValue(actual, null));
                }
            }
        }

        public static void DoubleEqual(double expected, double actual)
        {
            Assert.True(double.IsPositiveInfinity(expected) && double.IsPositiveInfinity(actual) ||
                double.IsNegativeInfinity(expected) && double.IsNegativeInfinity(actual) ||
                double.IsNaN(expected) && double.IsNaN(actual) ||
                Math.Abs(expected - actual) < 0.0000000001);
        }

        public static void SingleEqual(float expected, float actual)
        {
            Assert.True(float.IsPositiveInfinity(expected) && float.IsPositiveInfinity(actual) ||
                float.IsNegativeInfinity(expected) && float.IsNegativeInfinity(actual) ||
                float.IsNaN(expected) && float.IsNaN(actual) ||
                Math.Abs(expected - actual) < 0.00001);
        }

        public static void DeepEqual(object expected, object actual, Dictionary<object, object> visited = null)
        {
            if (visited == null)
            {
                visited = new Dictionary<object, object>();
            }

            if (expected == null)
            {
                Assert.Null(actual);
                return;
            }
            else
            {
                Assert.NotNull(actual);
            }

            Assert.Equal(expected.GetType(), actual.GetType());

            if (expected.GetType().IsPrimitive)
            {
                if (expected.GetType() == typeof(double))
                {
                    DoubleEqual((double)expected, (double)actual);
                    return;
                }

                if (expected.GetType() == typeof(float))
                {
                    SingleEqual((float)expected, (float)actual);
                    return;
                }

                Assert.Equal(expected, actual);
                return;
            }

            if (expected.GetType() == typeof(string))
            {
                Assert.Equal(expected, actual);
                return;
            }

            if (visited.ContainsKey(expected) && visited[expected] == actual)
            {
                return;
            }

            visited.Add(expected, actual);

            var expectedCollection = expected as IEnumerable;
            if (expectedCollection != null)
            {
                var expectedEnumerator = expectedCollection.GetEnumerator();
                var actualEnumerator = (actual as IEnumerable).GetEnumerator();
                bool hasNext = expectedEnumerator.MoveNext();
                Assert.Equal(hasNext, actualEnumerator.MoveNext());
                while (hasNext)
                {
                    DeepEqual(expectedEnumerator.Current, actualEnumerator.Current);
                    hasNext = expectedEnumerator.MoveNext();
                    Assert.Equal(hasNext, actualEnumerator.MoveNext());
                }
                return;
            }

            var props = expected.GetType().GetProperties();
            foreach (var prop in props)
            {
                DeepEqual(prop.GetValue(expected, null), prop.GetValue(actual, null), visited);
            }
        }
    }
}
