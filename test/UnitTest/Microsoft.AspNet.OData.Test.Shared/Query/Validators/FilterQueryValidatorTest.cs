//-----------------------------------------------------------------------------
// <copyright file="FilterQueryValidatorTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNet.OData.Query.Validators;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.AspNet.OData.Test.Query.Expressions;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Xunit;

namespace Microsoft.AspNet.OData.Test.Query.Validators
{
    public class FilterQueryValidatorTest
    {
        private MyFilterValidator _validator;
        private ODataValidationSettings _settings = new ODataValidationSettings();
        private ODataQueryContext _context;
        private ODataQueryContext _productContext;

        public static TheoryDataSet<string> LongInputs
        {
            get
            {
                return GetLongInputsTestData(100);
            }
        }

        public static TheoryDataSet<string> CloseToLongInputs
        {
            get
            {
                return GetLongInputsTestData(95);
            }
        }

        public static TheoryDataSet<string> NestedAnyAllInputs
        {
            get
            {
                return new TheoryDataSet<string>
                {
                    "Category/QueryableProducts/any(P: P/Category/EnumerableProducts/any(PP: PP/ProductName eq 'Snacks'))",
                    "Category/QueryableProducts/all(P: P/Category/EnumerableProducts/all(PP: PP/ProductName eq 'Snacks'))",
                    "Category/QueryableProducts/any(P: P/Category/EnumerableProducts/all(PP: PP/ProductName eq 'Snacks'))",
                    "Category/QueryableProducts/all(P: P/Category/EnumerableProducts/any(PP: PP/ProductName eq 'Snacks'))",
                };
            }
        }

        public static TheoryDataSet<AllowedArithmeticOperators, string, string> ArithmeticOperators
        {
            get
            {
                return new TheoryDataSet<AllowedArithmeticOperators, string, string>
                {
                    { AllowedArithmeticOperators.Add, "UnitPrice add 0 eq 23", "Add" },
                    { AllowedArithmeticOperators.Divide, "UnitPrice div 23 eq 1", "Divide" },
                    { AllowedArithmeticOperators.Modulo, "UnitPrice mod 23 eq 0", "Modulo" },
                    { AllowedArithmeticOperators.Multiply, "UnitPrice mul 1 eq 23", "Multiply" },
                    { AllowedArithmeticOperators.Subtract, "UnitPrice sub 0 eq 23", "Subtract" },
                };
            }
        }

        public static TheoryDataSet<string> ArithmeticOperators_CheckArguments
        {
            get
            {
                return new TheoryDataSet<string>
                {
                    { "day(DiscontinuedDate) add 0 eq 23" },
                    { "day(DiscontinuedDate) div 23 eq 1" },
                    { "day(DiscontinuedDate) mod 23 eq 0" },
                    { "day(DiscontinuedDate) mul 1 eq 23" },
                    { "day(DiscontinuedDate) sub 0 eq 23" },
                    { "0 add day(DiscontinuedDate) eq 23" },
                    { "23 div day(DiscontinuedDate) eq 1" },
                    { "23 mod day(DiscontinuedDate) eq 0" },
                    { "1 mul day(DiscontinuedDate) eq 23" },
                    { "0 sub day(DiscontinuedDate) eq -23" },
                };
            }
        }

        // No support for OData v4 functions:
        // maxdatetime, mindatetime, now, totaloffsetminutes, or totalseconds?
        public static TheoryDataSet<AllowedFunctions, string, string> DateTimeFunctions
        {
            get
            {
                return new TheoryDataSet<AllowedFunctions, string, string>
                {
                    { AllowedFunctions.Day, "day(null) eq 20", "day" },
                    { AllowedFunctions.Day, "day(DiscontinuedDate) eq 20", "day" },
                    { AllowedFunctions.Hour, "hour(null) eq 10", "hour" },
                    { AllowedFunctions.Hour, "hour(DiscontinuedDate) eq 10", "hour" },
                    { AllowedFunctions.Minute, "minute(null) eq 20", "minute" },
                    { AllowedFunctions.Minute, "minute(DiscontinuedDate) eq 20", "minute" },
                    { AllowedFunctions.Month, "month(null) eq 10", "month" },
                    { AllowedFunctions.Month, "month(DiscontinuedDate) eq 10", "month" },
                    { AllowedFunctions.Second, "second(null) eq 20", "second" },
                    { AllowedFunctions.Second, "second(DiscontinuedDate) eq 20", "second" },
                    { AllowedFunctions.Year, "year(null) eq 2000", "year" },
                    { AllowedFunctions.Year, "year(DiscontinuedDate) eq 2000", "year" },
                    { AllowedFunctions.Date, "date(null) eq 2015-02-26", "date" },
                    { AllowedFunctions.Date, "date(DiscontinuedDate) eq 2015-02-26", "date" },
                    { AllowedFunctions.Time, "time(null) eq 03:4:05.90100", "time" },
                    { AllowedFunctions.Time, "time(DiscontinuedDate) eq 03:4:05.90100", "time" },
                    { AllowedFunctions.FractionalSeconds, "fractionalseconds(null) eq 0.05", "fractionalseconds" },
                    { AllowedFunctions.FractionalSeconds, "fractionalseconds(DiscontinuedDate) eq 0.050", "fractionalseconds" },
                };
            }
        }

        public static TheoryDataSet<AllowedFunctions, string, string> MathFunctions
        {
            get
            {
                return new TheoryDataSet<AllowedFunctions, string, string>
                {
                    { AllowedFunctions.Ceiling, "ceiling(null) eq 0", "ceiling" },
                    { AllowedFunctions.Ceiling, "ceiling(Weight) eq 0", "ceiling" },
                    { AllowedFunctions.Floor, "floor(null) eq 0", "floor" },
                    { AllowedFunctions.Floor, "floor(Weight) eq 0", "floor" },
                    { AllowedFunctions.Round, "round(null) eq 0", "round" },
                    { AllowedFunctions.Round, "round(Weight) eq 0", "round" },
                };
            }
        }

