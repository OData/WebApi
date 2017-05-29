using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace WebStack.QA.Test.OData.Common
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
            if (expected.GetType().IsPrimitive || expected.GetType() == typeof(string))
            {
                Assert.Equal(expected, actual);
                return;
            }

            var props = expected.GetType().GetProperties().Where(p => p.PropertyType.IsPrimitive || p.PropertyType == typeof(string));
            foreach (var prop in props)
            {
                Assert.Equal(prop.GetValue(expected, null), prop.GetValue(actual, null));
            }
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

            if (expected.GetType().IsPrimitive || expected.GetType() == typeof(string))
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
