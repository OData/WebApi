//-----------------------------------------------------------------------------
// <copyright file="SelectTypeExplosionTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.Test.E2E.AspNet.OData.Common;
using Microsoft.Test.E2E.AspNet.OData.Common.Controllers;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.QueryComposition
{
    public class SelectTypeExplosionTests : WebHostTestBase
    {
        public SelectTypeExplosionTests(WebHostTestFixture fixture)
            :base(fixture)
        {
        }

        protected override void UpdateConfiguration(WebRouteConfiguration config)
        {
            config.Count().Filter().OrderBy().Expand().MaxTop(null);
#if NETCORE
            config.MapHttpRoute("api", "{controller}/{action=Get}");
#else
            config.MapHttpRoute("api", "{controller}");
#endif
            config.EnableDependencyInjection();
        }

        public static TheoryDataSet<string> Queries
        {
            get
            {
                TheoryDataSet<string> result = new TheoryDataSet<string>();
                IList<string> propertyNames = typeof(TypeWithManyProperties).GetProperties().Select(p => p.Name).ToList();
                IEnumerable<IEnumerable<string>> combinations = new CombinationGenerator<string>(propertyNames, 3);
                IEnumerable<IEnumerable<string>> queries = combinations.SelectMany(c => new PermutationGenerator<string>(c));
                IEnumerable<string> selectQueries = queries.Select(p => string.Concat(p.Select((w, i) => w + (i == p.Count() - 1 ? "" : ","))));
                foreach (var query in selectQueries)
                {
                    result.Add(query);
                }
                return result;
            }
        }

        private class CombinationGenerator<T> : IEnumerable<IEnumerable<T>>
        {
            private T[] _setOfValues;
            private int _startIndex;
            private int _numberOfElements;
            public CombinationGenerator(IEnumerable<T> setOfValues, int numberOfElements) : this(setOfValues, numberOfElements, 0) { }

            private CombinationGenerator(IEnumerable<T> setOfValues, int numberOfElements, int startIndex)
            {
                if (setOfValues == null)
                {
                    throw new ArgumentNullException("setOfValues");
                }
                _setOfValues = setOfValues.ToArray();
                _numberOfElements = numberOfElements;
                _startIndex = startIndex;
            }

            public IEnumerator<IEnumerable<T>> GetEnumerator()
            {
                if (_numberOfElements == 1)
                {
                    foreach (var element in _setOfValues)
                    {
                        yield return Enumerable.Repeat(element, 1);
                    }
                }
                else
                {
                    for (int i = _startIndex; i < _setOfValues.Length; i++)
                    {
                        T element = _setOfValues.ElementAt(i);
                        var otherValues = _setOfValues.Skip(i + 1);
                        CombinationGenerator<T> restOfTheCombinations = new CombinationGenerator<T>(otherValues, _numberOfElements - 1);
                        foreach (var combination in restOfTheCombinations)
                        {
                            yield return Enumerable.Repeat(element, 1).Concat(combination);
                        }
                    }
                }
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }
        }

        private class PermutationGenerator<T> : IEnumerable<IEnumerable<T>>
        {
            private readonly T[] _setOfValues;
            private int _startLevel;

            public PermutationGenerator(IEnumerable<T> setOfValues)
                : this(setOfValues, 0)
            {
            }

            protected PermutationGenerator(IEnumerable<T> setOfValues, int startLevel)
            {
                if (setOfValues == null)
                {
                    throw new ArgumentNullException("setOfValues");
                }
                _setOfValues = setOfValues.ToArray();
                if (_setOfValues.Length < startLevel)
                {
                    throw new ArgumentOutOfRangeException("startLevel");
                }
                _startLevel = startLevel;
            }

            public IEnumerator<IEnumerable<T>> GetEnumerator()
            {
                if (_startLevel + 1 == _setOfValues.Length)
                {
                    yield return Enumerable.Repeat(_setOfValues.ElementAt(_startLevel), 1);
                }
                else
                {
                    PermutationGenerator<T> permutations = new PermutationGenerator<T>(_setOfValues, _startLevel + 1);
                    foreach (var permutation in permutations.ToList())
                    {
                        foreach (var newPermutation in Mix(_setOfValues[_startLevel], permutation).ToList())
                        {
                            yield return newPermutation;
                        }
                    }
                }
            }

            private IEnumerable<IEnumerable<T>> Mix(T element, IEnumerable<T> array)
            {
                for (int i = 0; i <= array.Count(); i++)
                {
                    yield return InsertAt(element, i, array);
                }
            }

            private IEnumerable<T> InsertAt(T element, int position, IEnumerable<T> array)
            {
                for (int i = 0; i < array.Count(); i++)
                {
                    if (i == position)
                    {
                        yield return element;
                    }
                    yield return array.ElementAt(i);
                }
                if (position == array.Count())
                {
                    yield return element;
                }
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }
        }

        //[Fact]
        //public void CombinatorsWork()
        //{
        //    char[] letters = "abcdef".ToCharArray();
        //    for (int i = 1; i <= letters.Length + 1; i++)
        //    {
        //        string[] combinations = new CombinationGenerator<char>(letters, i).Select(ca => new string(ca.ToArray())).ToArray();
        //        string[] permutedCombinations = combinations.SelectMany(c => new PermutationGenerator<char>(c).Select(ca => new string(ca.ToArray()))).ToArray();
        //    }
        //}

        [Theory]
        [MemberData(nameof(Queries))]
        public async Task ServerDoesntCreateAnInfiniteAmmountOfTypes(string query)
        {
            string requestUrl = BaseAddress + "/TypeWithManyProperties?$select=" + query;
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            HttpResponseMessage response = await Client.SendAsync(request);
            response.EnsureSuccessStatusCode();
        }
    }

    public class TypeWithManyProperties
    {
        public int Property0 { get; set; }
        public string Property1 { get; set; }
        public bool Property2 { get; set; }
        //public DateTime Property3 { get; set; }
        public DateTimeOffset Property4 { get; set; }
        public char Property5 { get; set; }
        //public long Property6 { get; set; }
        //public short Property7 { get; set; }
        //public double Property8 { get; set; }
        //public float Property9 { get; set; }
        //public decimal Property10 { get; set; }
        //public int? Property11 { get; set; }
        //public char[] Property12 { get; set; }
        //public bool? Property13 { get; set; }
        //public DateTime? Property14 { get; set; }
        //public DateTimeOffset? Property15 { get; set; }
        //public char? Property16 { get; set; }
        //public long? Property17 { get; set; }
        //public short? Property18 { get; set; }
        //public double? Property19 { get; set; }
        //public float? Property20 { get; set; }
    }

    public class TypeWithManyPropertiesController : TestNonODataController
    {
        public ITestActionResult Get()
        {
            return Ok(Enumerable.Range(0, 10).Select(i =>
                    new TypeWithManyProperties
                    {
                        Property0 = i,
                        Property1 = "string " + i,
                        Property2 = i % 2 == 0,
                        //Property3 = DateTime.Now.AddHours(-i),
                        Property4 = DateTimeOffset.Now.AddHours(-i * 2),
                        Property5 = Convert.ToChar(i),
                        //Property6 = i * 3,
                        //Property7 = (short)(i * 5),
                        //Property8 = i * 7,
                        //Property9 = i * 11,
                        //Property10 = i * 13,
                        //Property11 = i * 17,
                        //Property12 = new char[] { Convert.ToChar(i) },
                        //Property13 = i % 2 == 1,
                        //Property14 = DateTime.Now.AddHours(i * 19),
                        //Property15 = DateTimeOffset.Now.AddHours(i * 23),
                        //Property16 = Convert.ToChar(i * 3),
                        //Property17 = i * 29,
                        //Property18 = (short?)(i * 31),
                        //Property19 = i * 37,
                        //Property20 = i * 41
                    })
                );
        }
    }
}