        public static TheoryDataSet<AllowedFunctions, string, string> OtherFunctions
        {
            get
            {
                return new TheoryDataSet<AllowedFunctions, string, string>
                {
                    { AllowedFunctions.All, "AlternateIDs/all(t : null eq 1)", "all" },
                    { AllowedFunctions.All, "AlternateIDs/all(t : t eq 1)", "all" },
                    { AllowedFunctions.All, "AlternateAddresses/all(t : null eq 'Redmond')", "all" },
                    { AllowedFunctions.All, "AlternateAddresses/all(t : t/City eq 'Redmond')", "all" },
                    { AllowedFunctions.All, "Category/QueryableProducts/all(t : null eq 'Name')", "all" },
                    { AllowedFunctions.All, "Category/QueryableProducts/all(t : t/ProductName eq 'Name')", "all" },
                    { AllowedFunctions.All, "Category/EnumerableProducts/all(t : null eq 'Name')", "all" },
                    { AllowedFunctions.All, "Category/EnumerableProducts/all(t : t/ProductName eq 'Name')", "all" },

                    { AllowedFunctions.Any, "AlternateIDs/any()", "any" },
                    { AllowedFunctions.Any, "AlternateIDs/any(t : null eq 1)", "any" },
                    { AllowedFunctions.Any, "AlternateIDs/any(t : t eq 1)", "any" },
                    { AllowedFunctions.Any, "AlternateAddresses/any()", "any" },
                    { AllowedFunctions.Any, "AlternateAddresses/any(t : null eq 'Redmond')", "any" },
                    { AllowedFunctions.Any, "AlternateAddresses/any(t : t/City eq 'Redmond')", "any" },
                    { AllowedFunctions.Any, "Category/QueryableProducts/any()", "any" },
                    { AllowedFunctions.Any, "Category/QueryableProducts/any(t : null eq 'Name')", "any" },
                    { AllowedFunctions.Any, "Category/QueryableProducts/any(t : t/ProductName eq 'Name')", "any" },
                    { AllowedFunctions.Any, "Category/EnumerableProducts/any()", "any" },
                    { AllowedFunctions.Any, "Category/EnumerableProducts/any(t : null eq 'Name')", "any" },
                    { AllowedFunctions.Any, "Category/EnumerableProducts/any(t : t/ProductName eq 'Name')", "any" },

                    { AllowedFunctions.Cast, "cast('Edm.Int64') eq 0", "cast" },
                    { AllowedFunctions.Cast, "cast(Edm.String) eq 'Name'", "cast" },
                    { AllowedFunctions.Cast, "cast('Edm.String') eq 'Name'", "cast" },
                    { AllowedFunctions.Cast, "cast('Microsoft.AspNet.OData.Test.Query.Expressions.Address')/City eq 'Redmond'", "cast" },
                    { AllowedFunctions.Cast, "cast('Microsoft.AspNet.OData.Test.Query.Expressions.DerivedProduct')/DerivedProductName eq 'Name'", "cast" },
                    { AllowedFunctions.Cast, "cast(null,'Edm.Int64') eq 0", "cast" },
                    { AllowedFunctions.Cast, "cast(null, 'Edm.Int64') eq 0", "cast" },
                    { AllowedFunctions.Cast, "cast(null,Edm.String) eq 'Name'", "cast" },
                    { AllowedFunctions.Cast, "cast(null, Edm.String) eq 'Name'", "cast" },
                    { AllowedFunctions.Cast, "cast(null,'Edm.String') eq 'Name'", "cast" },
                    { AllowedFunctions.Cast, "cast(null, 'Edm.String') eq 'Name'", "cast" },
                    { AllowedFunctions.Cast, "cast(null,'Microsoft.AspNet.OData.Test.Common.Types.SimpleEnum') eq Microsoft.AspNet.OData.Test.Common.Types.SimpleEnum'First'", "cast" },
                    { AllowedFunctions.Cast, "cast(null, 'Microsoft.AspNet.OData.Test.Common.Types.SimpleEnum') eq Microsoft.AspNet.OData.Test.Common.Types.SimpleEnum'First'", "cast" },

                    // TODO: uncomment the follow two cases after ODL fix the issues.
                 //   { AllowedFunctions.Cast, "cast(null,'Microsoft.AspNet.OData.Test.Query.Expressions.Address')/City eq 'Redmond'", "cast" },
                //    { AllowedFunctions.Cast, "cast(null, 'Microsoft.AspNet.OData.Test.Query.Expressions.Address')/City eq 'Redmond'", "cast" },
                    { AllowedFunctions.Cast, "cast(Microsoft.AspNet.OData.Test.Common.Types.SimpleEnum'First','Edm.String') eq 'First'", "cast" },
                    { AllowedFunctions.Cast, "cast(Microsoft.AspNet.OData.Test.Common.Types.SimpleEnum'First', 'Edm.String') eq 'First'", "cast" },
                    { AllowedFunctions.Cast, "cast(CategoryID,'Edm.Int64') eq 0", "cast" },
                    { AllowedFunctions.Cast, "cast(CategoryID, 'Edm.Int64') eq 0", "cast" },
                    { AllowedFunctions.Cast, "cast(ReorderLevel,Edm.String) eq 'Name'", "cast" },
                    { AllowedFunctions.Cast, "cast(ReorderLevel, Edm.String) eq 'Name'", "cast" },
                    { AllowedFunctions.Cast, "cast(ReorderLevel,'Edm.String') eq 'Name'", "cast" },
                    { AllowedFunctions.Cast, "cast(ReorderLevel, 'Edm.String') eq 'Name'", "cast" },
                    { AllowedFunctions.Cast, "cast(Ranking,'Edm.String') eq 'First'", "cast" },
                    { AllowedFunctions.Cast, "cast(Ranking, 'Edm.String') eq 'First'", "cast" },
                    { AllowedFunctions.Cast, "cast(ProductName,'Microsoft.AspNet.OData.Test.Common.Types.SimpleEnum') eq Microsoft.AspNet.OData.Test.Common.Types.SimpleEnum'First'", "cast" },
                    { AllowedFunctions.Cast, "cast(ProductName, 'Microsoft.AspNet.OData.Test.Common.Types.SimpleEnum') eq Microsoft.AspNet.OData.Test.Common.Types.SimpleEnum'First'", "cast" },
                    { AllowedFunctions.Cast, "cast(SupplierAddress,'Microsoft.AspNet.OData.Test.Query.Expressions.Address')/City eq 'Redmond'", "cast" },
                    { AllowedFunctions.Cast, "cast(SupplierAddress, 'Microsoft.AspNet.OData.Test.Query.Expressions.Address')/City eq 'Redmond'", "cast" },
                    { AllowedFunctions.Cast, "cast(Category,'Microsoft.AspNet.OData.Test.Query.Expressions.DerivedCategory')/DerivedCategoryName eq 'Name'", "cast" },
                    { AllowedFunctions.Cast, "cast(Category, 'Microsoft.AspNet.OData.Test.Query.Expressions.DerivedCategory')/DerivedCategoryName eq 'Name'", "cast" },

                    { AllowedFunctions.IsOf, "isof('Edm.Int64')", "isof" },
                    { AllowedFunctions.IsOf, "isof(Edm.String)", "isof" },
                    { AllowedFunctions.IsOf, "isof('Edm.String')", "isof" },
                    { AllowedFunctions.IsOf, "isof('Microsoft.AspNet.OData.Test.Common.Types.SimpleEnum')", "isof" },
                    { AllowedFunctions.IsOf, "isof('Microsoft.AspNet.OData.Test.Query.Expressions.Address')", "isof" },
                    { AllowedFunctions.IsOf, "isof('Microsoft.AspNet.OData.Test.Query.Expressions.DerivedCategory')", "isof" },
                    { AllowedFunctions.IsOf, "isof('Microsoft.AspNet.OData.Test.Query.Expressions.DerivedProduct')", "isof" },
                    { AllowedFunctions.IsOf, "isof(null,'Edm.Int64')", "isof" },
                    { AllowedFunctions.IsOf, "isof(null, 'Edm.Int64')", "isof" },
                    { AllowedFunctions.IsOf, "isof(null,Edm.String)", "isof" },
                    { AllowedFunctions.IsOf, "isof(null, Edm.String)", "isof" },
                    { AllowedFunctions.IsOf, "isof(null,'Edm.String')", "isof" },
                    { AllowedFunctions.IsOf, "isof(null, 'Edm.String')", "isof" },
                    { AllowedFunctions.IsOf, "isof(null,'Microsoft.AspNet.OData.Test.Common.Types.SimpleEnum')", "isof" },
                    { AllowedFunctions.IsOf, "isof(null, 'Microsoft.AspNet.OData.Test.Common.Types.SimpleEnum')", "isof" },
                    { AllowedFunctions.IsOf, "isof(null,'Microsoft.AspNet.OData.Test.Query.Expressions.Address')", "isof" },
                    { AllowedFunctions.IsOf, "isof(null, 'Microsoft.AspNet.OData.Test.Query.Expressions.Address')", "isof" },
                    { AllowedFunctions.IsOf, "isof(null,'Microsoft.AspNet.OData.Test.Query.Expressions.DerivedCategory')", "isof" },
                    { AllowedFunctions.IsOf, "isof(null, 'Microsoft.AspNet.OData.Test.Query.Expressions.DerivedCategory')", "isof" },
                    { AllowedFunctions.IsOf, "isof(CategoryID,'Edm.Int64')", "isof" },
                    { AllowedFunctions.IsOf, "isof(CategoryID, 'Edm.Int64')", "isof" },
                    { AllowedFunctions.IsOf, "isof(ReorderLevel,Edm.String)", "isof" },
                    { AllowedFunctions.IsOf, "isof(ReorderLevel, Edm.String)", "isof" },
                    { AllowedFunctions.IsOf, "isof(ReorderLevel,'Edm.String')", "isof" },
                    { AllowedFunctions.IsOf, "isof(ReorderLevel, 'Edm.String')", "isof" },
                    { AllowedFunctions.IsOf, "isof(Ranking,'Microsoft.AspNet.OData.Test.Common.Types.SimpleEnum')", "isof" },
                    { AllowedFunctions.IsOf, "isof(Ranking, 'Microsoft.AspNet.OData.Test.Common.Types.SimpleEnum')", "isof" },
                    { AllowedFunctions.IsOf, "isof(SupplierAddress,'Microsoft.AspNet.OData.Test.Query.Expressions.Address')", "isof" },
                    { AllowedFunctions.IsOf, "isof(SupplierAddress, 'Microsoft.AspNet.OData.Test.Query.Expressions.Address')", "isof" },
                    { AllowedFunctions.IsOf, "isof(Category,'Microsoft.AspNet.OData.Test.Query.Expressions.DerivedCategory')", "isof" },
                    { AllowedFunctions.IsOf, "isof(Category, 'Microsoft.AspNet.OData.Test.Query.Expressions.DerivedCategory')", "isof" },
                };
            }
        }

        public static TheoryDataSet<AllowedFunctions, string, string> OtherFunctions_SomeSingleParameterCasts
        {
            get
            {
                return new TheoryDataSet<AllowedFunctions, string, string>
                {
                    // Single-parameter casts without quotes around the type name.
                    { AllowedFunctions.Cast, "cast(Microsoft.AspNet.OData.Test.Query.Expressions.DerivedProduct)/DerivedProductName eq 'Name'", "cast" },
                    { AllowedFunctions.IsOf, "isof(Microsoft.AspNet.OData.Test.Query.Expressions.DerivedProduct)", "isof" },
                };
            }
        }

        public static TheoryDataSet<AllowedFunctions, string, string> OtherFunctions_SomeTwoParameterCasts
        {
            get
            {
                return new TheoryDataSet<AllowedFunctions, string, string>
                {
                    // Two-parameter casts without quotes around the type name.
                    { AllowedFunctions.Cast, "cast(null,Microsoft.AspNet.OData.Test.Query.Expressions.DerivedCategory)/DerivedCategoryName eq 'Name'", "cast" },
                    { AllowedFunctions.Cast, "cast(null, Microsoft.AspNet.OData.Test.Query.Expressions.DerivedCategory)/DerivedCategoryName eq 'Name'", "cast" },
                    { AllowedFunctions.Cast, "cast(Category,Microsoft.AspNet.OData.Test.Query.Expressions.DerivedCategory)/DerivedCategoryName eq 'Name'", "cast" },
                    { AllowedFunctions.Cast, "cast(Category, Microsoft.AspNet.OData.Test.Query.Expressions.DerivedCategory)/DerivedCategoryName eq 'Name'", "cast" },

                    { AllowedFunctions.IsOf, "isof(null,Microsoft.AspNet.OData.Test.Query.Expressions.DerivedCategory)", "isof" },
                    { AllowedFunctions.IsOf, "isof(null, Microsoft.AspNet.OData.Test.Query.Expressions.DerivedCategory)", "isof" },
                    { AllowedFunctions.IsOf, "isof(Category,Microsoft.AspNet.OData.Test.Query.Expressions.DerivedCategory)", "isof" },
                    { AllowedFunctions.IsOf, "isof(Category, Microsoft.AspNet.OData.Test.Query.Expressions.DerivedCategory)", "isof" },
                };
            }
        }

        public static TheoryDataSet<AllowedFunctions, string, string> OtherFunctions_SomeQuotedTwoParameterCasts
        {
            get
            {
                return new TheoryDataSet<AllowedFunctions, string, string>
                {
                    // Cast null to an entity type. Note 'isof' with same arguments is fine.
                    { AllowedFunctions.Cast, "cast(null,'Microsoft.AspNet.OData.Test.Query.Expressions.DerivedCategory')/DerivedCategoryName eq 'Name'", "cast" },
                    { AllowedFunctions.Cast, "cast(null, 'Microsoft.AspNet.OData.Test.Query.Expressions.DerivedCategory')/DerivedCategoryName eq 'Name'", "cast" },
                };
            }
        }

