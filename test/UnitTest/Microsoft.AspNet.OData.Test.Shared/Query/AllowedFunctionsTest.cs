﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNet.OData.Test.Common;
using Xunit;

namespace Microsoft.AspNet.OData.Test.Query
{
    public class AllowedFunctionsTest
    {
        public static TheoryDataSet<AllowedFunctions> AllStringFunctionsData
        {
            get
            {
                return new TheoryDataSet<AllowedFunctions>
                {
                    AllowedFunctions.Concat,
                    AllowedFunctions.IndexOf,
                    AllowedFunctions.Length,
                    AllowedFunctions.StartsWith,
                    AllowedFunctions.EndsWith,
                    AllowedFunctions.Substring,
                    AllowedFunctions.Contains,
                    AllowedFunctions.ToLower,
                    AllowedFunctions.ToUpper,
                    AllowedFunctions.Trim,
                };
            }
        }

        public static IEnumerable<object[]> AllNonStringFunctionsData
        {
            get
            {
                return
                    new List<AllowedFunctions>(Enum.GetValues(typeof(AllowedFunctions)) as AllowedFunctions[])
                    .Except(new[] { AllowedFunctions.AllFunctions, AllowedFunctions.AllStringFunctions })
                    .Except(AllStringFunctionsData.Select(t => (AllowedFunctions)t[0]))
                    .Select(t => new object[] { t });
            }
        }

        public static TheoryDataSet<AllowedFunctions> AllDateTimeFunctionsData
        {
            get
            {
                return new TheoryDataSet<AllowedFunctions>
                {
                    AllowedFunctions.Year,
                    AllowedFunctions.Month,
                    AllowedFunctions.Day,
                    AllowedFunctions.Hour,
                    AllowedFunctions.Minute,
                    AllowedFunctions.Second,
                    AllowedFunctions.FractionalSeconds,
                    AllowedFunctions.Date,
                    AllowedFunctions.Time,
                };
            }
        }

        public static IEnumerable<object[]> AllNonDateTimeFunctionsData
        {
            get
            {
                return
                    new List<AllowedFunctions>(Enum.GetValues(typeof(AllowedFunctions)) as AllowedFunctions[])
                    .Except(new[] { AllowedFunctions.AllFunctions, AllowedFunctions.AllDateTimeFunctions })
                    .Except(AllDateTimeFunctionsData.Select(t => (AllowedFunctions)t[0]))
                    .Select(t => new object[] { t });
            }
        }

        public static TheoryDataSet<AllowedFunctions> AllMathFunctionsData
        {
            get
            {
                return new TheoryDataSet<AllowedFunctions>
                {
                    AllowedFunctions.Ceiling,
                    AllowedFunctions.Round,
                    AllowedFunctions.Floor
                };
            }
        }

        public static IEnumerable<object[]> AllNonMathFunctionsData
        {
            get
            {
                return
                    new List<AllowedFunctions>(Enum.GetValues(typeof(AllowedFunctions)) as AllowedFunctions[])
                    .Except(new[] { AllowedFunctions.AllFunctions, AllowedFunctions.AllMathFunctions })
                    .Except(AllMathFunctionsData.Select(t => (AllowedFunctions)t[0]))
                    .Select(t => new object[] { t });
            }
        }

        [Fact]
        public void None_MatchesNone()
        {
            Assert.Equal(AllowedFunctions.None, AllowedFunctions.AllFunctions & AllowedFunctions.None);
        }

        [Theory]
        [MemberData(nameof(AllStringFunctionsData))]
        public void AllStringFunctions_Has_AllStringFunctions(AllowedFunctions stringFunction)
        {
            Assert.Equal(stringFunction, AllowedFunctions.AllStringFunctions & stringFunction);
        }

        [Theory]
        [MemberData(nameof(AllNonStringFunctionsData))]
        public void AllStringFunctions_DoesNot_Have_NonStringFunctions(AllowedFunctions nonStringFunction)
        {
            Assert.Equal(AllowedFunctions.None, AllowedFunctions.AllStringFunctions & nonStringFunction);
        }

        [Theory]
        [MemberData(nameof(AllDateTimeFunctionsData))]
        public void AllDateTimeFunctions_Has_AllDateFunctions(AllowedFunctions dateFunction)
        {
            Assert.Equal(dateFunction, AllowedFunctions.AllDateTimeFunctions & dateFunction);
        }

        [Theory]
        [MemberData(nameof(AllNonDateTimeFunctionsData))]
        public void AllDateTimeFunctions_DoesNot_Have_NontringFunctions(AllowedFunctions nonDateTimeFunction)
        {
            Assert.Equal(AllowedFunctions.None, AllowedFunctions.AllDateTimeFunctions & nonDateTimeFunction);
        }

        [Theory]
        [MemberData(nameof(AllMathFunctionsData))]
        public void AllMathFunctions_Has_AllMathFunctions(AllowedFunctions mathFunction)
        {
            Assert.Equal(mathFunction, AllowedFunctions.AllMathFunctions & mathFunction);
        }

        [Theory]
        [MemberData(nameof(AllNonMathFunctionsData))]
        public void AllMathFunctions_DoesNot_Have_NonMathFunctions(AllowedFunctions nonMathFunction)
        {
            Assert.Equal(AllowedFunctions.None, AllowedFunctions.AllMathFunctions & nonMathFunction);
        }

        [Fact]
        public void AllFunctions_Contains_AllFunctionNames()
        {
            AllowedFunctions allFunctions = 0;
            foreach (AllowedFunctions allowedFunction in Enum.GetValues(typeof(AllowedFunctions)))
            {
                if (allowedFunction != AllowedFunctions.AllFunctions)
                {
                    allFunctions = allFunctions | allowedFunction;
                }
            }

            Assert.Equal(AllowedFunctions.AllFunctions, allFunctions);
        }
    }
}
