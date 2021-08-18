//-----------------------------------------------------------------------------
// <copyright file="QueryBuilder.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Test.E2E.AspNet.OData.Common.Instancing;

namespace Microsoft.Test.E2E.AspNet.OData.QueryComposition.Fuzzing
{
    public class QueryBuilder
    {
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1305:FieldNamesMustNotUseHungarianNotation", Justification = "'or' and 'and' are not hungarian notation.")]
        public static Syntax CreateFilterQuerySyntax()
        {
            var boolLiteralExpr = new Literal("true") | new Literal("false");
            var stringLiteralExpr = new Literal("'stringLiternal'");
            var datetimeOffsetLiteralExpr = new Literal("2012-09-11T00:09:00%2B08:00");
            var nullLiteralExpr = new Literal("null");
            var binaryLiteralExpr = new Literal("binary'AQIE'"); // AQIE equals to new byte { 1, 2, 3} to Base64
            var decimalLiteralExpr = new Literal("123.123M");
            var doubleLiteralExpr = new Literal("1E%2B10") | new Literal("2.029");
            var floatLiteralExpr = new Literal("2.0f");
            var guidLiteralExpr = new Literal("guid'9bcaf17a-7414-4e97-89e7-6f84a2f16280'");
            var int32LiteralExpr = new Literal("123");
            var int64LiteralExpr = new Literal("-9223372036854775808L");
            var charLiteralExpr = new Literal("a");

            var memberPrefixPathExpr = Literal.Empty
                | new Literal("ComplexTypeProperty/")
                | new Literal("SingleNavigationProperty/").Occurs(1, 5);

            var boolMemberExpr = memberPrefixPathExpr + new Literal("BoolProperty");
            var stringMemberExpr = memberPrefixPathExpr + (new Literal("StringProperty"));
            var dateTimeMemberExpr = memberPrefixPathExpr + new Literal("DateTimeOffsetProperty");
            var nullableMemberExpr =
                (memberPrefixPathExpr + (
                    new Literal("StringProperty")
                    | new Literal("DateTimeOffsetProperty")
                    | new Literal("DecimalProperty")));
            var binaryMemberExpr = memberPrefixPathExpr + new Literal("ByteArrayProperty");
            var decimalMemberExpr = memberPrefixPathExpr + new Literal("DecimalProperty");
            var doubleMemberExpr = memberPrefixPathExpr + new Literal("DoubleProperty");
            var floatMemberExpr = memberPrefixPathExpr + new Literal("FloatProperty");
            var guidMemberExpr = memberPrefixPathExpr + new Literal("GuidProperty");
            var int64MemberExpr = memberPrefixPathExpr + new Literal("LongProperty");
            var int32MemberExpr = memberPrefixPathExpr +
                (new Literal("Int32Property"));
            var charMemberExpr = (memberPrefixPathExpr + new Literal("CharProperty"));

            var stringCommonExpr = (stringLiteralExpr | stringMemberExpr).AsNonTerminal();
            var dateTimeCommonExpr = datetimeOffsetLiteralExpr | dateTimeMemberExpr;
            var nullableCommonExpr = nullLiteralExpr | nullableMemberExpr;
            var binaryCommonExpr = binaryLiteralExpr | binaryMemberExpr;
            var decimalCommonExpr = (decimalLiteralExpr | decimalMemberExpr).AsNonTerminal();
            var doubleCommonExpr = (doubleLiteralExpr | doubleMemberExpr).AsNonTerminal();
            var floatCommonExpr = (floatLiteralExpr | floatMemberExpr).AsNonTerminal();
            var guidCommonExpr = guidLiteralExpr | guidMemberExpr;
            var int64CommonExpr = (int64LiteralExpr | int64MemberExpr).AsNonTerminal();
            var boolCommonExpr = (boolLiteralExpr | boolMemberExpr).AsNonTerminal();
            var int32CommonExpr = (int32LiteralExpr | int32MemberExpr).AsNonTerminal();
            var charCommonExpr = (charMemberExpr | charLiteralExpr);

            var commonExprs = new Syntax[] 
            {
                stringCommonExpr,
                dateTimeCommonExpr,
                binaryCommonExpr,
                decimalCommonExpr,
                guidCommonExpr,
                int64CommonExpr,
                int32CommonExpr,
                boolCommonExpr,
                charCommonExpr,
                doubleCommonExpr,
                floatCommonExpr
            };
            var comparableExprs = new Syntax[] 
            {
                dateTimeCommonExpr,
                decimalCommonExpr,                
                doubleCommonExpr,
                floatCommonExpr,
                int64CommonExpr,
                int32CommonExpr
            };
            var arithmeticParameters = new NonTerminalSyntax[]
            {
                decimalCommonExpr,                
                doubleCommonExpr,
                floatCommonExpr,
                int64CommonExpr,
                int32CommonExpr
            };

            foreach (var s in arithmeticParameters)
            {
                s.Syntax |= (s + " add " + s)
                    | (s + " sub " + s)
                    | (s + " mul " + s)
                    | (s + " div " + s)
                    | (s + " mod " + s);
            }

            var andExpr = boolCommonExpr + " and " + boolCommonExpr;
            var orExpr = boolCommonExpr + " or " + boolCommonExpr;
            var notExpr = new Literal("not ") + boolCommonExpr;
            var eqExpr = commonExprs.Select(s => s + " eq " + s).Aggregate((s1, s2) => s1 | s2)
                | nullableCommonExpr + " eq " + nullLiteralExpr;
            var neExpr = commonExprs.Select(s => s + " ne " + s).Aggregate((s1, s2) => s1 | s2)
                | nullableCommonExpr + " ne " + nullLiteralExpr;
            var ltExpr = comparableExprs.Select(s => s + " lt " + s).Aggregate((s1, s2) => s1 | s2);
            var gtExpr = comparableExprs.Select(s => s + " gt " + s).Aggregate((s1, s2) => s1 | s2);
            var leExpr = comparableExprs.Select(s => s + " le " + s).Aggregate((s1, s2) => s1 | s2);
            var geExpr = comparableExprs.Select(s => s + " ge " + s).Aggregate((s1, s2) => s1 | s2);
            boolCommonExpr.Syntax = new Literal("(")
                + (boolCommonExpr.Syntax
                    | andExpr
                    | orExpr
                    | eqExpr
                    | neExpr
                    | ltExpr
                    | gtExpr
                    | leExpr
                    | geExpr
                    | notExpr)
                + ")";

            // return bool func call
            var endsWithMethodCallExpr = new Literal("endswith(") + stringCommonExpr + ", " + stringCommonExpr + ")";
            var startsWithMethodCallExpr = new Literal("startswith(") + stringCommonExpr + ", " + stringCommonExpr + ")";
            var containsMethodCallExpr = new Literal("contains(") + stringCommonExpr + ", " + stringCommonExpr + ")";

            // return string func call
            var toLowerMethodCallExpr = new Literal("tolower(") + stringCommonExpr + ")";
            var toUpperMethodCallExpr = new Literal("toupper(") + stringCommonExpr + ")";
            var trimMethodCallExpr = new Literal("trim(") + stringCommonExpr + ")";
            var substringMethodCallExp = new Literal("substring(") + stringCommonExpr + ", 1, 10)";
            var concatMethodCallExpr = new Literal("concat(") + stringCommonExpr + ", " + stringCommonExpr + ")";

            // return int func call
            var lengthMethodCallExpr = new Literal("length(") + stringCommonExpr + ")";
            var yearMethodCallExpr = new Literal("year(") + dateTimeCommonExpr + ")";
            var monthMethodCallExpr = new Literal("month(") + dateTimeCommonExpr + ")";
            var dayMethodCallExpr = new Literal("day(") + dateTimeCommonExpr + ")";
            var hourMethodCallExpr = new Literal("hour(") + dateTimeCommonExpr + ")";
            var minuteMethodCallExpr = new Literal("minute(") + dateTimeCommonExpr + ")";
            var secondMethodCallExpr = new Literal("second(") + dateTimeCommonExpr + ")";
            var indexOfMethodCallExpr = new Literal("indexof(") + stringCommonExpr + ", " + stringCommonExpr + ")";

            // return decimal func call
            var roundDecimalMethodCallExpr = new Literal("round(") + decimalCommonExpr + ")";
            var floorDecimalMethodCallExpr = new Literal("floor(") + decimalCommonExpr + ")";
            var ceilingDecimalMethodCallExpr = new Literal("ceiling(") + decimalCommonExpr + ")";

            // return double func call
            var roundDoubleMethodCallExpr = new Literal("round(") + doubleCommonExpr + ")";
            var floorDoubleMethodCallExpr = new Literal("floor(") + doubleCommonExpr + ")";
            var ceilingDoubleMethodCallExpr = new Literal("ceiling(") + doubleCommonExpr + ")";

            boolCommonExpr.Syntax |= endsWithMethodCallExpr
                | startsWithMethodCallExpr
                | containsMethodCallExpr;

            stringCommonExpr.Syntax |= toLowerMethodCallExpr
                | toUpperMethodCallExpr
                | trimMethodCallExpr
                | substringMethodCallExp
                | concatMethodCallExpr;

            int32CommonExpr.Syntax |= indexOfMethodCallExpr
                | lengthMethodCallExpr
                | yearMethodCallExpr
                | monthMethodCallExpr
                | dayMethodCallExpr
                | hourMethodCallExpr
                | minuteMethodCallExpr
                | secondMethodCallExpr;

            decimalCommonExpr.Syntax |= roundDecimalMethodCallExpr
                | floorDecimalMethodCallExpr
                | ceilingDecimalMethodCallExpr;

            doubleCommonExpr.Syntax |= roundDoubleMethodCallExpr
                | floorDoubleMethodCallExpr
                | ceilingDoubleMethodCallExpr;

            return boolCommonExpr;
        }

        public static Syntax CreateODataQuerySyntax()
        {
            var top = new Literal("$top=10");
            var skip = new Literal("$skip=10");

            var primitiveColumns = new Literal("StringProperty") | "DateTimeOffsetProperty" | "DecimalProperty" | "LongProperty" | "BoolProperty" | "GuidProperty" | "Int32Property";
            var complexPropertyColumns = new Literal("ComplexTypeProperty/") + primitiveColumns;
            var columns = complexPropertyColumns | primitiveColumns;

            var columnsOrder = columns + (new Literal(",") + columns).Occurs(0, 2);

            var orderby = new Literal("$orderby=") + columnsOrder;

            var filter = new Literal("$filter=") + CreateFilterQuerySyntax();

            var query = top | skip | orderby | filter;

            return query + (new Literal(@"&") + query).Occurs(0, 10);
        }
    }
}