        public static TheoryDataSet<AllowedFunctions, string, string> StringFunctions
        {
            get
            {
                return new TheoryDataSet<AllowedFunctions, string, string>
                {
                    { AllowedFunctions.Concat, "concat(null,'Name') eq 'Name'", "concat" },
                    { AllowedFunctions.Concat, "concat(null, 'Name') eq 'Name'", "concat" },
                    { AllowedFunctions.Concat, "concat(ProductName,'Name') eq 'Name'", "concat" },
                    { AllowedFunctions.Concat, "concat(ProductName, 'Name') eq 'Name'", "concat" },
                    { AllowedFunctions.EndsWith, "endswith(null,'Name')", "endswith" },
                    { AllowedFunctions.EndsWith, "endswith(null, 'Name')", "endswith" },
                    { AllowedFunctions.EndsWith, "endswith(ProductName,'Name')", "endswith" },
                    { AllowedFunctions.EndsWith, "endswith(ProductName, 'Name')", "endswith" },
                    { AllowedFunctions.IndexOf, "indexof(null,'Name') eq 1", "indexof" },
                    { AllowedFunctions.IndexOf, "indexof(null, 'Name') eq 1", "indexof" },
                    { AllowedFunctions.IndexOf, "indexof(ProductName,'Name') eq 1", "indexof" },
                    { AllowedFunctions.IndexOf, "indexof(ProductName, 'Name') eq 1", "indexof" },
                    { AllowedFunctions.Length, "length(null) eq 6", "length" },
                    { AllowedFunctions.Length, "length(ProductName) eq 6", "length" },
                    { AllowedFunctions.StartsWith, "startswith(null,'Name')", "startswith" },
                    { AllowedFunctions.StartsWith, "startswith(null, 'Name')", "startswith" },
                    { AllowedFunctions.StartsWith, "startswith(ProductName,'Name')", "startswith" },
                    { AllowedFunctions.StartsWith, "startswith(ProductName, 'Name')", "startswith" },
                    { AllowedFunctions.MatchesPattern, "matchesPattern(null,'Name')", "matchesPattern" },
                    { AllowedFunctions.MatchesPattern, "matchesPattern(null, 'Name')", "matchesPattern" },
                    { AllowedFunctions.MatchesPattern, "matchesPattern(ProductName,'Name')", "matchesPattern" },
                    { AllowedFunctions.MatchesPattern, "matchesPattern(ProductName, 'Name')", "matchesPattern" },
                    { AllowedFunctions.Substring, "substring(null,1) eq 'Name'", "substring" },
                    { AllowedFunctions.Substring, "substring(null, 1) eq 'Name'", "substring" },
                    { AllowedFunctions.Substring, "substring(ProductName,1) eq 'Name'", "substring" },
                    { AllowedFunctions.Substring, "substring(ProductName, 1) eq 'Name'", "substring" },
                    { AllowedFunctions.Substring, "substring(null,1,2) eq 'Name'", "substring" },
                    { AllowedFunctions.Substring, "substring(null, 1, 2) eq 'Name'", "substring" },
                    { AllowedFunctions.Substring, "substring(ProductName,1,2) eq 'Name'", "substring" },
                    { AllowedFunctions.Substring, "substring(ProductName, 1, 2) eq 'Name'", "substring" },
                    // Contains isn't in `AllowedFunctions` with expected name.
                    { AllowedFunctions.Contains, "contains(null,'Name')", "contains" },
                    { AllowedFunctions.Contains, "contains(null, 'Name')", "contains" },
                    { AllowedFunctions.Contains, "contains(ProductName,'Name')", "contains" },
                    { AllowedFunctions.Contains, "contains(ProductName, 'Name')", "contains" },
                    { AllowedFunctions.ToLower, "tolower(null) eq 'Name'", "tolower" },
                    { AllowedFunctions.ToLower, "tolower(ProductName) eq 'Name'", "tolower" },
                    { AllowedFunctions.ToUpper, "toupper(null) eq 'Name'", "toupper" },
                    { AllowedFunctions.ToUpper, "toupper(ProductName) eq 'Name'", "toupper" },
                    { AllowedFunctions.Trim, "trim(null) eq 'Name'", "trim" },
                    { AllowedFunctions.Trim, "trim(ProductName) eq 'Name'", "trim" },
                };
            }
        }

        public static TheoryDataSet<AllowedFunctions, AllowedFunctions, string, string> Functions_CheckArguments
        {
            get
            {
                return new TheoryDataSet<AllowedFunctions, AllowedFunctions, string, string>
                {
                    { AllowedFunctions.Ceiling, AllowedFunctions.IndexOf, "ceiling(indexof(ProductName, 'Name')) eq 0", "indexof" },
                    { AllowedFunctions.Floor, AllowedFunctions.IndexOf, "floor(indexof(ProductName, 'Name')) eq 0", "indexof" },
                    { AllowedFunctions.Round, AllowedFunctions.IndexOf, "round(indexof(ProductName, 'Name')) eq 0", "indexof" },

                    { AllowedFunctions.All, AllowedFunctions.IndexOf, "AlternateAddresses/all(t : indexof(t/City, 'Name') eq 3)", "indexof" },
                    { AllowedFunctions.Any, AllowedFunctions.IndexOf, "AlternateAddresses/any(t : indexof(t/City, 'Name') eq 3)", "indexof" },
                    { AllowedFunctions.Cast, AllowedFunctions.IndexOf, "cast(indexof(ProductName, 'Name'), 'Edm.Int64') eq 0", "indexof" },
                    { AllowedFunctions.IsOf, AllowedFunctions.IndexOf, "isof(indexof(ProductName, 'Name'), 'Edm.Int64')", "indexof" },

                    { AllowedFunctions.Concat, AllowedFunctions.Substring, "concat(substring(ProductName, 1), 'Name') eq 'Name'", "substring" },
                    { AllowedFunctions.Concat, AllowedFunctions.Substring, "concat(ProductName, substring(ProductName, 1)) eq 'Name'", "substring" },
                    { AllowedFunctions.EndsWith, AllowedFunctions.Substring, "endswith(substring(ProductName, 1), 'Name')", "substring" },
                    { AllowedFunctions.EndsWith, AllowedFunctions.Substring, "endswith(ProductName, substring(ProductName, 1))", "substring" },
                    { AllowedFunctions.IndexOf, AllowedFunctions.Substring, "indexof(substring(ProductName, 1), 'Name') eq 1", "substring" },
                    { AllowedFunctions.IndexOf, AllowedFunctions.Substring, "indexof(ProductName, substring(ProductName, 1)) eq 1", "substring" },
                    { AllowedFunctions.Length, AllowedFunctions.Substring, "length(substring(ProductName, 1)) eq 6", "substring" },
                    { AllowedFunctions.StartsWith, AllowedFunctions.Substring, "startswith(substring(ProductName, 1), 'Name')", "substring" },
                    { AllowedFunctions.StartsWith, AllowedFunctions.Substring, "startswith(ProductName, substring(ProductName, 1))", "substring" },
                    { AllowedFunctions.Substring, AllowedFunctions.Concat, "substring(concat(ProductName, 'Name'), 1) eq 'Name'", "concat" },
                    { AllowedFunctions.Substring, AllowedFunctions.IndexOf, "substring(ProductName, indexof(ProductName, 'Name')) eq 'Name'", "indexof" },
                    { AllowedFunctions.Substring, AllowedFunctions.Concat, "substring(concat(ProductName, 'Name'), 1, 2) eq 'Name'", "concat" },
                    { AllowedFunctions.Substring, AllowedFunctions.IndexOf, "substring(ProductName, indexof(ProductName, 'Name'), 2) eq 'Name'", "indexof" },
                    { AllowedFunctions.Substring, AllowedFunctions.IndexOf, "substring(ProductName, 1, indexof(ProductName, 'Name')) eq 'Name'", "indexof" },
                    { AllowedFunctions.Contains, AllowedFunctions.Substring, "contains(substring(ProductName, 1), 'Name')", "substring" },
                    { AllowedFunctions.Contains, AllowedFunctions.Substring, "contains(ProductName, substring(ProductName, 1))", "substring" },
                    { AllowedFunctions.ToLower, AllowedFunctions.Substring, "tolower(substring(ProductName, 1)) eq 'Name'", "substring" },
                    { AllowedFunctions.ToUpper, AllowedFunctions.Substring, "toupper(substring(ProductName, 1)) eq 'Name'", "substring" },
                    { AllowedFunctions.Trim, AllowedFunctions.Substring, "trim(substring(ProductName, 1)) eq 'Name'", "substring" },
                };
            }
        }

        public static TheoryDataSet<string, string> Functions_CheckNotFilterable
        {
            get
            {
                return new TheoryDataSet<string, string>
                {
                    { "day(NotFilterableDiscontinuedDate) eq 20", "NotFilterableDiscontinuedDate" },
                    { "hour(NotFilterableDiscontinuedDate) eq 10", "NotFilterableDiscontinuedDate" },
                    { "minute(NotFilterableDiscontinuedDate) eq 20", "NotFilterableDiscontinuedDate" },
                    { "month(NotFilterableDiscontinuedDate) eq 10", "NotFilterableDiscontinuedDate" },
                    { "second(NotFilterableDiscontinuedDate) eq 20", "NotFilterableDiscontinuedDate" },
                    { "year(NotFilterableDiscontinuedDate) eq 2000", "NotFilterableDiscontinuedDate" },

                    { "date(NotFilterableDiscontinuedDate) eq 2015-02-26", "NotFilterableDiscontinuedDate" },
                    { "time(NotFilterableDiscontinuedDate) eq 03:4:05.90100", "NotFilterableDiscontinuedDate" },
                    { "fractionalseconds(NotFilterableDiscontinuedDate) eq 0.1", "NotFilterableDiscontinuedDate" },

                    { "NotFilterableAlternateAddresses/all(t : t/City eq 'Redmond')", "NotFilterableAlternateAddresses" },
                    { "NotFilterableAlternateAddresses/any(t : t/City eq 'Redmond')", "NotFilterableAlternateAddresses" },
                };
            }
        }

