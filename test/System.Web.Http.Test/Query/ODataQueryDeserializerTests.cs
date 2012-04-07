// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using System.Linq.Expressions;
using Microsoft.TestCommon;
using Xunit;
using Xunit.Extensions;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Web.Http.Query
{
    public class ODataQueryDeserializerTests
    {
        [Fact]
        public void SimpleMultipartQuery()
        {
            VerifyQueryDeserialization(
                "$filter=ProductName eq 'Doritos'&$orderby=UnitPrice&$top=100",
                "Where(Param_0 => (Param_0.ProductName == \"Doritos\")).OrderBy(Param_1 => Param_1.UnitPrice).Take(100)");
        }

        #region Ordering
        [Fact]
        public void OrderBy()
        {
            VerifyQueryDeserialization(
                "$orderby=UnitPrice",
                "OrderBy(Param_0 => Param_0.UnitPrice)");
        }

        [Fact]
        public void OrderByAscending()
        {
            VerifyQueryDeserialization(
                "$orderby=UnitPrice asc",
                "OrderBy(Param_0 => Param_0.UnitPrice)");
        }

        [Fact]
        public void OrderByDescending()
        {
            VerifyQueryDeserialization(
                "$orderby=UnitPrice desc",
                "OrderByDescending(Param_0 => Param_0.UnitPrice)");
        }

        [Fact]
        public void OrderByAscendingThenDesscending()
        {
            VerifyQueryDeserialization(
                "$orderby=UnitPrice desc, ProductName asc",
                "OrderByDescending(Param_0 => Param_0.UnitPrice).ThenBy(Param_0 => Param_0.ProductName)");
        }

        [Theory]
        [InlineData("UnitPrice desc, ProductName asc", "OrderByDescending(Param_0 => Param_0.UnitPrice).ThenBy(Param_0 => Param_0.ProductName)")]
        [InlineData("UnitPrice desc, ProductName desc", "OrderByDescending(Param_0 => Param_0.UnitPrice).ThenByDescending(Param_0 => Param_0.ProductName)")]
        [InlineData("UnitPrice     , ProductName ", "OrderBy(Param_0 => Param_0.UnitPrice).ThenBy(Param_0 => Param_0.ProductName)")]
        [InlineData("Discontinued, UnitsOnOrder, DiscontinuedDate desc", "OrderBy(Param_0 => Param_0.Discontinued).ThenBy(Param_0 => Param_0.UnitsOnOrder).ThenByDescending(Param_0 => Param_0.DiscontinuedDate)")]
        public void MultipleOrderBy(string clause, string expressionResult)
        {
            VerifyQueryDeserialization(
               "$orderby=" + clause,
               expressionResult);
        }

        #endregion

        #region Inequalities
        [Fact]
        public void EqualityOperator()
        {
            VerifyQueryDeserialization(
                "$filter=ProductName eq 'Doritos'",
                "Where(Param_0 => (Param_0.ProductName == \"Doritos\"))");
        }

        [Fact]
        public void NotEqualOperator()
        {
            VerifyQueryDeserialization(
                "$filter=ProductName ne 'Doritos'",
                "Where(Param_0 => (Param_0.ProductName != \"Doritos\"))");
        }

        [Fact]
        public void GreaterThanOperator()
        {
            VerifyQueryDeserialization(
                "$filter=UnitPrice gt 5.00",
                "Where(Param_0 => (Param_0.UnitPrice > 5.00))");
        }

        [Fact]
        public void GreaterThanEqualOperator()
        {
            VerifyQueryDeserialization(
                "$filter=UnitPrice ge 5.00",
                "Where(Param_0 => (Param_0.UnitPrice >= 5.00))");
        }

        [Fact]
        public void LessThanOperator()
        {
            VerifyQueryDeserialization(
                "$filter=UnitPrice lt 5.00",
                "Where(Param_0 => (Param_0.UnitPrice < 5.00))");
        }

        [Fact]
        public void LessThanOrEqualOperator()
        {
            VerifyQueryDeserialization(
                "$filter=UnitPrice le 5.00",
                "Where(Param_0 => (Param_0.UnitPrice <= 5.00))");
        }

        [Fact]
        public void NegativeNumbers()
        {
            VerifyQueryDeserialization(
                "$filter=UnitPrice le -5.00",
                "Where(Param_0 => (Param_0.UnitPrice <= -5.00))");
        }

        [Theory]
        [InlineData("DateTimeProp eq datetime'2000-12-12T12:00'", "Where(Param_0 => (Param_0.DateTimeProp == 12/12/2000 12:00:00 PM))")]
        [InlineData("DateTimeOffsetProp eq datetimeoffset'2002-10-10T17:00:00Z'", "Where(Param_0 => (Param_0.DateTimeOffsetProp == 10/10/2002 5:00:00 PM +00:00))")]
        [InlineData("DateTimeOffsetProp eq DateTimeOffsetProp", "Where(Param_0 => (Param_0.DateTimeOffsetProp == Param_0.DateTimeOffsetProp))")]
        [InlineData("DateTimeOffsetProp ne DateTimeOffsetProp", "Where(Param_0 => (Param_0.DateTimeOffsetProp != Param_0.DateTimeOffsetProp))")]
        [InlineData("DateTimeOffsetProp ge DateTimeOffsetProp", "Where(Param_0 => (Param_0.DateTimeOffsetProp >= Param_0.DateTimeOffsetProp))")]
        [InlineData("DateTimeOffsetProp le DateTimeOffsetProp", "Where(Param_0 => (Param_0.DateTimeOffsetProp <= Param_0.DateTimeOffsetProp))")]
        public void DateInEqualities(string clause, string expectedExpression)
        {
            VerifyQueryDeserialization<DataTypes>(
                "$filter=" + clause,
                expectedExpression);
        }

        #endregion

        #region Logical Operators
        [Fact]
        public void OrOperator()
        {
            VerifyQueryDeserialization(
                "$filter=UnitPrice eq 5.00 or UnitPrice eq 10.00",
                "Where(Param_0 => ((Param_0.UnitPrice == 5.00) OrElse (Param_0.UnitPrice == 10.00)))");
        }

        [Fact]
        public void AndOperator()
        {
            VerifyQueryDeserialization(
                "$filter=UnitPrice eq 5.00 and UnitPrice eq 10.00",
                "Where(Param_0 => ((Param_0.UnitPrice == 5.00) AndAlso (Param_0.UnitPrice == 10.00)))");
        }

        [Fact]
        public void Negation()
        {
            VerifyQueryDeserialization(
                "$filter=not (UnitPrice eq 5.00)",
                "Where(Param_0 => Not((Param_0.UnitPrice == 5.00)))");
        }

        [Fact]
        public void BoolNegation()
        {
            VerifyQueryDeserialization(
                "$filter=not Discontinued",
                "Where(Param_0 => Not(Param_0.Discontinued))");
        }

        [Fact]
        public void NestedNegation()
        {
            VerifyQueryDeserialization(
                "$filter=not (not(not    (Discontinued)))",
                "Where(Param_0 => Not(Not(Not(Param_0.Discontinued))))");
        }
        #endregion

        #region Arithmetic Operators
        [Fact]
        public void Subtraction()
        {
            VerifyQueryDeserialization(
                "$filter=UnitPrice sub 1.00 lt 5.00",
                "Where(Param_0 => ((Param_0.UnitPrice - 1.00) < 5.00))");
        }

        [Fact]
        public void Addition()
        {
            VerifyQueryDeserialization(
                "$filter=UnitPrice add 1.00 lt 5.00",
                "Where(Param_0 => ((Param_0.UnitPrice + 1.00) < 5.00))");
        }

        [Fact]
        public void Multiplication()
        {
            VerifyQueryDeserialization(
                "$filter=UnitPrice mul 1.00 lt 5.00",
                "Where(Param_0 => ((Param_0.UnitPrice * 1.00) < 5.00))");
        }

        [Fact]
        public void Division()
        {
            VerifyQueryDeserialization(
                "$filter=UnitPrice div 1.00 lt 5.00",
                "Where(Param_0 => ((Param_0.UnitPrice / 1.00) < 5.00))");
        }

        [Fact]
        public void Modulo()
        {
            VerifyQueryDeserialization(
                "$filter=UnitPrice mod 1.00 lt 5.00",
                "Where(Param_0 => ((Param_0.UnitPrice % 1.00) < 5.00))");
        }
        #endregion

        [Fact]
        public void Grouping()
        {
            VerifyQueryDeserialization(
                "$filter=((ProductName ne 'Doritos') or (UnitPrice lt 5.00))",
                "Where(Param_0 => ((Param_0.ProductName != \"Doritos\") OrElse (Param_0.UnitPrice < 5.00)))");
        }

        [Fact]
        public void MemberExpressions()
        {
            VerifyQueryDeserialization(
                "$filter=Category/CategoryName eq 'Snacks'",
                "Where(Param_0 => (Param_0.Category.CategoryName == \"Snacks\"))");
        }

        #region String Functions
        [Fact]
        public void StringSubstringOf()
        {
            // In OData, the order of parameters is actually reversed in the resulting
            // string.Contains expression

            VerifyQueryDeserialization(
                "$filter=substringof('Abc', ProductName) eq true",
                "Where(Param_0 => (Param_0.ProductName.Contains(\"Abc\") == True))");

            VerifyQueryDeserialization(
                "$filter=substringof(ProductName, 'Abc') eq true",
                "Where(Param_0 => (\"Abc\".Contains(Param_0.ProductName) == True))");
        }

        [Fact]
        public void StringStartsWith()
        {
            VerifyQueryDeserialization(
                "$filter=startswith(ProductName, 'Abc') eq true",
                "Where(Param_0 => (Param_0.ProductName.StartsWith(\"Abc\") == True))");
        }

        [Fact]
        public void StringEndsWith()
        {
            VerifyQueryDeserialization(
                "$filter=endswith(ProductName, 'Abc') eq true",
                "Where(Param_0 => (Param_0.ProductName.EndsWith(\"Abc\") == True))");
        }

        [Fact]
        public void StringLength()
        {
            VerifyQueryDeserialization(
                "$filter=length(ProductName) gt 0",
                "Where(Param_0 => (Param_0.ProductName.Length > 0))");
        }

        [Fact]
        public void StringIndexOf()
        {
            VerifyQueryDeserialization(
                "$filter=indexof(ProductName, 'Abc') eq 5",
                "Where(Param_0 => (Param_0.ProductName.IndexOf(\"Abc\") == 5))");
        }

        [Fact]
        public void StringReplace()
        {
            VerifyQueryDeserialization(
                "$filter=replace(ProductName, 'Abc', 'Def') eq 'FooDef'",
                "Where(Param_0 => (Param_0.ProductName.Replace(\"Abc\", \"Def\") == \"FooDef\"))");
        }

        [Fact]
        public void StringSubstring()
        {
            VerifyQueryDeserialization(
                "$filter=substring(ProductName, 3) eq 'uctName'",
                "Where(Param_0 => (Param_0.ProductName.Substring(3) == \"uctName\"))");

            VerifyQueryDeserialization(
                "$filter=substring(ProductName, 3, 4) eq 'uctN'",
                "Where(Param_0 => (Param_0.ProductName.Substring(3, 4) == \"uctN\"))");
        }

        [Fact]
        public void StringToLower()
        {
            VerifyQueryDeserialization(
                "$filter=tolower(ProductName) eq 'tasty treats'",
                "Where(Param_0 => (Param_0.ProductName.ToLower() == \"tasty treats\"))");
        }

        [Fact]
        public void StringToUpper()
        {
            VerifyQueryDeserialization(
                "$filter=toupper(ProductName) eq 'TASTY TREATS'",
                "Where(Param_0 => (Param_0.ProductName.ToUpper() == \"TASTY TREATS\"))");
        }

        [Fact]
        public void StringTrim()
        {
            VerifyQueryDeserialization(
                "$filter=trim(ProductName) eq 'Tasty Treats'",
                "Where(Param_0 => (Param_0.ProductName.Trim() == \"Tasty Treats\"))");
        }

        [Fact]
        public void StringConcat()
        {
            VerifyQueryDeserialization(
                "$filter=concat('Foo', 'Bar') eq 'FooBar'",
                "Where(Param_0 => (Concat(\"Foo\", \"Bar\") == \"FooBar\"))");
        }
        #endregion

        #region Date Functions
        [Fact]
        public void DateDay()
        {
            VerifyQueryDeserialization(
                "$filter=day(DiscontinuedDate) eq 8",
                "Where(Param_0 => (Param_0.DiscontinuedDate.Day == 8))");
        }

        [Fact]
        public void DateMonth()
        {
            VerifyQueryDeserialization(
                "$filter=month(DiscontinuedDate) eq 8",
                "Where(Param_0 => (Param_0.DiscontinuedDate.Month == 8))");
        }

        [Fact]
        public void DateYear()
        {
            VerifyQueryDeserialization(
                "$filter=year(DiscontinuedDate) eq 1974",
                "Where(Param_0 => (Param_0.DiscontinuedDate.Year == 1974))");
        }

        [Fact]
        public void DateHour()
        {
            VerifyQueryDeserialization("$filter=hour(DiscontinuedDate) eq 8",
                "Where(Param_0 => (Param_0.DiscontinuedDate.Hour == 8))");
        }

        [Fact]
        public void DateMinute()
        {
            VerifyQueryDeserialization(
                "$filter=minute(DiscontinuedDate) eq 12",
                "Where(Param_0 => (Param_0.DiscontinuedDate.Minute == 12))");
        }

        [Fact]
        public void DateSecond()
        {
            VerifyQueryDeserialization(
                "$filter=second(DiscontinuedDate) eq 33",
                "Where(Param_0 => (Param_0.DiscontinuedDate.Second == 33))");
        }
        #endregion

        #region Math Functions
        [Fact]
        public void MathRound()
        {
            VerifyQueryDeserialization(
                "$filter=round(UnitPrice) gt 5.00",
                "Where(Param_0 => (Round(Param_0.UnitPrice) > 5.00))");
        }

        [Fact]
        public void MathFloor()
        {
            VerifyQueryDeserialization(
                "$filter=floor(UnitPrice) eq 5",
                "Where(Param_0 => (Floor(Param_0.UnitPrice) == 5))");
        }

        [Fact]
        public void MathCeiling()
        {
            VerifyQueryDeserialization(
                "$filter=ceiling(UnitPrice) eq 5",
                "Where(Param_0 => (Ceiling(Param_0.UnitPrice) == 5))");
        }
        #endregion

        #region Data Types
        [Fact]
        public void GuidExpression()
        {
            VerifyQueryDeserialization<DataTypes>(
                "$filter=GuidProp eq guid'0EFDAECF-A9F0-42F3-A384-1295917AF95E'",
                "Where(Param_0 => (Param_0.GuidProp == 0efdaecf-a9f0-42f3-a384-1295917af95e))");

            // verify case insensitivity
            VerifyQueryDeserialization<DataTypes>(
                "$filter=GuidProp eq GuiD'0EFDAECF-A9F0-42F3-A384-1295917AF95E'",
                "Where(Param_0 => (Param_0.GuidProp == 0efdaecf-a9f0-42f3-a384-1295917af95e))");
        }

        [Fact]
        public void DateTimeExpression()
        {
            VerifyQueryDeserialization<DataTypes>(
                "$filter=DateTimeProp lt datetime'2000-12-12T12:00'",
                "Where(Param_0 => (Param_0.DateTimeProp < 12/12/2000 12:00:00 PM))");
        }

        [Fact]
        public void DateTimeOffsetExpression()
        {
            VerifyQueryDeserialization<DataTypes>(
                "$filter=DateTimeOffsetProp ge datetimeoffset'2002-10-10T17:00:00Z'",
                "Where(Param_0 => (Param_0.DateTimeOffsetProp >= 10/10/2002 5:00:00 PM +00:00))");
        }

        [Fact]
        public void TimeExpression()
        {
            VerifyQueryDeserialization<DataTypes>(
                "$filter=TimeSpanProp ge time'13:20:00'",
                "Where(Param_0 => (Param_0.TimeSpanProp >= 13:20:00))");

            // verify parse error for invalid literal format
            Assert.Throws<ParseException>(delegate
            {
                VerifyQueryDeserialization<DataTypes>("$filter=TimeSpanProp ge time'invalid'", String.Empty);
            },
            "Parse error in $filter. String was not recognized as a valid TimeSpan. (at index 20)");
        }

        [Fact]
        public void BinaryExpression()
        {
            string binary = "23ABFF";
            byte[] bytes = new byte[] { 
                byte.Parse("23", Globalization.NumberStyles.HexNumber),
                byte.Parse("AB", Globalization.NumberStyles.HexNumber),
                byte.Parse("FF", Globalization.NumberStyles.HexNumber)
            };

            VerifyQueryDeserialization<DataTypes>(
                String.Format("$filter=ByteArrayProp eq binary'{0}'", binary),
                "Where(Param_0 => (Param_0.ByteArrayProp == value(System.Byte[])))",
                q =>
                {
                    // verify that the binary data was deserialized into a constant expression of type byte[]
                    LambdaExpression lex = (LambdaExpression)((UnaryExpression)((MethodCallExpression)q.Expression).Arguments[1]).Operand;
                    BinaryExpression bex = (BinaryExpression)lex.Body;
                    byte[] actualBytes = (byte[])((ConstantExpression)bex.Right).Value;
                    Assert.True(actualBytes.SequenceEqual(bytes));
                });

            // test alternate 'X' syntax
            VerifyQueryDeserialization<DataTypes>(
                String.Format("$filter=ByteArrayProp eq X'{0}'", binary),
                "Where(Param_0 => (Param_0.ByteArrayProp == value(System.Byte[])))",
                q =>
                {
                    // verify that the binary data was deserialized into a constant expression of type byte[]
                    LambdaExpression lex = (LambdaExpression)((UnaryExpression)((MethodCallExpression)q.Expression).Arguments[1]).Operand;
                    BinaryExpression bex = (BinaryExpression)lex.Body;
                    byte[] actualBytes = (byte[])((ConstantExpression)bex.Right).Value;
                    Assert.True(actualBytes.SequenceEqual(bytes));
                });

            // verify parse error for invalid literal format
            Assert.Throws<ParseException>(delegate
            {
                VerifyQueryDeserialization<DataTypes>(
                 String.Format("$filter=ByteArrayProp eq binary'{0}'", "WXYZ"), String.Empty);
            },
            "Parse error in $filter. Input string was not in a correct format. (at index 23)");

            // verify parse error for invalid hex literal (odd hex strings are not supported)
            Assert.Throws<ParseException>(delegate
            {
                VerifyQueryDeserialization<DataTypes>(
                 String.Format("$filter=ByteArrayProp eq binary'23A'", "XYZ"), String.Empty);
            },
            "Parse error in $filter. Invalid hexadecimal literal. (at index 23)");
        }

        [Fact]
        public void IntegerLiteralSuffix()
        {
            // long L
            VerifyQueryDeserialization<DataTypes>(
                "$filter=LongProp lt 987654321L and LongProp gt 123456789l",
                "Where(Param_0 => ((Param_0.LongProp < 987654321) AndAlso (Param_0.LongProp > 123456789)))");

            VerifyQueryDeserialization<DataTypes>(
                "$filter=LongProp lt -987654321L and LongProp gt -123456789l",
                "Where(Param_0 => ((Param_0.LongProp < -987654321) AndAlso (Param_0.LongProp > -123456789)))");
        }

        [Fact]
        public void RealLiteralSuffixes()
        {
            // Float F
            VerifyQueryDeserialization<DataTypes>(
                "$filter=FloatProp lt 4321.56F and FloatProp gt 1234.56f",
                "Where(Param_0 => ((Param_0.FloatProp < 4321.56) AndAlso (Param_0.FloatProp > 1234.56)))");

            // Decimal M
            VerifyQueryDeserialization<DataTypes>(
                "$filter=DecimalProp lt 4321.56M and DecimalProp gt 1234.56m",
                "Where(Param_0 => ((Param_0.DecimalProp < 4321.56) AndAlso (Param_0.DecimalProp > 1234.56)))");
        }

        [Theory]
        [InlineData("'hello,world'", "hello,world")]
        [InlineData("'''hello,world'", "'hello,world")]
        [InlineData("'hello,world'''", "hello,world'")]
        [InlineData("'hello,''wor''ld'", "hello,'wor'ld")]
        [InlineData("'hello,''''''world'", "hello,'''world")]
        [InlineData("'\"hello,world\"'", "\"hello,world\"")]
        [InlineData("'\"hello,world'", "\"hello,world")]
        [InlineData("'hello,world\"'", "hello,world\"")]
        [InlineData("'hello,\"world'", "hello,\"world")]
        [InlineData("'México D.F.'", "México D.F.")]
        public void StringLiterals(string literal, string expected)
        {
            VerifyQueryDeserialization<Product>(
                "$filter=ProductName eq " + literal,
                String.Format("Where(Param_0 => (Param_0.ProductName == \"{0}\"))", expected));
        }
        #endregion

        #region Negative tests
        [Theory]
        [InlineData('+')]
        [InlineData('*')]
        [InlineData('%')]
        [InlineData('[')]
        [InlineData(']')]
        public void InvalidCharactersInQuery_TokenizerFails(char ch)
        {
            Assert.Throws<ParseException>(delegate
            {
                string filter = String.Format("2 {0} 3", ch);
                VerifyQueryDeserialization<DataTypes>("$filter=" + Uri.EscapeDataString(filter), String.Empty);
            },
            String.Format("Parse error in $filter. Syntax error '{0}' (at index 2)", ch));
        }

        [Fact]
        public void InvalidTypeCreationExpression()
        {
            // underminated string literal
            Assert.Throws<ParseException>(delegate
            {
                VerifyQueryDeserialization<DataTypes>("$filter=TimeSpanProp ge time'13:20:00", String.Empty);
            },
            "Parse error in $filter. Unterminated string literal (at index 29)");

            // use of parens rather than quotes
            Assert.Throws<ParseException>(delegate
            {
                VerifyQueryDeserialization<DataTypes>("$filter=TimeSpanProp ge time(13:20:00)", String.Empty);
            },
            "Parse error in $filter. Invalid 'time' type creation expression. (at index 16)");

            // verify the exception returned when type expression that isn't
            // one of the supported keyword types is used. In this case it falls
            // through as a member expression
            Assert.Throws<ParseException>(delegate
            {
                VerifyQueryDeserialization("$filter=math'123' eq true", String.Empty);
            },
            "Parse error in $filter. No property or field 'math' exists in type 'Product' (at index 0)");
        }

        [Fact]
        public void InvalidMethodCall()
        {
            // incorrect casing of supported method
            Assert.Throws<ParseException>(delegate
            {
                VerifyQueryDeserialization("$filter=Startswith(ProductName, 'Abc') eq true", String.Empty);
            },
            "Parse error in $filter. Unknown identifier 'Startswith' (at index 0)");

            // attempt to access a method defined on the entity type
            Assert.Throws<ParseException>(delegate
            {
                VerifyQueryDeserialization<DataTypes>("$filter=Inaccessable() eq \"Bar\"", String.Empty);
            },
            "Parse error in $filter. Unknown identifier 'Inaccessable' (at index 0)");

            // verify that Type methods like string.PadLeft, etc. are not supported.
            Assert.Throws<ParseException>(delegate
            {
                VerifyQueryDeserialization("$filter=ProductName/PadLeft(100000000000000000000) eq \"Foo\"", String.Empty);
            },
            "Parse error in $filter. Unknown identifier 'PadLeft' (at index 12)");
        }

        [Fact]
        public void InvalidQueryParameterToTop()
        {
            Assert.Throws<ParseException>(
                () => VerifyQueryDeserialization("$top=-42", String.Empty),
                "The OData query parameter '$top' has an invalid value. The value should be a positive integer. The provided value was '-42'");
        }

        [Fact]
        public void InvalidQueryParameterToSkip()
        {
            Assert.Throws<ParseException>(
                () => VerifyQueryDeserialization("$skip=-42", String.Empty),
                "The OData query parameter '$skip' has an invalid value. The value should be a positive integer. The provided value was '-42'");
        }

        [Fact]
        public void InvalidProperty_NotExists()
        {
            Assert.Throws<ParseException>(
                () => VerifyQueryDeserialization("$filter=length mod n(ProductName) eq 12", String.Empty),
                "Parse error in $filter. No property or field 'length' exists in type 'Product' (at index 0)");
        }

        [Fact]
        public void InvalidFunctionCall_EmptyArguments()
        {
            Assert.Throws<ParseException>(
                () => VerifyQueryDeserialization("$filter=length() eq 12", String.Empty),
                "Parse error in $filter. No applicable method 'Length' exists in type 'System.String' (at index 0)");
        }

        [Theory]
        [InlineData("(2 add 3 eq 2", 13)]
        [InlineData("(2 add (3) eq 2", 15)]
        [InlineData("(((( 2 eq 2", 11)]
        public void Missing_Parantheses(string clause, int index)
        {
            Assert.Throws<ParseException>(
                () => VerifyQueryDeserialization("$filter=" + clause, String.Empty),
                String.Format("Parse error in $filter. ')' or operator expected (at index {0})", index));
        }

        [Theory]
        [InlineData("'hello,world", 12)]
        [InlineData("'''hello,world", 14)]
        [InlineData("'hello,world''", 14)]
        [InlineData("'hello,''wor''ld", 16)]
        public void UnterminatedStringLiterals(string clause, int index)
        {
            Assert.Throws<ParseException>(
                () => VerifyQueryDeserialization("$filter=" + clause, String.Empty),
                String.Format("Parse error in $filter. Unterminated string literal (at index {0})", index));
        }

        [Theory]
        [InlineData("\"hello,world\"", 0)]
        public void InvalidStringLiterals(string clause, int index)
        {
            Assert.Throws<ParseException>(
                () => VerifyQueryDeserialization("$filter=" + clause, String.Empty),
                String.Format("Parse error in $filter. Syntax error '\"' (at index {0})", index));
        }

        #endregion

        [Fact(DisplayName = "ODataQueryDeserializer is internal.")]
        public void TypeIsCorrect()
        {
            Assert.Type.HasProperties(typeof(ODataQueryDeserializer), TypeAssert.TypeProperties.IsStatic | TypeAssert.TypeProperties.IsClass);
        }

        /// <summary>
        /// Call the query deserializer and verify the results
        /// </summary>
        /// <param name="queryString">The URL query string to deserialize (e.g. $filter=ProductName eq 'Doritos')</param>
        /// <param name="expectedResult">The Expression.ToString() representation of the expected result (e.g. Where(Param_0 => (Param_0.ProductName == \"Doritos\"))</param>
        private void VerifyQueryDeserialization(string queryString, string expectedResult)
        {
            VerifyQueryDeserialization<Product>(queryString, expectedResult, null);
        }

        private void VerifyQueryDeserialization<T>(string queryString, string expectedResult)
        {
            VerifyQueryDeserialization<T>(queryString, expectedResult, null);
        }

        private void VerifyQueryDeserialization<T>(string queryString, string expectedResult, Action<IQueryable<T>> verify)
        {
            string uri = "http://myhost/odata.svc/Get?" + queryString;

            IQueryable<T> baseQuery = new T[0].AsQueryable();
            IQueryable<T> resultQuery = (IQueryable<T>)ODataQueryDeserializer.Deserialize(baseQuery, new Uri(uri));
            VerifyExpression(resultQuery, expectedResult);

            if (verify != null)
            {
                verify(resultQuery);
            }

            QueryValidator.Instance.Validate(resultQuery);
        }

        private void VerifyExpression(IQueryable query, string expectedExpression)
        {
            // strip off the beginning part of the expression to get to the first
            // actual query operator
            string resultExpression = query.Expression.ToString();
            int startIdx = (query.ElementType.FullName + "[]").Length + 1;
            resultExpression = resultExpression.Substring(startIdx);

            Assert.True(resultExpression == expectedExpression,
                String.Format("Expected expression '{0}' but the deserializer produced '{1}'", expectedExpression, resultExpression));
        }
    }
}
