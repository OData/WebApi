﻿using System.Collections;
using System.Collections.Generic;

namespace Microsoft.Test.E2E.AspNet.OData.Common
{
    /// <summary>
    /// Helper class for generating test data for XUnit's <see cref="Xunit.Extensions.TheoryAttribute"/>-based tests.
    /// Should be used in combination with <see cref="Xunit.Extensions.PropertyDataAttribute"/>.
    /// </summary>
    /// <typeparam name="TParam">First parameter type</typeparam>
    public class TheoryDataSet<TParam> : TheoryDataSet
    {
        public void Add(TParam p)
        {
            AddItem(p);
        }
    }

    /// <summary>
    /// Helper class for generating test data for XUnit's <see cref="Xunit.Extensions.TheoryAttribute"/>-based tests.
    /// Should be used in combination with <see cref="Xunit.Extensions.PropertyDataAttribute"/>.
    /// </summary>
    /// <typeparam name="TParam1">First parameter type</typeparam>
    /// <typeparam name="TParam2">Second parameter type</typeparam>
    public class TheoryDataSet<TParam1, TParam2> : TheoryDataSet
    {
        public void Add(TParam1 p1, TParam2 p2)
        {
            AddItem(p1, p2);
        }
    }

    /// <summary>
    /// Helper class for generating test data for XUnit's <see cref="Xunit.Extensions.TheoryAttribute"/>-based tests.
    /// Should be used in combination with <see cref="Xunit.Extensions.PropertyDataAttribute"/>.
    /// </summary>
    /// <typeparam name="TParam1">First parameter type</typeparam>
    /// <typeparam name="TParam2">Second parameter type</typeparam>
    /// <typeparam name="TParam3">Third parameter type</typeparam>
    public class TheoryDataSet<TParam1, TParam2, TParam3> : TheoryDataSet
    {
        public void Add(TParam1 p1, TParam2 p2, TParam3 p3)
        {
            AddItem(p1, p2, p3);
        }
    }

    /// <summary>
    /// Helper class for generating test data for XUnit's <see cref="Xunit.Extensions.TheoryAttribute"/>-based tests.
    /// Should be used in combination with <see cref="Xunit.Extensions.PropertyDataAttribute"/>.
    /// </summary>
    /// <typeparam name="TParam1">First parameter type</typeparam>
    /// <typeparam name="TParam2">Second parameter type</typeparam>
    /// <typeparam name="TParam3">Third parameter type</typeparam>
    /// <typeparam name="TParam4">Fourth parameter type</typeparam>
    public class TheoryDataSet<TParam1, TParam2, TParam3, TParam4> : TheoryDataSet
    {
        public void Add(TParam1 p1, TParam2 p2, TParam3 p3, TParam4 p4)
        {
            AddItem(p1, p2, p3, p4);
        }
    }

    /// <summary>
    /// Base class for <c>TheoryDataSet</c> classes.
    /// </summary>
    public abstract class TheoryDataSet : IEnumerable<object[]>
    {
        private readonly List<object[]> data = new List<object[]>();

        protected void AddItem(params object[] values)
        {
            data.Add(values);
        }

        public IEnumerator<object[]> GetEnumerator()
        {
            return data.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