        public static TheoryDataSet<AllowedLogicalOperators, string, string> LogicalOperators
        {
            get
            {
                return new TheoryDataSet<AllowedLogicalOperators, string, string>
                {
                    { AllowedLogicalOperators.And, "Discontinued and AlternateIDs/any()", "And" },
                    { AllowedLogicalOperators.Equal, "UnitPrice add 0 eq UnitPrice", "Equal" },
                    { AllowedLogicalOperators.GreaterThan, "UnitPrice add 1 gt UnitPrice", "GreaterThan" },
                    { AllowedLogicalOperators.GreaterThanOrEqual, "UnitPrice add 0 ge UnitPrice", "GreaterThanOrEqual" },
                    { AllowedLogicalOperators.Has, "Ranking has Microsoft.AspNet.OData.Test.Common.Types.SimpleEnum'First'", "Has" },
                    { AllowedLogicalOperators.LessThan, "UnitPrice add -1 lt UnitPrice", "LessThan" },
                    { AllowedLogicalOperators.LessThanOrEqual, "UnitPrice add 0 le UnitPrice", "LessThanOrEqual" },
                    { AllowedLogicalOperators.Not, "not Discontinued", "Not" },
                    { AllowedLogicalOperators.NotEqual, "UnitPrice add 1 ne UnitPrice", "NotEqual" },
                    { AllowedLogicalOperators.Or, "Discontinued or AlternateIDs/any()", "Or" },
                };
            }
        }

        public static TheoryDataSet<string> LogicalOperators_CheckArguments
        {
            get
            {
                return new TheoryDataSet<string>
                {
                    { "(UnitPrice add 0 eq UnitPrice) and AlternateIDs/any()" },
                    { "UnitPrice add 0 eq UnitPrice" },
                    { "UnitPrice add 1 gt UnitPrice" },
                    { "UnitPrice add 0 ge UnitPrice" },
                    { "UnitPrice add -1 lt UnitPrice" },
                    { "UnitPrice add 0 le UnitPrice" },
                    { "not (UnitPrice add 0 eq UnitPrice)" },
                    { "UnitPrice add 1 ne UnitPrice" },
                    { "(UnitPrice add 0 eq UnitPrice) or AlternateIDs/any()" },

                    { "Discontinued and (UnitPrice add 0 eq UnitPrice)" },
                    { "UnitPrice eq UnitPrice add 0" },
                    { "UnitPrice gt UnitPrice add -1" },
                    { "UnitPrice ge UnitPrice add 0" },
                    { "UnitPrice lt UnitPrice add 1" },
                    { "UnitPrice le UnitPrice add 0" },
                    { "UnitPrice ne UnitPrice add 1" },
                    { "Discontinued or (UnitPrice add 0 eq UnitPrice)" },
                };
            }
        }

        public FilterQueryValidatorTest()
        {
            _context = ValidationTestHelper.CreateCustomerContext();
            _productContext = ValidationTestHelper.CreateDerivedProductsContext();
            _validator = new MyFilterValidator(_productContext.DefaultQuerySettings);
        }

        [Fact]
        public void ValidateThrowsOnNullOption()
        {
            ExceptionAssert.Throws<ArgumentNullException>(() =>
                _validator.Validate(null, new ODataValidationSettings()));
        }

        [Fact]
        public void ValidateThrowsOnNullSettings()
        {
            ExceptionAssert.Throws<ArgumentNullException>(() =>
                _validator.Validate(new FilterQueryOption("Name eq 'abc'", _context), null));
        }

        // want to test if all the virtual methods are being invoked correctly
        [Fact]
        public void ValidateVisitAll()
        {
            // Arrange
            FilterQueryOption option = new FilterQueryOption("Tags/all(t: t eq '42')", _context);

            // Act
            _validator.Validate(option, _settings);

            // Assert
            Assert.Equal(7, _validator.Times.Keys.Count);
            Assert.Equal(1, _validator.Times["Validate"]); // entry
            Assert.Equal(1, _validator.Times["ValidateAllQueryNode"]); // all
            Assert.Equal(1, _validator.Times["ValidateLogicalOperator"]); // eq
            Assert.Equal(1, _validator.Times["ValidateCollectionPropertyAccessNode"]); // Tags
            Assert.Equal(1, _validator.Times["ValidateConstantQueryNode"]); // 42
            Assert.Equal(1, _validator.Times["ValidateBinaryOperatorQueryNode"]); // eq
            Assert.Equal(2, _validator.Times["ValidateParameterQueryNode"]); // $it, t
        }

        [Fact]
        public void ValidateVisitAny()
        {
            // Arrange
            FilterQueryOption option = new FilterQueryOption("Tags/any(t: t eq '42')", _context);

            // Act
            _validator.Validate(option, _settings);

            // Assert
            Assert.Equal(7, _validator.Times.Keys.Count);
            Assert.Equal(1, _validator.Times["Validate"]); // entry
            Assert.Equal(1, _validator.Times["ValidateAnyQueryNode"]); // all
            Assert.Equal(1, _validator.Times["ValidateLogicalOperator"]); // eq
            Assert.Equal(1, _validator.Times["ValidateCollectionPropertyAccessNode"]); // Tags
            Assert.Equal(1, _validator.Times["ValidateConstantQueryNode"]); // 42
            Assert.Equal(1, _validator.Times["ValidateBinaryOperatorQueryNode"]); // eq
            Assert.Equal(2, _validator.Times["ValidateParameterQueryNode"]); // $it, t
        }

        [Theory]
        [InlineData("NotFilterableProperty")]
        [InlineData("NonFilterableProperty")]
        public void ValidateThrowsIfNotFilterableProperty(string property)
        {
            ExceptionAssert.Throws<ODataException>(() =>
                _validator.Validate(
                    new FilterQueryOption(String.Format("{0} eq 'David'", property), _context),
                    new ODataValidationSettings()),
                String.Format("The property '{0}' cannot be used in the $filter query option.", property));
        }

        [Theory]
        [InlineData("NotFilterableNavigationProperty")]
        [InlineData("NonFilterableNavigationProperty")]
        public void ValidateThrowsIfNotFilterableNavigationProperty(string property)
        {
            ExceptionAssert.Throws<ODataException>(() =>
                _validator.Validate(
                    new FilterQueryOption(String.Format("{0}/Name eq 'Seattle'", property), _context),
                    new ODataValidationSettings()),
                String.Format("The property '{0}' cannot be used in the $filter query option.", property));
        }

        [Theory]
        [InlineData("NotFilterableProperty")]
        [InlineData("NonFilterableProperty")]
        public void ValidateThrowsIfNavigationHasNotFilterableProperty(string property)
        {
            ExceptionAssert.Throws<ODataException>(() =>
                _validator.Validate(
                    new FilterQueryOption(String.Format("NavigationWithNotFilterableProperty/{0} eq 'David'", property), _context),
                    new ODataValidationSettings()),
                String.Format("The property '{0}' cannot be used in the $filter query option.", property));
        }

        [Theory]
        [MemberData(nameof(NestedAnyAllInputs))]
        public void MaxAnyAllExpressionDepthLimitExceeded(string filter)
        {
            // Arrange
            ODataValidationSettings settings = new ODataValidationSettings();
            settings.MaxAnyAllExpressionDepth = 1;

            // Act & Assert
            ExceptionAssert.Throws<ODataException>(() => _validator.Validate(new FilterQueryOption(filter, _productContext), settings), "The Any/All nesting limit of '1' has been exceeded. 'MaxAnyAllExpressionDepth' can be configured on ODataQuerySettings or EnableQueryAttribute.");
        }

        [Theory]
        [MemberData(nameof(NestedAnyAllInputs))]
        public void IncreaseMaxAnyAllExpressionDepthWillAllowNestedAnyAllInputs(string filter)
        {
            // Arrange
            ODataValidationSettings settings = new ODataValidationSettings();
            settings.MaxAnyAllExpressionDepth = 2;

            // Act & Assert
            ExceptionAssert.DoesNotThrow(() => _validator.Validate(new FilterQueryOption(filter, _productContext), settings));
        }

        [Theory]
        [MemberData(nameof(LongInputs))]
        public void LongInputs_CauseMaxNodeCountExceededException(string filter)
        {
            // Arrange
            ODataValidationSettings settings = new ODataValidationSettings
            {
                MaxAnyAllExpressionDepth = Int32.MaxValue
            };

            FilterQueryOption option = new FilterQueryOption(filter, _productContext);

            // Act & Assert
            ExceptionAssert.Throws<ODataException>(() => _validator.Validate(option, settings), "The node count limit of '100' has been exceeded. To increase the limit, set the 'MaxNodeCount' property on EnableQueryAttribute or ODataValidationSettings.");
        }

        [Theory]
        [MemberData(nameof(LongInputs))]
        public void IncreaseMaxNodeCountWillAllowLongInputs(string filter)
        {
            // Arrange
            ODataValidationSettings settings = new ODataValidationSettings
            {
                MaxAnyAllExpressionDepth = Int32.MaxValue,
                MaxNodeCount = 105,
            };

            FilterQueryOption option = new FilterQueryOption(filter, _productContext);

            // Act & Assert
            ExceptionAssert.DoesNotThrow(() => _validator.Validate(option, settings));
        }

        [Theory]
        [MemberData(nameof(CloseToLongInputs))]
        public void AlmostLongInputs_DonotCauseMaxNodeCountExceededExceptionOrTimeoutDuringCompilation(string filter)
        {
            // Arrange
            ODataValidationSettings settings = new ODataValidationSettings
            {
                MaxAnyAllExpressionDepth = Int32.MaxValue
            };

            FilterQueryOption option = new FilterQueryOption(filter, _productContext);

            // Act & Assert
            ExceptionAssert.DoesNotThrow(() => _validator.Validate(option, settings));
            ExceptionAssert.DoesNotThrow(() => option.ApplyTo(new List<Product>().AsQueryable(), new ODataQuerySettings()));
        }

        [Fact]
        public void ArithmeticOperatorsDataSet_CoversAllValues()
        {
            // Arrange
            // Get all values in the AllowedArithmeticOperators enum.
            var values = new HashSet<AllowedArithmeticOperators>(
                Enum.GetValues(typeof(AllowedArithmeticOperators)).Cast<AllowedArithmeticOperators>());
            var groupValues = new[]
            {
                AllowedArithmeticOperators.All,
                AllowedArithmeticOperators.None,
            };

            // Act
            // Remove the group items.
            foreach (var allowed in groupValues)
            {
                values.Remove(allowed);
            }

            // Remove the individual items.
            foreach (var allowed in ArithmeticOperators.Select(item => (AllowedArithmeticOperators)(item[0])))
            {
                values.Remove(allowed);
            }

            // Assert
            // Should have nothing left.
            Assert.Empty(values);
        }

