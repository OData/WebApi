// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.TestCommon;

namespace System.Web.Http.OData.Query
{
    public class AllowedFunctionNamesTest
    {
        public static TheoryDataSet<AllowedFunctionNames> AllStringFunctionsData
        {
            get
            {
                return new TheoryDataSet<AllowedFunctionNames>
                {
                    AllowedFunctionNames.Concat,
                    AllowedFunctionNames.IndexOf,
                    AllowedFunctionNames.Length,
                    AllowedFunctionNames.StartsWith,
                    AllowedFunctionNames.EndsWith,
                    AllowedFunctionNames.Substring,
                    AllowedFunctionNames.SubstringOf,
                    AllowedFunctionNames.ToLower,
                    AllowedFunctionNames.ToUpper,
                    AllowedFunctionNames.Trim,
                };
            }
        }

        public static IEnumerable<object[]> AllNonStringFunctionsData
        {
            get
            {
                return
                    new List<AllowedFunctionNames>(Enum.GetValues(typeof(AllowedFunctionNames)) as AllowedFunctionNames[])
                    .Except(new[] { AllowedFunctionNames.AllFunctionNames, AllowedFunctionNames.AllStringFunctionNames })
                    .Except(AllStringFunctionsData.Select(t => (AllowedFunctionNames)t[0]))
                    .Select(t => new object[] { t });
            }
        }

        public static TheoryDataSet<AllowedFunctionNames> AllDateTimeFunctionsData
        {
            get
            {
                return new TheoryDataSet<AllowedFunctionNames>
                {
                    AllowedFunctionNames.Year,
                    AllowedFunctionNames.Years,
                    AllowedFunctionNames.Month,
                    AllowedFunctionNames.Months,
                    AllowedFunctionNames.Day,
                    AllowedFunctionNames.Days,
                    AllowedFunctionNames.Hour,
                    AllowedFunctionNames.Hours,
                    AllowedFunctionNames.Minute,
                    AllowedFunctionNames.Minutes,
                    AllowedFunctionNames.Second,
                    AllowedFunctionNames.Seconds,
                };
            }
        }

        public static IEnumerable<object[]> AllNonDateTimeFunctionsData
        {
            get
            {
                return
                    new List<AllowedFunctionNames>(Enum.GetValues(typeof(AllowedFunctionNames)) as AllowedFunctionNames[])
                    .Except(new[] { AllowedFunctionNames.AllFunctionNames, AllowedFunctionNames.AllDateTimeFunctionNames })
                    .Except(AllDateTimeFunctionsData.Select(t => (AllowedFunctionNames)t[0]))
                    .Select(t => new object[] { t });
            }
        }

        public static TheoryDataSet<AllowedFunctionNames> AllMathFunctionsData
        {
            get
            {
                return new TheoryDataSet<AllowedFunctionNames>
                {
                    AllowedFunctionNames.Ceiling,
                    AllowedFunctionNames.Round,
                    AllowedFunctionNames.Floor
                };
            }
        }

        public static IEnumerable<object[]> AllNonMathFunctionsData
        {
            get
            {
                return
                    new List<AllowedFunctionNames>(Enum.GetValues(typeof(AllowedFunctionNames)) as AllowedFunctionNames[])
                    .Except(new[] { AllowedFunctionNames.AllFunctionNames, AllowedFunctionNames.AllMathFunctionNames })
                    .Except(AllMathFunctionsData.Select(t => (AllowedFunctionNames)t[0]))
                    .Select(t => new object[] { t });
            }
        }

        [Fact]
        public void None_MatchesNone()
        {
            Assert.Equal(AllowedFunctionNames.None, AllowedFunctionNames.AllFunctionNames & AllowedFunctionNames.None);
        }

        [Theory]
        [PropertyData("AllStringFunctionsData")]
        public void AllStringFunctionNames_Has_AllStringFunctions(AllowedFunctionNames stringFunction)
        {
            Assert.Equal(stringFunction, AllowedFunctionNames.AllStringFunctionNames & stringFunction);
        }

        [Theory]
        [PropertyData("AllNonStringFunctionsData")]
        public void AllStringFunctionNames_DoesNot_Have_NonStringFunctions(AllowedFunctionNames nonStringFunction)
        {
            Assert.Equal(AllowedFunctionNames.None, AllowedFunctionNames.AllStringFunctionNames & nonStringFunction);
        }

        [Theory]
        [PropertyData("AllDateTimeFunctionsData")]
        public void AllDateTimeFunctionNames_Has_AllDateFunctions(AllowedFunctionNames dateFunction)
        {
            Assert.Equal(dateFunction, AllowedFunctionNames.AllDateTimeFunctionNames & dateFunction);
        }

        [Theory]
        [PropertyData("AllNonDateTimeFunctionsData")]
        public void AllDateTimeFunctionNames_DoesNot_Have_NontringFunctions(AllowedFunctionNames nonDateTimeFunction)
        {
            Assert.Equal(AllowedFunctionNames.None, AllowedFunctionNames.AllDateTimeFunctionNames & nonDateTimeFunction);
        }

        [Theory]
        [PropertyData("AllMathFunctionsData")]
        public void AllMathFunctionNames_Has_AllMathFunctions(AllowedFunctionNames mathFunction)
        {
            Assert.Equal(mathFunction, AllowedFunctionNames.AllMathFunctionNames & mathFunction);
        }

        [Theory]
        [PropertyData("AllNonMathFunctionsData")]
        public void AllMathFunctionNames_DoesNot_Have_NonMathFunctions(AllowedFunctionNames nonMathFunction)
        {
            Assert.Equal(AllowedFunctionNames.None, AllowedFunctionNames.AllMathFunctionNames & nonMathFunction);
        }

        [Fact]
        public void AllFunctionNames_Contains_AllFunctionNames()
        {
            AllowedFunctionNames allFunctionNames = 0;
            foreach (AllowedFunctionNames allowedFunctionName in Enum.GetValues(typeof(AllowedFunctionNames)))
            {
                if (allowedFunctionName != AllowedFunctionNames.AllFunctionNames)
                {
                    allFunctionNames = allFunctionNames | allowedFunctionName;
                }
            }

            Assert.Equal(allFunctionNames, AllowedFunctionNames.AllFunctionNames);
        }
    }
}