        [Theory]
        [MemberData(nameof(ArithmeticOperators))]
        public void AllowedArithmeticOperators_SucceedIfAllowed(AllowedArithmeticOperators allow, string query, string unused)
        {
            // Arrange
            var settings = new ODataValidationSettings
            {
                AllowedArithmeticOperators = allow,
            };
            var option = new FilterQueryOption(query, _productContext);

            // Act & Assert
            Assert.NotNull(unused);
            ExceptionAssert.DoesNotThrow(() => _validator.Validate(option, settings));
        }

        [Theory]
        [MemberData(nameof(ArithmeticOperators))]
        public void AllowedArithmeticOperators_ThrowIfNotAllowed(AllowedArithmeticOperators exclude, string query, string operatorName)
        {
            // Arrange
            var settings = new ODataValidationSettings
            {
                AllowedArithmeticOperators = AllowedArithmeticOperators.All & ~exclude,
            };
            var expectedMessage = string.Format(
                "Arithmetic operator '{0}' is not allowed. " +
                "To allow it, set the 'AllowedArithmeticOperators' property on EnableQueryAttribute or QueryValidationSettings.",
                operatorName);
            var option = new FilterQueryOption(query, _productContext);

            // Act & Assert
            ExceptionAssert.Throws<ODataException>(() => _validator.Validate(option, settings), expectedMessage);
        }

        [Theory]
        [MemberData(nameof(ArithmeticOperators))]
        public void AllowedArithmeticOperators_ThrowIfNoneAllowed(AllowedArithmeticOperators unused, string query, string operatorName)
        {
            // Arrange
            var settings = new ODataValidationSettings
            {
                AllowedArithmeticOperators = AllowedArithmeticOperators.None,
            };
            var expectedMessage = string.Format(
                "Arithmetic operator '{0}' is not allowed. " +
                "To allow it, set the 'AllowedArithmeticOperators' property on EnableQueryAttribute or QueryValidationSettings.",
                operatorName);
            var option = new FilterQueryOption(query, _productContext);

            // Act & Assert
            Assert.NotEqual(unused, settings.AllowedArithmeticOperators);
            ExceptionAssert.Throws<ODataException>(() => _validator.Validate(option, settings), expectedMessage);
        }

        [Theory]
        [MemberData(nameof(ArithmeticOperators_CheckArguments))]
        public void ArithmeticOperators_CheckArguments_SucceedIfAllowed(string query)
        {
            // Arrange
            var settings = new ODataValidationSettings
            {
                AllowedFunctions = AllowedFunctions.Day,
            };
            var option = new FilterQueryOption(query, _productContext);

            // Act & Assert
            ExceptionAssert.DoesNotThrow(() => _validator.Validate(option, settings));
        }

        [Theory]
        [MemberData(nameof(ArithmeticOperators_CheckArguments))]
        public void ArithmeticOperators_CheckArguments_ThrowIfNotAllowed(string query)
        {
            // Arrange
            var settings = new ODataValidationSettings
            {
                AllowedFunctions = AllowedFunctions.AllFunctions & ~AllowedFunctions.Day,
            };
            var expectedMessage = string.Format(
                "Function 'day' is not allowed. " +
                "To allow it, set the 'AllowedFunctions' property on EnableQueryAttribute or QueryValidationSettings.");
            var option = new FilterQueryOption(query, _productContext);

            // Act & Assert
            ExceptionAssert.Throws<ODataException>(() => _validator.Validate(option, settings), expectedMessage);
        }

        [Fact]
        public void AllowedFunctionDataSets_CoverAllValues()
        {
            // Arrange
            // Get all values in the AllowedFunctions enum.
            var values = new HashSet<AllowedFunctions>(Enum.GetValues(typeof(AllowedFunctions)).Cast<AllowedFunctions>());

            var groupValues = new[]
            {
                AllowedFunctions.None,
                AllowedFunctions.AllFunctions,
                AllowedFunctions.AllDateTimeFunctions,
                AllowedFunctions.AllMathFunctions,
                AllowedFunctions.AllStringFunctions
            };

            // No need to include OtherFunctions_* here since they cover enum values also in OtherFunctions.
            var dataSets = DateTimeFunctions
                .Concat(MathFunctions)
                .Concat(OtherFunctions)
                .Concat(StringFunctions);

            // Act
            // Remove the group items.
            foreach (var allowed in groupValues)
            {
                values.Remove(allowed);
            }

            // Remove the individual items.
            foreach (var allowed in dataSets.Select(item => (AllowedFunctions)(item[0])))
            {
                values.Remove(allowed);
            }

            // Assert
            // Should have nothing left.
            Assert.Empty(values);
        }

        [Theory]
        [MemberData(nameof(DateTimeFunctions))]
        [MemberData(nameof(MathFunctions))]
        [MemberData(nameof(OtherFunctions))]
        [MemberData(nameof(StringFunctions))]
        public void AllowedFunctions_SucceedIfAllowed(AllowedFunctions allow, string query, string unused)
        {
            // Arrange
            var settings = new ODataValidationSettings
            {
                AllowedFunctions = allow,
            };
            var option = new FilterQueryOption(query, _productContext);

            // Act & Assert
            Assert.NotNull(unused);
            ExceptionAssert.DoesNotThrow(() => _validator.Validate(option, settings));
        }

        [Theory]
        [MemberData(nameof(DateTimeFunctions))]
        [MemberData(nameof(MathFunctions))]
        [MemberData(nameof(OtherFunctions))]
        [MemberData(nameof(StringFunctions))]
        public void AllowedFunctions_ThrowIfNotAllowed(AllowedFunctions exclude, string query, string functionName)
        {
            // Arrange
            var settings = new ODataValidationSettings
            {
                AllowedFunctions = AllowedFunctions.AllFunctions & ~exclude,
            };
            var expectedMessage = string.Format(
                "Function '{0}' is not allowed. " +
                "To allow it, set the 'AllowedFunctions' property on EnableQueryAttribute or QueryValidationSettings.",
                functionName);
            var option = new FilterQueryOption(query, _productContext);

            // Act & Assert
            ExceptionAssert.Throws<ODataException>(() => _validator.Validate(option, settings), expectedMessage);
        }

        [Theory]
        [MemberData(nameof(DateTimeFunctions))]
        [MemberData(nameof(MathFunctions))]
        [MemberData(nameof(OtherFunctions))]
        [MemberData(nameof(StringFunctions))]
        public void AllowedFunctions_ThrowIfNoneAllowed(AllowedFunctions unused, string query, string functionName)
        {
            // Arrange
            var settings = new ODataValidationSettings
            {
                AllowedFunctions = AllowedFunctions.None,
            };
            var expectedMessage = string.Format(
                "Function '{0}' is not allowed. " +
                "To allow it, set the 'AllowedFunctions' property on EnableQueryAttribute or QueryValidationSettings.",
                functionName);
            var option = new FilterQueryOption(query, _productContext);

            // Act & Assert
            Assert.NotEqual(AllowedFunctions.None, unused);
            ExceptionAssert.Throws<ODataException>(() => _validator.Validate(option, settings), expectedMessage);
        }

        [Theory]
        [MemberData(nameof(DateTimeFunctions))]
        public void DateTimeFunctions_SucceedIfGroupAllowed(AllowedFunctions unused, string query, string unusedName)
        {
            // Arrange
            var settings = new ODataValidationSettings
            {
                AllowedFunctions = AllowedFunctions.AllDateTimeFunctions,
            };
            var option = new FilterQueryOption(query, _productContext);

            // Act & Assert
            Assert.NotEqual(AllowedFunctions.None, unused);
            Assert.NotNull(unusedName);
            ExceptionAssert.DoesNotThrow(() => _validator.Validate(option, settings));
        }

        [Theory]
        [MemberData(nameof(DateTimeFunctions))]
        public void DateTimeFunctions_ThrowIfGroupNotAllowed(AllowedFunctions unused, string query, string functionName)
        {
            // Arrange
            var settings = new ODataValidationSettings
            {
                AllowedFunctions = AllowedFunctions.AllFunctions & ~AllowedFunctions.AllDateTimeFunctions,
            };
            var expectedMessage = string.Format(
                "Function '{0}' is not allowed. " +
                "To allow it, set the 'AllowedFunctions' property on EnableQueryAttribute or QueryValidationSettings.",
                functionName);
            var option = new FilterQueryOption(query, _productContext);

            // Act & Assert
            Assert.NotEqual(AllowedFunctions.None, unused);
            ExceptionAssert.Throws<ODataException>(() => _validator.Validate(option, settings), expectedMessage);
        }

        [Theory]
        [MemberData(nameof(MathFunctions))]
        public void MathFunctions_SucceedIfGroupAllowed(AllowedFunctions unused, string query, string unusedName)
        {
            // Arrange
            var settings = new ODataValidationSettings
            {
                AllowedFunctions = AllowedFunctions.AllMathFunctions,
            };
            var option = new FilterQueryOption(query, _productContext);

            // Act & Assert
            Assert.NotEqual(AllowedFunctions.None, unused);
            Assert.NotNull(unusedName);
            ExceptionAssert.DoesNotThrow(() => _validator.Validate(option, settings));
        }

        [Theory]
        [MemberData(nameof(MathFunctions))]
        public void MathFunctions_ThrowIfGroupNotAllowed(AllowedFunctions unused, string query, string functionName)
        {
            // Arrange
            var settings = new ODataValidationSettings
            {
                AllowedFunctions = AllowedFunctions.AllFunctions & ~AllowedFunctions.AllMathFunctions,
            };
            var expectedMessage = string.Format(
                "Function '{0}' is not allowed. " +
                "To allow it, set the 'AllowedFunctions' property on EnableQueryAttribute or QueryValidationSettings.",
                functionName);
            var option = new FilterQueryOption(query, _productContext);

            // Act & Assert
            Assert.NotEqual(AllowedFunctions.None, unused);
            ExceptionAssert.Throws<ODataException>(() => _validator.Validate(option, settings), expectedMessage);
        }

        [Theory]
        [MemberData(nameof(StringFunctions))]
        public void StringFunctions_SucceedIfGroupAllowed(AllowedFunctions unused, string query, string unusedName)
        {
            // Arrange
            var settings = new ODataValidationSettings
            {
                AllowedFunctions = AllowedFunctions.AllStringFunctions,
            };
            var option = new FilterQueryOption(query, _productContext);

            // Act & Assert
            Assert.NotEqual(AllowedFunctions.None, unused);
            Assert.NotNull(unusedName);
            ExceptionAssert.DoesNotThrow(() => _validator.Validate(option, settings));
        }

        [Theory]
        [MemberData(nameof(StringFunctions))]
        public void StringFunctions_ThrowIfGroupNotAllowed(AllowedFunctions unused, string query, string functionName)
        {
            // Arrange
            var settings = new ODataValidationSettings
            {
                AllowedFunctions = AllowedFunctions.AllFunctions & ~AllowedFunctions.AllStringFunctions,
            };
            var expectedMessage = string.Format(
                "Function '{0}' is not allowed. " +
                "To allow it, set the 'AllowedFunctions' property on EnableQueryAttribute or QueryValidationSettings.",
                functionName);
            var option = new FilterQueryOption(query, _productContext);

            // Act & Assert
            Assert.NotEqual(AllowedFunctions.None, unused);
            ExceptionAssert.Throws<ODataException>(() => _validator.Validate(option, settings), expectedMessage);
        }

        [Theory]
        [MemberData(nameof(OtherFunctions_SomeSingleParameterCasts))]
        public void OtherFunctions_SomeSingleParameterCasts_ThrowODataException(AllowedFunctions unused, string query, string unusedName)
        {
            // Thrown at
            // Microsoft.OData.Core.dll!Microsoft.OData.Core.UriParser.Parsers.FunctionCallBinder.ValidateIsOfOrCast(BindingState state, ..., ref List<QueryNode> args) Line 600
            // Microsoft.OData.Core.dll!Microsoft.OData.Core.UriParser.Parsers.FunctionCallBinder.ValidateAndBuildCastArgs(BindingState state, ref List<QueryNode> args) Line 557
            // Microsoft.OData.Core.dll!Microsoft.OData.Core.UriParser.Parsers.FunctionCallBinder.CreateUnboundFunctionNode(FunctionCallToken functionCallToken, ..., BindingState state) Line 525
            // Microsoft.OData.Core.dll!Microsoft.OData.Core.UriParser.Parsers.FunctionCallBinder.BindAsBuiltInFunction(FunctionCallToken functionCallToken, ..., List<QueryNode> argumentNodes) Line 265
            // Microsoft.OData.Core.dll!Microsoft.OData.Core.UriParser.Parsers.FunctionCallBinder.BindFunctionCall(FunctionCallToken functionCallToken, BindingState state) Line 202
            // Microsoft.OData.Core.dll!Microsoft.OData.Core.UriParser.Parsers.MetadataBinder.BindFunctionCall(FunctionCallToken functionCallToken) Line 323
            // Microsoft.OData.Core.dll!Microsoft.OData.Core.UriParser.Parsers.MetadataBinder.Bind(QueryToken token) Line 172
            // Microsoft.OData.Core.dll!Microsoft.OData.Core.UriParser.Parsers.EndPathBinder.DetermineParentNode(EndPathToken segmentToken, BindingState state) Line 188
            // Microsoft.OData.Core.dll!Microsoft.OData.Core.UriParser.Parsers.EndPathBinder.BindEndPath(EndPathToken endPathToken, BindingState state) Line 138
            // Microsoft.OData.Core.dll!Microsoft.OData.Core.UriParser.Parsers.MetadataBinder.BindEndPath(EndPathToken endPathToken) Line 312
            // Microsoft.OData.Core.dll!Microsoft.OData.Core.UriParser.Parsers.MetadataBinder.Bind(QueryToken token) Line 169
            // Microsoft.OData.Core.dll!Microsoft.OData.Core.UriParser.Parsers.BinaryOperatorBinder.GetOperandFromToken(BinaryOperatorKind operatorKind, QueryToken queryToken) Line 83
            // Microsoft.OData.Core.dll!Microsoft.OData.Core.UriParser.Parsers.BinaryOperatorBinder.BindBinaryOperator(BinaryOperatorToken binaryOperatorToken) Line 46
            // Microsoft.OData.Core.dll!Microsoft.OData.Core.UriParser.Parsers.MetadataBinder.BindBinaryOperator(BinaryOperatorToken binaryOperatorToken) Line 266
            // Microsoft.OData.Core.dll!Microsoft.OData.Core.UriParser.Parsers.MetadataBinder.Bind(QueryToken token) Line 163
            // Microsoft.OData.Core.dll!Microsoft.OData.Core.UriParser.Parsers.FilterBinder.BindFilter(QueryToken filter) Line 51
            // Microsoft.OData.Core.dll!Microsoft.OData.Core.UriParser.ODataQueryOptionParser.ParseFilterImplementation(string filter, ..., IEdmNavigationSource navigationSource) Line 250
            // Microsoft.OData.Core.dll!Microsoft.OData.Core.UriParser.ODataQueryOptionParser.ParseFilter() Line 112
            // Microsoft.AspNet.OData.Test.dll!Microsoft.AspNet.OData.Test.Query.FilterQueryOption.FilterClause.get() Line 99
            // Microsoft.AspNet.OData.Test.dll!Microsoft.AspNet.OData.Test.Query.Validators.FilterQueryValidator.Validate(FilterQueryOption filterQueryOption, ODataValidationSettings settings) Line 54

            // Arrange
            var settings = new ODataValidationSettings
            {
                AllowedFunctions = AllowedFunctions.None,
            };
            var expectedMessage = "Cast or IsOf Function must have a type in its arguments.";
            var option = new FilterQueryOption(query, _productContext);

            // Act & Assert
            Assert.NotEqual(AllowedFunctions.None, unused);
            Assert.NotNull(unusedName);
            ExceptionAssert.Throws<ODataException>(() => _validator.Validate(option, settings), expectedMessage);
        }

        [Theory]
        [MemberData(nameof(OtherFunctions_SomeTwoParameterCasts))]
        public void OtherFunctions_SomeTwoParameterCasts_ThrowODataException(AllowedFunctions unused, string query, string unusedName)
        {
            // Thrown at
            // Microsoft.OData.Core.dll!Microsoft.OData.Core.UriParser.Metadata.UriEdmHelpers.CheckRelatedTo(IEdmType parentType, IEdmType childType) Line 108
            // Microsoft.OData.Core.dll!Microsoft.OData.Core.UriParser.Parsers.DottedIdentifierBinder.BindDottedIdentifier(DottedIdentifierToken dottedIdentifierToken, BindingState state) Line 107
            // Microsoft.OData.Core.dll!Microsoft.OData.Core.UriParser.Parsers.MetadataBinder.BindCast(DottedIdentifierToken dottedIdentifierToken) Line 288
            // Microsoft.OData.Core.dll!Microsoft.OData.Core.UriParser.Parsers.MetadataBinder.Bind(QueryToken token) Line 175
            // Microsoft.OData.Core.dll!Microsoft.OData.Core.UriParser.Parsers.MetadataBinder.BindFunctionParameter(FunctionParameterToken token) Line 223
            // Microsoft.OData.Core.dll!Microsoft.OData.Core.UriParser.Parsers.MetadataBinder.Bind(QueryToken token) Line 184
            // Microsoft.OData.Core.dll!Microsoft.OData.Core.UriParser.Parsers.FunctionCallBinder.BindFunctionCall.AnonymousMethod__8(FunctionParameterToken ar) Line 201
            // System.Core.dll!System.Linq.Enumerable.WhereSelectEnumerableIterator<FunctionParameterToken,QueryNode>.MoveNext() Line 285
            // mscorlib.dll!System.Collections.Generic.List<QueryNode>.List(System.Collections.Generic.IEnumerable<QueryNode> collection) Line 105
            // Microsoft.OData.Core.dll!Microsoft.OData.Core.UriParser.Parsers.FunctionCallBinder.BindFunctionCall(FunctionCallToken functionCallToken, BindingState state) Line 201
            // Microsoft.OData.Core.dll!Microsoft.OData.Core.UriParser.Parsers.MetadataBinder.BindFunctionCall(FunctionCallToken functionCallToken) Line 323
            // Microsoft.OData.Core.dll!Microsoft.OData.Core.UriParser.Parsers.MetadataBinder.Bind(QueryToken token) Line 172
            // Microsoft.OData.Core.dll!Microsoft.OData.Core.UriParser.Parsers.EndPathBinder.DetermineParentNode(EndPathToken segmentToken, BindingState state) Line 188
            // Microsoft.OData.Core.dll!Microsoft.OData.Core.UriParser.Parsers.EndPathBinder.BindEndPath(EndPathToken endPathToken, BindingState state) Line 138
            // Microsoft.OData.Core.dll!Microsoft.OData.Core.UriParser.Parsers.MetadataBinder.BindEndPath(EndPathToken endPathToken) Line 312
            // Microsoft.OData.Core.dll!Microsoft.OData.Core.UriParser.Parsers.MetadataBinder.Bind(QueryToken token) Line 169
            // Microsoft.OData.Core.dll!Microsoft.OData.Core.UriParser.Parsers.BinaryOperatorBinder.GetOperandFromToken(BinaryOperatorKind operatorKind, QueryToken queryToken) Line 83
            // Microsoft.OData.Core.dll!Microsoft.OData.Core.UriParser.Parsers.BinaryOperatorBinder.BindBinaryOperator(BinaryOperatorToken binaryOperatorToken) Line 46
            // Microsoft.OData.Core.dll!Microsoft.OData.Core.UriParser.Parsers.MetadataBinder.BindBinaryOperator(BinaryOperatorToken binaryOperatorToken) Line 266
            // Microsoft.OData.Core.dll!Microsoft.OData.Core.UriParser.Parsers.MetadataBinder.Bind(QueryToken token) Line 163
            // Microsoft.OData.Core.dll!Microsoft.OData.Core.UriParser.Parsers.FilterBinder.BindFilter(QueryToken filter) Line 51
            // Microsoft.OData.Core.dll!Microsoft.OData.Core.UriParser.ODataQueryOptionParser.ParseFilterImplementation(string filter, ..., IEdmNavigationSource navigationSource) Line 250
            // Microsoft.OData.Core.dll!Microsoft.OData.Core.UriParser.ODataQueryOptionParser.ParseFilter() Line 112
            // Microsoft.AspNet.OData.Test.dll!Microsoft.AspNet.OData.Test.Query.FilterQueryOption.FilterClause.get() Line 99
            // Microsoft.AspNet.OData.Test.dll!Microsoft.AspNet.OData.Test.Query.Validators.FilterQueryValidator.Validate(FilterQueryOption filterQueryOption, ODataValidationSettings settings) Line 54

            // Arrange
            var settings = new ODataValidationSettings
            {
                AllowedFunctions = AllowedFunctions.None,
            };
            var expectedMessage = string.Format(
                "Encountered invalid type cast. '{0}' is not assignable from '{1}'.",
                typeof(DerivedCategory).FullName,
                typeof(Product).FullName);
            var option = new FilterQueryOption(query, _productContext);

            // Act & Assert
            Assert.NotEqual(AllowedFunctions.None, unused);
            Assert.NotNull(unusedName);
            ExceptionAssert.Throws<ODataException>(() => _validator.Validate(option, settings), expectedMessage);
        }

        [Theory]
        [MemberData(nameof(OtherFunctions_SomeQuotedTwoParameterCasts))]
        public void OtherFunctions_SomeQuotedTwoParameterCasts_DoesnotThrowArgumentException(AllowedFunctions unused, string query, string unusedName)
        {
            // Arrange
            var settings = new ODataValidationSettings
            {
                AllowedFunctions = AllowedFunctions.AllFunctions,
            };
            var option = new FilterQueryOption(query, _productContext);

            // Act & Assert
            Assert.NotEqual(AllowedFunctions.None, unused);
            Assert.NotNull(unusedName);
            ExceptionAssert.DoesNotThrow(() => _validator.Validate(option, settings));
        }

        [Theory]
        [MemberData(nameof(Functions_CheckArguments))]
        public void Functions_CheckArguments_SucceedIfAllowed(AllowedFunctions outer, AllowedFunctions inner, string query, string unused)
        {
            // Arrange
            var settings = new ODataValidationSettings
            {
                AllowedFunctions = outer | inner,
            };
            var option = new FilterQueryOption(query, _productContext);

            // Act & Assert
            Assert.NotNull(unused);
            ExceptionAssert.DoesNotThrow(() => _validator.Validate(option, settings));
        }

        [Theory]
        [MemberData(nameof(Functions_CheckArguments))]
        public void Functions_CheckArguments_ThrowIfNotAllowed(AllowedFunctions outer, AllowedFunctions inner, string query, string functionName)
        {
            // Arrange
            var settings = new ODataValidationSettings
            {
                AllowedFunctions = outer,
            };
            var expectedMessage = string.Format(
                "Function '{0}' is not allowed. " +
                "To allow it, set the 'AllowedFunctions' property on EnableQueryAttribute or QueryValidationSettings.",
                functionName);
            var option = new FilterQueryOption(query, _productContext);

            // Act & Assert
            Assert.NotEqual(AllowedFunctions.None, inner);
            ExceptionAssert.Throws<ODataException>(() => _validator.Validate(option, settings), expectedMessage);
        }

        [Theory]
        [MemberData(nameof(Functions_CheckNotFilterable))]
        public void Functions_CheckNotFilterable_ThrowODataException(string query, string propertyName)
        {
            // Arrange
            var settings = new ODataValidationSettings
            {
                AllowedFunctions = AllowedFunctions.AllFunctions,
            };
            var expectedMessage = string.Format(
                "The property '{0}' cannot be used in the $filter query option.",
                propertyName);
            var option = new FilterQueryOption(query, _productContext);

            // Act & Assert
            ExceptionAssert.Throws<ODataException>(() => _validator.Validate(option, settings), expectedMessage);
        }

        [Fact]
        public void LogicalOperatorsDataSet_CoversAllValues()
        {
            // Arrange
            // Get all values in the AllowedLogicalOperators enum.
            var values = new HashSet<AllowedLogicalOperators>(
                Enum.GetValues(typeof(AllowedLogicalOperators)).Cast<AllowedLogicalOperators>());
            var groupValues = new[]
            {
                AllowedLogicalOperators.All,
                AllowedLogicalOperators.None,
            };

            // Act
            // Remove the group items.
            foreach (var allowed in groupValues)
            {
                values.Remove(allowed);
            }

            // Remove the individual items.
            foreach (var allowed in LogicalOperators.Select(item => (AllowedLogicalOperators)(item[0])))
            {
                values.Remove(allowed);
            }

            // Assert
            // Should have nothing left.
            Assert.Empty(values);
        }

        [Theory]
        [MemberData(nameof(LogicalOperators))]
        public void AllowedLogicalOperators_SucceedIfAllowed(AllowedLogicalOperators allow, string query, string unused)
        {
            // Arrange
            var settings = new ODataValidationSettings
            {
                AllowedLogicalOperators = allow,
            };
            var option = new FilterQueryOption(query, _productContext);

            // Act & Assert
            Assert.NotNull(unused);
            ExceptionAssert.DoesNotThrow(() => _validator.Validate(option, settings));
        }

        [Theory]
        [MemberData(nameof(LogicalOperators))]
        public void AllowedLogicalOperators_ThrowIfNotAllowed(AllowedLogicalOperators exclude, string query, string operatorName)
        {
            // Arrange
            var settings = new ODataValidationSettings
            {
                AllowedLogicalOperators = AllowedLogicalOperators.All & ~exclude,
            };
            var expectedMessage = string.Format(
                "Logical operator '{0}' is not allowed. " +
                "To allow it, set the 'AllowedLogicalOperators' property on EnableQueryAttribute or QueryValidationSettings.",
                operatorName);
            var option = new FilterQueryOption(query, _productContext);

            // Act & Assert
            ExceptionAssert.Throws<ODataException>(() => _validator.Validate(option, settings), expectedMessage);
        }

        [Theory]
        [MemberData(nameof(LogicalOperators))]
        public void AllowedLogicalOperators_ThrowIfNoneAllowed(AllowedLogicalOperators unused, string query, string operatorName)
        {
            // Arrange
            var settings = new ODataValidationSettings
            {
                AllowedLogicalOperators = AllowedLogicalOperators.None,
            };
            var expectedMessage = string.Format(
                "Logical operator '{0}' is not allowed. " +
                "To allow it, set the 'AllowedLogicalOperators' property on EnableQueryAttribute or QueryValidationSettings.",
                operatorName);
            var option = new FilterQueryOption(query, _productContext);

            // Act & Assert
            Assert.NotEqual(unused, settings.AllowedLogicalOperators);
            ExceptionAssert.Throws<ODataException>(() => _validator.Validate(option, settings), expectedMessage);
        }

        [Theory]
        [MemberData(nameof(LogicalOperators_CheckArguments))]
        public void LogicalOperators_CheckArguments_SucceedIfAllowed(string query)
        {
            // Arrange
            var settings = new ODataValidationSettings
            {
                AllowedArithmeticOperators = AllowedArithmeticOperators.Add,
            };
            var option = new FilterQueryOption(query, _productContext);

            // Act & Assert
            ExceptionAssert.DoesNotThrow(() => _validator.Validate(option, settings));
        }

        [Theory]
        [MemberData(nameof(LogicalOperators_CheckArguments))]
        public void LogicalOperators_CheckArguments_ThrowIfNotAllowed(string query)
        {
            // Arrange
            var settings = new ODataValidationSettings
            {
                AllowedArithmeticOperators = AllowedArithmeticOperators.All & ~AllowedArithmeticOperators.Add,
            };
            var expectedMessage = string.Format(
                "Arithmetic operator 'Add' is not allowed. " +
                "To allow it, set the 'AllowedArithmeticOperators' property on EnableQueryAttribute or QueryValidationSettings.");
            var option = new FilterQueryOption(query, _productContext);

            // Act & Assert
            ExceptionAssert.Throws<ODataException>(() => _validator.Validate(option, settings), expectedMessage);
        }

        [Fact]
        public void ArithmeticNegation_SucceedsIfLogicalNotIsAllowed()
        {
            // Arrange
            var settings = new ODataValidationSettings
            {
                AllowedLogicalOperators = AllowedLogicalOperators.LessThan | AllowedLogicalOperators.Not,
            };
            var option = new FilterQueryOption("-UnitPrice lt 0", _productContext);

            // Act & Assert
            ExceptionAssert.DoesNotThrow(() => _validator.Validate(option, settings));
        }

        // Note Negate is _not_ a logical operator.
        [Fact]
        public void ArithmeticNegation_ThrowsIfLogicalNotIsNotAllowed()
        {
            // Arrange
            var settings = new ODataValidationSettings
            {
                AllowedLogicalOperators = AllowedLogicalOperators.LessThan,
            };
            var expectedMessage = string.Format(
                "Logical operator 'Negate' is not allowed. " +
                "To allow it, set the 'AllowedLogicalOperators' property on EnableQueryAttribute or QueryValidationSettings.");
            var option = new FilterQueryOption("-UnitPrice lt 0", _productContext);

            // Act & Assert
            ExceptionAssert.Throws<ODataException>(() => _validator.Validate(option, settings), expectedMessage);
        }

        [Fact]
        public void ValidateVisitLogicalOperatorEqual()
        {
            // Arrange
            FilterQueryOption option = new FilterQueryOption("Id eq 1", _context);

            // Act
            _validator.Validate(option, _settings);

            // Assert
            Assert.Equal(6, _validator.Times.Keys.Count);
            Assert.Equal(1, _validator.Times["Validate"]); // entry
            Assert.Equal(1, _validator.Times["ValidateSingleValuePropertyAccessNode"]); // Id
            Assert.Equal(1, _validator.Times["ValidateLogicalOperator"]); // eq
            Assert.Equal(1, _validator.Times["ValidateConstantQueryNode"]); // 1
            Assert.Equal(1, _validator.Times["ValidateBinaryOperatorQueryNode"]); // eq
            Assert.Equal(1, _validator.Times["ValidateParameterQueryNode"]); // $it
        }

        [Fact]
        public void ValidateVisitLogicalOperatorHas()
        {
            // Arrange
            FilterQueryOption option = new FilterQueryOption("FavoriteColor has Microsoft.AspNet.OData.Test.Builder.TestModels.Color'Red'", _context);

            // Act
            _validator.Validate(option, _settings);

            // Assert
            Assert.Equal(6, _validator.Times.Keys.Count);
            Assert.Equal(1, _validator.Times["Validate"]); // entry
            Assert.Equal(1, _validator.Times["ValidateSingleValuePropertyAccessNode"]); // FavouriteColor
            Assert.Equal(1, _validator.Times["ValidateLogicalOperator"]); // has
            Assert.Equal(1, _validator.Times["ValidateBinaryOperatorQueryNode"]); // has
            Assert.Equal(1, _validator.Times["ValidateParameterQueryNode"]); // $it
        }

        [Theory]
        [InlineData("Id eq 1")]
        [InlineData("Id ne 1")]
        [InlineData("Id gt 1")]
        [InlineData("Id lt 1")]
        [InlineData("Id ge 1")]
        [InlineData("Id le 1")]
        [InlineData("Id eq Id add 1")]
        [InlineData("Id eq Id sub 1")]
        [InlineData("Id eq Id mul 1")]
        [InlineData("Id eq Id div 1")]
        [InlineData("Id eq Id mod 1")]
        [InlineData("startswith(Name, 'Microsoft')")]
        [InlineData("endswith(Name, 'Microsoft')")]
        [InlineData("contains(Name, 'Microsoft')")]
        [InlineData("substring(Name, 1) eq 'Name'")]
        [InlineData("substring(Name, 1, 2) eq 'Name'")]
        [InlineData("length(Name) eq 1")]
        [InlineData("tolower(Name) eq 'Name'")]
        [InlineData("toupper(Name) eq 'Name'")]
        [InlineData("trim(Name) eq 'Name'")]
        [InlineData("indexof(Name, 'Microsoft') eq 1")]
        [InlineData("concat(Name, 'Microsoft') eq 'Microsoft'")]
        [InlineData("year(Birthday) eq 2000")]
        [InlineData("month(Birthday) eq 2000")]
        [InlineData("day(Birthday) eq 2000")]
        [InlineData("hour(Birthday) eq 2000")]
        [InlineData("minute(Birthday) eq 2000")]
        [InlineData("round(AmountSpent) eq 0")]
        [InlineData("floor(AmountSpent) eq 0")]
        [InlineData("ceiling(AmountSpent) eq 0")]
        [InlineData("Tags/any()")]
        [InlineData("Tags/all(t : t eq '1')")]
        [InlineData("Microsoft.AspNet.OData.Test.Query.QueryCompositionCustomerBase/Id eq 1")]
        [InlineData("Contacts/Microsoft.AspNet.OData.Test.Query.QueryCompositionCustomerBase/any()")]
        [InlineData("FavoriteColor has Microsoft.AspNet.OData.Test.Builder.TestModels.Color'Red'")]
        public void Validator_Doesnot_Throw_For_ValidQueries(string filter)
        {
            // Arrange
            FilterQueryOption option = new FilterQueryOption(filter, _context);

            // Act & Assert
            ExceptionAssert.DoesNotThrow(() => _validator.Validate(option, _settings));
        }

        [Fact]
        public void Validator_Doesnot_Throw_For_ParameterAlias()
        {
            // Arrange
            FilterQueryOption option = new FilterQueryOption(
                "Id eq @p",
                _context,
                new ODataQueryOptionParser(
                    _context.Model,
                    _context.ElementType,
                    _context.NavigationSource,
                    new Dictionary<string, string> { { "$filter", "Id eq @p" }, { "@p", "1" } }));

            // Act & Assert
            ExceptionAssert.DoesNotThrow(() => _validator.Validate(option, _settings));
            Assert.Equal(6, _validator.Times.Keys.Count);
            Assert.Equal(1, _validator.Times["Validate"]); // entry
            Assert.Equal(1, _validator.Times["ValidateParameterQueryNode"]); // $it
            Assert.Equal(1, _validator.Times["ValidateSingleValuePropertyAccessNode"]); // Id
            Assert.Equal(1, _validator.Times["ValidateBinaryOperatorQueryNode"]); // eq
            Assert.Equal(1, _validator.Times["ValidateLogicalOperator"]); // eq
            Assert.Equal(1, _validator.Times["ValidateConstantQueryNode"]); // 1
        }

        private static TheoryDataSet<string> GetLongInputsTestData(int maxCount)
        {
            return new TheoryDataSet<string>
                {
                    "" + String.Join(" and ", Enumerable.Range(1, (maxCount/5) + 1).Select(_ => "SupplierID eq 1")),
                    "" + String.Join(" ", Enumerable.Range(1, maxCount).Select(_ => "not")) + " Discontinued",
                    "" + String.Join(" add ", Enumerable.Range(1, maxCount/2)) + " eq 5050",
                    "" + String.Join("/", Enumerable.Range(1, maxCount/2).Select(_ => "Category/Product")) + "/ProductID eq 1",
                    "" + String.Join("/", Enumerable.Range(1, maxCount/2).Select(_ => "Category/Product")) + "/UnsignedReorderLevel eq 1",
                    //"" + Enumerable.Range(1,maxCount).Aggregate("'abc'", (prev,i) => String.Format("trim({0})", prev)) + " eq '123'", // Recursion Depth exceeded in Microsoft.OData.Core
                    " Category/Products/any(" + Enumerable.Range(1,maxCount/4).Aggregate("", (prev,i) => String.Format("p{1}: p{1}/Category/Products/any({0})", prev, i)) +")"
                };
        }

        private class MyFilterValidator : FilterQueryValidator
        {
            private Dictionary<string, int> _times = new Dictionary<string, int>();

            public MyFilterValidator(DefaultQuerySettings defaultQuerySettings)
                : base(defaultQuerySettings)
            {
            }

            public Dictionary<string, int> Times
            {
                get
                {
                    return _times;
                }
            }

            public override void Validate(FilterQueryOption filterQueryOption, ODataValidationSettings settings)
            {
                IncrementCount("Validate");
                base.Validate(filterQueryOption, settings);
            }

            public override void ValidateAllNode(AllNode allQueryNode, ODataValidationSettings settings)
            {
                IncrementCount("ValidateAllQueryNode");
                base.ValidateAllNode(allQueryNode, settings);
            }

            public override void ValidateAnyNode(AnyNode anyQueryNode, ODataValidationSettings settings)
            {
                IncrementCount("ValidateAnyQueryNode");
                base.ValidateAnyNode(anyQueryNode, settings);
            }

            public override void ValidateArithmeticOperator(BinaryOperatorNode binaryNode, ODataValidationSettings settings)
            {
                IncrementCount("ValidateArithmeticOperator");
                base.ValidateArithmeticOperator(binaryNode, settings);
            }

            public override void ValidateBinaryOperatorNode(BinaryOperatorNode binaryOperatorNode, ODataValidationSettings settings)
            {
                IncrementCount("ValidateBinaryOperatorQueryNode");
                base.ValidateBinaryOperatorNode(binaryOperatorNode, settings);
            }

            public override void ValidateConstantNode(ConstantNode constantNode, ODataValidationSettings settings)
            {
                IncrementCount("ValidateConstantQueryNode");
                base.ValidateConstantNode(constantNode, settings);
            }

            public override void ValidateConvertNode(ConvertNode convertQueryNode, ODataValidationSettings settings)
            {
                IncrementCount("ValidateConvertQueryNode");
                base.ValidateConvertNode(convertQueryNode, settings);
            }

            public override void ValidateLogicalOperator(BinaryOperatorNode binaryNode, ODataValidationSettings settings)
            {
                IncrementCount("ValidateLogicalOperator");
                base.ValidateLogicalOperator(binaryNode, settings);
            }

            public override void ValidateNavigationPropertyNode(QueryNode sourceNode, IEdmNavigationProperty navigationProperty, ODataValidationSettings settings)
            {
                IncrementCount("ValidateNavigationPropertyNode");
                base.ValidateNavigationPropertyNode(sourceNode, navigationProperty, settings);
            }

            public override void ValidateRangeVariable(RangeVariable rangeVariable, ODataValidationSettings settings)
            {
                IncrementCount("ValidateParameterQueryNode");
                base.ValidateRangeVariable(rangeVariable, settings);
            }

            public override void ValidateSingleValuePropertyAccessNode(SingleValuePropertyAccessNode propertyAccessNode, ODataValidationSettings settings)
            {
                IncrementCount("ValidateSingleValuePropertyAccessNode");
                base.ValidateSingleValuePropertyAccessNode(propertyAccessNode, settings);
            }

            public override void ValidateSingleComplexNode(SingleComplexNode singleComplexNode, ODataValidationSettings settings)
            {
                IncrementCount("ValidateSingleComplexNode");
                base.ValidateSingleComplexNode(singleComplexNode, settings);
            }

            public override void ValidateCollectionPropertyAccessNode(CollectionPropertyAccessNode propertyAccessNode, ODataValidationSettings settings)
            {
                IncrementCount("ValidateCollectionPropertyAccessNode");
                base.ValidateCollectionPropertyAccessNode(propertyAccessNode, settings);
            }

            public override void ValidateCollectionComplexNode(CollectionComplexNode collectionComplexNode, ODataValidationSettings settings)
            {
                IncrementCount("ValidateCollectionComplexNode");
                base.ValidateCollectionComplexNode(collectionComplexNode, settings);
            }

            public override void ValidateSingleValueFunctionCallNode(SingleValueFunctionCallNode node, ODataValidationSettings settings)
            {
                IncrementCount("ValidateSingleValueFunctionCallQueryNode");
                base.ValidateSingleValueFunctionCallNode(node, settings);
            }

            public override void ValidateUnaryOperatorNode(UnaryOperatorNode unaryOperatorQueryNode, ODataValidationSettings settings)
            {
                IncrementCount("ValidateUnaryOperatorQueryNode");
                base.ValidateUnaryOperatorNode(unaryOperatorQueryNode, settings);
            }

            private void IncrementCount(string functionName)
            {
                int count = 0;
                if (_times.TryGetValue(functionName, out count))
                {
                    _times[functionName] = ++count;
                }
                else
                {
                    // first time
                    _times[functionName] = 1;
                }
            }
        }
    }
}
