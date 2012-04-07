// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Web.Http.Properties;

namespace System.Web.Http.Query
{
    internal static class DynamicQueryable
    {
        public static IQueryable Where(this IQueryable source, string predicate, QueryResolver queryResolver)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            if (predicate == null)
            {
                throw new ArgumentNullException("predicate");
            }

            LambdaExpression lambda = DynamicExpression.ParseLambda(source.ElementType, typeof(bool), predicate, queryResolver);

            return source.Provider.CreateQuery(
                Expression.Call(
                    typeof(Queryable),
                    "Where",
                    new Type[] { source.ElementType },
                    source.Expression,
                    Expression.Quote(lambda)));
        }

        public static IQueryable OrderBy(this IQueryable source, string ordering, QueryResolver queryResolver)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            if (ordering == null)
            {
                throw new ArgumentNullException("ordering");
            }

            ParameterExpression[] parameters = new ParameterExpression[] 
            {
                Expression.Parameter(source.ElementType, "") 
            };
            ExpressionParser parser = new ExpressionParser(parameters, ordering, queryResolver);
            IEnumerable<DynamicOrdering> orderings = parser.ParseOrdering();
            Expression queryExpr = source.Expression;
            string methodAsc = "OrderBy";
            string methodDesc = "OrderByDescending";
            foreach (DynamicOrdering o in orderings)
            {
                queryExpr = Expression.Call(
                    typeof(Queryable),
                    o.Ascending ? methodAsc : methodDesc,
                    new Type[] { source.ElementType, o.Selector.Type },
                    queryExpr,
                    Expression.Quote(DynamicExpression.Lambda(o.Selector, parameters)));
                methodAsc = "ThenBy";
                methodDesc = "ThenByDescending";
            }
            return source.Provider.CreateQuery(queryExpr);
        }

        public static IQueryable Take(this IQueryable source, int count)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            return source.Provider.CreateQuery(
                Expression.Call(
                    typeof(Queryable),
                    "Take",
                    new Type[] { source.ElementType },
                    source.Expression,
                    Expression.Constant(count)));
        }

        public static IQueryable Skip(this IQueryable source, int count)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            return source.Provider.CreateQuery(
                Expression.Call(
                    typeof(Queryable),
                    "Skip",
                    new Type[] { source.ElementType },
                    source.Expression,
                    Expression.Constant(count)));
        }

        internal static class DynamicExpression
        {
            static readonly Type[] funcTypes = new Type[] 
            {
                typeof(Func<>),
                typeof(Func<,>),
                typeof(Func<,,>),
                typeof(Func<,,,>),
                typeof(Func<,,,,>)
            };

            public static LambdaExpression ParseLambda(Type itType, Type resultType, string expression, QueryResolver queryResolver)
            {
                return ParseLambda(new ParameterExpression[] { Expression.Parameter(itType, "") }, resultType, expression, queryResolver);
            }

            public static LambdaExpression ParseLambda(ParameterExpression[] parameters, Type resultType, string expression, QueryResolver queryResolver)
            {
                ExpressionParser parser = new ExpressionParser(parameters, expression, queryResolver);
                return Lambda(parser.Parse(resultType), parameters);
            }

            public static LambdaExpression Lambda(Expression body, params ParameterExpression[] parameters)
            {
                int paramCount = parameters == null ? 0 : parameters.Length;

                Type[] typeArgs = new Type[paramCount + 1];
                for (int i = 0; i < paramCount; i++)
                {
                    typeArgs[i] = parameters[i].Type;
                }
                typeArgs[paramCount] = body.Type;

                return Expression.Lambda(GetFuncType(typeArgs), body, parameters);
            }

            public static Type GetFuncType(params Type[] typeArgs)
            {
                Contract.Assert(typeArgs != null && typeArgs.Length >= 1 && typeArgs.Length <= 5);

                return funcTypes[typeArgs.Length - 1].MakeGenericType(typeArgs);
            }
        }

        internal class DynamicOrdering
        {
            public Expression Selector;
            public bool Ascending;
        }

        internal class ExpressionParser
        {
            struct Token
            {
                public TokenId id;
                public string text;
                public int pos;
            }

            enum TokenId
            {
                Unknown,
                End,
                Identifier,
                StringLiteral,
                IntegerLiteral,
                RealLiteral,
                Not,
                Modulo,
                OpenParen,
                CloseParen,
                Multiply,
                Add,
                Subtract,
                Comma,
                Minus,
                Dot,
                Divide,
                LessThan,
                GreaterThan,
                NotEqual,
                And,
                LessThanEqual,
                Equal,
                GreaterThanEqual,
                Or
            }

            internal class MappedMemberInfo
            {
                public MappedMemberInfo(Type mappedType, string memberName, bool isStatic, bool isMethod)
                {
                    MappedType = mappedType;
                    MemberName = memberName;
                    IsStatic = isStatic;
                    IsMethod = isMethod;
                }

                public Type MappedType { get; private set; }
                public string MemberName { get; private set; }
                public bool IsStatic { get; private set; }
                public bool IsMethod { get; private set; }
                public Action<Expression[]> MapParams { get; set; }
            }

            interface ILogicalSignatures
            {
                void F(bool x, bool y);
                void F(bool? x, bool? y);
            }

            interface IArithmeticSignatures
            {
                void F(int x, int y);
                void F(uint x, uint y);
                void F(long x, long y);
                void F(ulong x, ulong y);
                void F(float x, float y);
                void F(double x, double y);
                void F(decimal x, decimal y);
                void F(int? x, int? y);
                void F(uint? x, uint? y);
                void F(long? x, long? y);
                void F(ulong? x, ulong? y);
                void F(float? x, float? y);
                void F(double? x, double? y);
                void F(decimal? x, decimal? y);
            }

            interface IRelationalSignatures : IArithmeticSignatures
            {
                void F(string x, string y);
                void F(char x, char y);
                void F(DateTime x, DateTime y);
                void F(TimeSpan x, TimeSpan y);
                void F(char? x, char? y);
                void F(DateTime? x, DateTime? y);
                void F(TimeSpan? x, TimeSpan? y);
                void F(DateTimeOffset x, DateTimeOffset y);
                void F(DateTimeOffset? x, DateTimeOffset? y);
            }

            interface IEqualitySignatures : IRelationalSignatures
            {
                void F(bool x, bool y);
                void F(bool? x, bool? y);
                void F(Guid x, Guid y);
                void F(Guid? x, Guid? y);
            }

            interface IAddSignatures : IArithmeticSignatures
            {
                void F(DateTime x, TimeSpan y);
                void F(TimeSpan x, TimeSpan y);
                void F(DateTime? x, TimeSpan? y);
                void F(TimeSpan? x, TimeSpan? y);
                void F(DateTimeOffset x, TimeSpan y);
                void F(DateTimeOffset? x, TimeSpan? y);
            }

            interface ISubtractSignatures : IAddSignatures
            {
                void F(DateTime x, DateTime y);
                void F(DateTime? x, DateTime? y);
                void F(DateTimeOffset x, DateTimeOffset y);
                void F(DateTimeOffset? x, DateTimeOffset? y);
            }

            interface INegationSignatures
            {
                void F(int x);
                void F(long x);
                void F(float x);
                void F(double x);
                void F(decimal x);
                void F(int? x);
                void F(long? x);
                void F(float? x);
                void F(double? x);
                void F(decimal? x);
            }

            interface INotSignatures
            {
                void F(bool x);
                void F(bool? x);
            }

            static readonly Expression _trueLiteral = Expression.Constant(true);
            static readonly Expression _falseLiteral = Expression.Constant(false);
            static readonly Expression _nullLiteral = Expression.Constant(null);

            static Dictionary<string, object> _keywords;

            Dictionary<string, object> _symbols;
            Dictionary<Expression, string> _literals;
            ParameterExpression _it;
            string _text;
            int _textPos;
            int _textLen;
            char _ch;
            Token _token;
            QueryResolver _queryResolver;

            public ExpressionParser(ParameterExpression[] parameters, string expression, QueryResolver queryResolver)
            {
                if (expression == null)
                {
                    throw new ArgumentNullException("expression");
                }

                if (_keywords == null)
                {
                    _keywords = CreateKeywords();
                }

                this._queryResolver = queryResolver;
                _symbols = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                _literals = new Dictionary<Expression, string>();

                if (parameters != null)
                {
                    ProcessParameters(parameters);
                }

                _text = expression;
                _textLen = _text.Length;
                SetTextPos(0);
                NextToken();
            }

            void ProcessParameters(ParameterExpression[] parameters)
            {
                foreach (ParameterExpression pe in parameters)
                {
                    if (!String.IsNullOrEmpty(pe.Name))
                    {
                        AddSymbol(pe.Name, pe);
                    }
                }

                if (parameters.Length == 1 && String.IsNullOrEmpty(parameters[0].Name))
                {
                    _it = parameters[0];
                }
            }

            void AddSymbol(string name, object value)
            {
                if (_symbols.ContainsKey(name))
                {
                    throw ParseError(Error.Format(SRResources.DuplicateIdentifier, name));
                }

                _symbols.Add(name, value);
            }

            public Expression Parse(Type resultType)
            {
                int exprPos = _token.pos;
                Expression expr = ParseExpression();

                if (resultType != null)
                {
                    if ((expr = PromoteExpression(expr, resultType, exact: true)) == null)
                    {
                        throw ParseError(exprPos, Error.Format(SRResources.ExpressionTypeMismatch, GetTypeName(resultType)));
                    }
                }

                ValidateToken(TokenId.End, SRResources.SyntaxError);
                return expr;
            }

            public IEnumerable<DynamicOrdering> ParseOrdering()
            {
                List<DynamicOrdering> orderings = new List<DynamicOrdering>();

                while (true)
                {
                    Expression expr = ParseExpression();
                    bool ascending = true;

                    if (TokenIdentifierIs("asc"))
                    {
                        NextToken();
                    }
                    else if (TokenIdentifierIs("desc"))
                    {
                        NextToken();
                        ascending = false;
                    }

                    orderings.Add(
                        new DynamicOrdering
                        {
                            Selector = expr,
                            Ascending = ascending
                        });

                    if (_token.id != TokenId.Comma)
                    {
                        break;
                    }

                    NextToken();
                }

                ValidateToken(TokenId.End, SRResources.SyntaxError);
                return orderings;
            }

            Expression ParseExpression()
            {
                Expression expr = ParseLogicalOr();
                return expr;
            }

            // or operator
            Expression ParseLogicalOr()
            {
                Expression left = ParseLogicalAnd();

                while (_token.id == TokenId.Or)
                {
                    Token op = _token;
                    NextToken();
                    Expression right = ParseLogicalAnd();
                    CheckAndPromoteOperands(typeof(ILogicalSignatures), op.text, ref left, ref right, op.pos);
                    left = Expression.OrElse(left, right);
                }

                return left;
            }

            // and operator
            Expression ParseLogicalAnd()
            {
                Expression left = ParseComparison();

                while (_token.id == TokenId.And)
                {
                    Token op = _token;
                    NextToken();
                    Expression right = ParseComparison();
                    CheckAndPromoteOperands(typeof(ILogicalSignatures), op.text, ref left, ref right, op.pos);
                    left = Expression.AndAlso(left, right);
                }

                return left;
            }

            // eq, ne, gt, ge, lt, le operators
            Expression ParseComparison()
            {
                Expression left = ParseAdditive();

                while (
                    _token.id == TokenId.Equal ||
                    _token.id == TokenId.NotEqual ||
                    _token.id == TokenId.GreaterThan ||
                    _token.id == TokenId.GreaterThanEqual ||
                    _token.id == TokenId.LessThan ||
                    _token.id == TokenId.LessThanEqual)
                {
                    Token op = _token;
                    NextToken();
                    Expression right = ParseAdditive();

                    bool isEquality =
                        op.id == TokenId.Equal ||
                        op.id == TokenId.NotEqual;

                    if (isEquality && !left.Type.IsValueType && !right.Type.IsValueType)
                    {
                        if (left.Type != right.Type)
                        {
                            if (left.Type.IsAssignableFrom(right.Type))
                            {
                                right = Expression.Convert(right, left.Type);
                            }
                            else if (right.Type.IsAssignableFrom(left.Type))
                            {
                                left = Expression.Convert(left, right.Type);
                            }
                            else
                            {
                                throw IncompatibleOperandsError(op.text, left, right, op.pos);
                            }
                        }
                    }
                    else if (IsEnumType(left.Type) || IsEnumType(right.Type))
                    {
                        // convert enum expressions to their underlying values for comparison
                        left = ConvertEnumExpression(left, right);
                        right = ConvertEnumExpression(right, left);

                        CheckAndPromoteOperands(isEquality ? typeof(IEqualitySignatures) : typeof(IRelationalSignatures),
                            op.text, ref left, ref right, op.pos);
                    }
                    else
                    {
                        CheckAndPromoteOperands(isEquality ? typeof(IEqualitySignatures) : typeof(IRelationalSignatures),
                            op.text, ref left, ref right, op.pos);
                    }
                    switch (op.id)
                    {
                        case TokenId.Equal:
                            left = GenerateEqual(left, right);
                            break;
                        case TokenId.NotEqual:
                            left = GenerateNotEqual(left, right);
                            break;
                        case TokenId.GreaterThan:
                            left = GenerateGreaterThan(left, right);
                            break;
                        case TokenId.GreaterThanEqual:
                            left = GenerateGreaterThanEqual(left, right);
                            break;
                        case TokenId.LessThan:
                            left = GenerateLessThan(left, right);
                            break;
                        case TokenId.LessThanEqual:
                            left = GenerateLessThanEqual(left, right);
                            break;
                    }
                }
                return left;
            }

            /// <summary>
            /// We perform comparisons against enums using the underlying type
            /// because a more complete set of comparisons can be performed.
            /// </summary>
            static Expression ConvertEnumExpression(Expression expr, Expression otherExpr)
            {
                if (!IsEnumType(expr.Type))
                {
                    return expr;
                }

                Type underlyingType;
                if (IsNullableType(expr.Type) ||
                    (otherExpr.NodeType == ExpressionType.Constant && ((ConstantExpression)otherExpr).Value == null))
                {
                    // if the enum expression itself is nullable or is being compared against null
                    // we use a nullable type
                    underlyingType = typeof(Nullable<>).MakeGenericType(Enum.GetUnderlyingType(GetNonNullableType(expr.Type)));
                }
                else
                {
                    underlyingType = Enum.GetUnderlyingType(expr.Type);
                }

                return Expression.Convert(expr, underlyingType);
            }

            // add, sub operators
            Expression ParseAdditive()
            {
                Expression left = ParseMultiplicative();

                while (
                    _token.id == TokenId.Add ||
                    _token.id == TokenId.Subtract)
                {
                    Token op = _token;
                    NextToken();
                    Expression right = ParseMultiplicative();

                    switch (op.id)
                    {
                        case TokenId.Add:
                            CheckAndPromoteOperands(typeof(IAddSignatures), op.text, ref left, ref right, op.pos);
                            left = GenerateAdd(left, right);
                            break;
                        case TokenId.Subtract:
                            CheckAndPromoteOperands(typeof(ISubtractSignatures), op.text, ref left, ref right, op.pos);
                            left = GenerateSubtract(left, right);
                            break;
                    }
                }
                return left;
            }

            // mul, div, mod operators
            Expression ParseMultiplicative()
            {
                Expression left = ParseUnary();

                while (
                    _token.id == TokenId.Multiply ||
                    _token.id == TokenId.Divide ||
                    _token.id == TokenId.Modulo)
                {
                    Token op = _token;
                    NextToken();
                    Expression right = ParseUnary();

                    CheckAndPromoteOperands(typeof(IArithmeticSignatures), op.text, ref left, ref right, op.pos);

                    switch (op.id)
                    {
                        case TokenId.Multiply:
                            left = Expression.Multiply(left, right);
                            break;
                        case TokenId.Divide:
                            left = Expression.Divide(left, right);
                            break;
                        case TokenId.Modulo:
                            left = Expression.Modulo(left, right);
                            break;
                    }
                }
                return left;
            }

            // -, not unary operators
            Expression ParseUnary()
            {
                if (_token.id == TokenId.Minus || _token.id == TokenId.Not)
                {
                    Token op = _token;
                    NextToken();

                    if (op.id == TokenId.Minus &&
                        (_token.id == TokenId.IntegerLiteral || _token.id == TokenId.RealLiteral))
                    {
                        _token.text = "-" + _token.text;
                        _token.pos = op.pos;
                        return ParsePrimary();
                    }

                    Expression expr = ParseUnary();

                    if (op.id == TokenId.Minus)
                    {
                        CheckAndPromoteOperand(typeof(INegationSignatures), op.text, ref expr, op.pos);
                        expr = Expression.Negate(expr);
                    }
                    else
                    {
                        CheckAndPromoteOperand(typeof(INotSignatures), op.text, ref expr, op.pos);
                        expr = Expression.Not(expr);
                    }

                    return expr;
                }

                return ParsePrimary();
            }

            Expression ParsePrimary()
            {
                Expression expr = ParsePrimaryStart();

                while (true)
                {
                    if (_token.id == TokenId.Dot)
                    {
                        NextToken();
                        expr = ParseMemberAccess(null, expr);
                    }
                    else
                    {
                        break;
                    }
                }
                return expr;
            }

            Expression ParsePrimaryStart()
            {
                switch (_token.id)
                {
                    case TokenId.Identifier:
                        return ParseIdentifier();
                    case TokenId.StringLiteral:
                        return ParseStringLiteral();
                    case TokenId.IntegerLiteral:
                        return ParseIntegerLiteral();
                    case TokenId.RealLiteral:
                        return ParseRealLiteral();
                    case TokenId.OpenParen:
                        return ParseParenExpression();
                    default:
                        throw ParseError(SRResources.ExpressionExpected);
                }
            }

            Expression ParseStringLiteral()
            {
                ValidateToken(TokenId.StringLiteral);

                // Unwrap string (remove surrounding quotes)
                string s = _token.text.Substring(1, _token.text.Length - 2).Replace("''", "'");

                NextToken();
                return CreateLiteral(s, s);
            }

            Expression ParseIntegerLiteral()
            {
                ValidateToken(TokenId.IntegerLiteral);
                string text = _token.text;
                if (text[0] != '-')
                {
                    ulong value;
                    if (!UInt64.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out value))
                    {
                        throw ParseError(Error.Format(SRResources.InvalidIntegerLiteral, text));
                    }

                    NextToken();
                    if (_token.text == "L" || _token.text == "l")
                    {
                        NextToken();
                        return CreateLiteral((long)value, text);
                    }

                    if (value <= (ulong)Int32.MaxValue)
                    {
                        return CreateLiteral((int)value, text);
                    }

                    if (value <= (ulong)UInt32.MaxValue)
                    {
                        return CreateLiteral((uint)value, text);
                    }

                    if (value <= (ulong)Int64.MaxValue)
                    {
                        return CreateLiteral((long)value, text);
                    }

                    return CreateLiteral(value, text);
                }
                else
                {
                    long value;
                    if (!Int64.TryParse(text, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out value))
                    {
                        throw ParseError(Error.Format(SRResources.InvalidIntegerLiteral, text));
                    }

                    NextToken();

                    if (_token.text == "L" || _token.text == "l")
                    {
                        NextToken();
                        return CreateLiteral((long)value, text);
                    }

                    if (value >= Int32.MinValue && value <= Int32.MaxValue)
                    {
                        return CreateLiteral((int)value, text);
                    }

                    return CreateLiteral(value, text);
                }
            }

            Expression ParseRealLiteral()
            {
                ValidateToken(TokenId.RealLiteral);
                string text = _token.text;
                object value = null;
                char last = text[text.Length - 1];
                if (last == 'F' || last == 'f')
                {
                    float f;
                    if (Single.TryParse(text.Substring(0, text.Length - 1), NumberStyles.Number | NumberStyles.AllowExponent, CultureInfo.InvariantCulture, out f))
                    {
                        value = f;
                    }
                }
                else if (last == 'M' || last == 'm')
                {
                    decimal m;
                    if (Decimal.TryParse(text.Substring(0, text.Length - 1), NumberStyles.Number | NumberStyles.AllowExponent, CultureInfo.InvariantCulture, out m))
                    {
                        value = m;
                    }
                }
                else if (last == 'D' || last == 'd')
                {
                    double d;
                    if (Double.TryParse(text.Substring(0, text.Length - 1), NumberStyles.Number | NumberStyles.AllowExponent, CultureInfo.InvariantCulture, out d))
                    {
                        value = d;
                    }
                }
                else
                {
                    double d;
                    if (Double.TryParse(text, NumberStyles.Number | NumberStyles.AllowExponent, CultureInfo.InvariantCulture, out d))
                    {
                        value = d;
                    }
                }

                if (value == null)
                {
                    throw ParseError(Error.Format(SRResources.InvalidRealLiteral, text));
                }

                NextToken();
                return CreateLiteral(value, text);
            }

            Expression CreateLiteral(object value, string valueAsString)
            {
                ConstantExpression expr = Expression.Constant(value);
                _literals.Add(expr, valueAsString);
                return expr;
            }

            Expression ParseParenExpression()
            {
                ValidateToken(TokenId.OpenParen, SRResources.OpenParenExpected);
                NextToken();
                Expression e = ParseExpression();
                ValidateToken(TokenId.CloseParen, SRResources.CloseParenOrOperatorExpected);
                NextToken();
                return e;
            }

            Expression ParseIdentifier()
            {
                ValidateToken(TokenId.Identifier);

                object value;
                if (_keywords.TryGetValue(_token.text, out value))
                {
                    Type constructedType = value as Type;
                    if (constructedType != null)
                    {
                        return ParseTypeConstruction(constructedType);
                    }

                    NextToken();
                    return (Expression)value;
                }

                object symbolValue;
                if (_symbols.TryGetValue(_token.text, out symbolValue))
                {
                    Expression expr = symbolValue as Expression;
                    if (expr == null)
                    {
                        expr = Expression.Constant(symbolValue);
                    }

                    NextToken();
                    return expr;
                }

                if (_it != null)
                {
                    return ParseMemberAccess(null, _it);
                }

                throw ParseError(Error.Format(SRResources.UnknownIdentifier, _token.text));
            }

            static MappedMemberInfo MapFunction(string functionName)
            {
                MappedMemberInfo mappedMember = MapStringFunction(functionName);
                if (mappedMember != null)
                {
                    return mappedMember;
                }

                mappedMember = MapDateFunction(functionName);
                if (mappedMember != null)
                {
                    return mappedMember;
                }

                mappedMember = MapMathFunction(functionName);
                if (mappedMember != null)
                {
                    return mappedMember;
                }

                return null;
            }

            static MappedMemberInfo MapStringFunction(string functionName)
            {
                if (functionName == "startswith")
                {
                    return new MappedMemberInfo(typeof(string), "StartsWith", false, true);
                }
                else if (functionName == "endswith")
                {
                    return new MappedMemberInfo(typeof(string), "EndsWith", false, true);
                }
                else if (functionName == "length")
                {
                    return new MappedMemberInfo(typeof(string), "Length", false, false);
                }
                else if (functionName == "toupper")
                {
                    return new MappedMemberInfo(typeof(string), "ToUpper", false, true);
                }
                else if (functionName == "tolower")
                {
                    return new MappedMemberInfo(typeof(string), "ToLower", false, true);
                }
                else if (functionName == "substringof")
                {
                    MappedMemberInfo memberInfo = new MappedMemberInfo(typeof(string), "Contains", false, true);
                    memberInfo.MapParams = (args) =>
                    {
                        // reverse the order of arguments for string.Contains
                        Expression tmp = args[0];
                        args[0] = args[1];
                        args[1] = tmp;
                    };
                    return memberInfo;
                }
                else if (functionName == "indexof")
                {
                    return new MappedMemberInfo(typeof(string), "IndexOf", false, true);
                }
                else if (functionName == "replace")
                {
                    return new MappedMemberInfo(typeof(string), "Replace", false, true);
                }
                else if (functionName == "substring")
                {
                    return new MappedMemberInfo(typeof(string), "Substring", false, true);
                }
                else if (functionName == "trim")
                {
                    return new MappedMemberInfo(typeof(string), "Trim", false, true);
                }
                else if (functionName == "concat")
                {
                    return new MappedMemberInfo(typeof(string), "Concat", true, true);
                }

                return null;
            }

            static MappedMemberInfo MapDateFunction(string functionName)
            {
                // date functions
                if (functionName == "day")
                {
                    return new MappedMemberInfo(typeof(DateTime), "Day", false, false);
                }
                else if (functionName == "month")
                {
                    return new MappedMemberInfo(typeof(DateTime), "Month", false, false);
                }
                else if (functionName == "year")
                {
                    return new MappedMemberInfo(typeof(DateTime), "Year", false, false);
                }
                else if (functionName == "hour")
                {
                    return new MappedMemberInfo(typeof(DateTime), "Hour", false, false);
                }
                else if (functionName == "minute")
                {
                    return new MappedMemberInfo(typeof(DateTime), "Minute", false, false);
                }
                else if (functionName == "second")
                {
                    return new MappedMemberInfo(typeof(DateTime), "Second", false, false);
                }

                return null;
            }

            static MappedMemberInfo MapMathFunction(string functionName)
            {
                if (functionName == "round")
                {
                    return new MappedMemberInfo(typeof(Math), "Round", true, true);
                }
                if (functionName == "floor")
                {
                    return new MappedMemberInfo(typeof(Math), "Floor", true, true);
                }
                if (functionName == "ceiling")
                {
                    return new MappedMemberInfo(typeof(Math), "Ceiling", true, true);
                }

                return null;
            }

            Expression ParseTypeConstruction(Type type)
            {
                string typeIdentifier = _token.text;
                int errorPos = _token.pos;
                NextToken();
                Expression typeExpression = null;

                if (_token.id == TokenId.StringLiteral)
                {
                    errorPos = _token.pos;
                    Expression stringExpr = ParseStringLiteral();
                    string literalValue = (string)((ConstantExpression)stringExpr).Value;

                    try
                    {
                        if (type == typeof(DateTime))
                        {
                            DateTime dateTime = DateTime.Parse(literalValue, CultureInfo.CurrentCulture);
                            typeExpression = Expression.Constant(dateTime);
                        }
                        else if (type == typeof(Guid))
                        {
                            Guid guid = Guid.Parse(literalValue);
                            typeExpression = Expression.Constant(guid);
                        }
                        else if (type == typeof(DateTimeOffset))
                        {
                            DateTimeOffset dateTimeOffset = DateTimeOffset.Parse(literalValue, CultureInfo.CurrentCulture);
                            typeExpression = Expression.Constant(dateTimeOffset);
                        }
                        else if (type == typeof(byte[]))
                        {
                            if (literalValue.Length % 2 != 0)
                            {
                                // odd hex strings are not supported
                                throw ParseError(errorPos, Error.Format(SRResources.InvalidHexLiteral));
                            }
                            byte[] bytes = new byte[literalValue.Length / 2];
                            for (int i = 0, j = 0; i < literalValue.Length; i += 2, j++)
                            {
                                string hexValue = literalValue.Substring(i, 2);
                                bytes[j] = byte.Parse(hexValue, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                            }
                            typeExpression = Expression.Constant(bytes);
                        }
                        else if (type == typeof(TimeSpan))
                        {
                            TimeSpan timeSpan = TimeSpan.Parse(literalValue, CultureInfo.CurrentCulture);
                            typeExpression = Expression.Constant(timeSpan);
                        }
                    }
                    catch (FormatException ex)
                    {
                        throw ParseError(errorPos, ex.Message);
                    }
                }
                else
                {
                    throw ParseError(errorPos, Error.Format(SRResources.InvalidTypeCreationExpression, typeIdentifier));
                }

                return typeExpression;
            }

            Expression ParseMappedFunction(MappedMemberInfo mappedMember, int errorPos)
            {
                Type type = mappedMember.MappedType;
                string mappedMemberName = mappedMember.MemberName;
                Expression[] args;
                Expression instance = null;

                if (_token.id == TokenId.OpenParen)
                {
                    args = ParseArgumentList();

                    if (mappedMember.MapParams != null)
                    {
                        mappedMember.MapParams(args);
                    }

                    // static methods need to include the target
                    if (!mappedMember.IsStatic)
                    {
                        if (args.Length == 0)
                        {
                            throw ParseError(errorPos, SRResources.NoApplicableMethod, mappedMember.MemberName, mappedMember.MappedType);
                        }

                        instance = args[0];
                        args = args.Skip(1).ToArray();
                    }
                    else
                    {
                        instance = null;
                    }
                }
                else
                {
                    // If it is a function it should begin with a '('
                    throw ParseError(SRResources.OpenParenExpected);
                }

                if (mappedMember.IsMethod)
                {
                    MethodBase mb;

                    switch (FindMethod(type, mappedMemberName, mappedMember.IsStatic, args, out mb))
                    {
                        case 0:
                            throw ParseError(errorPos,
                                Error.Format(SRResources.NoApplicableMethod, mappedMemberName, GetTypeName(type)));
                        case 1:
                            MethodInfo method = (MethodInfo)mb;
                            if (method.ReturnType == typeof(void))
                            {
                                throw ParseError(errorPos,
                                    Error.Format(SRResources.MethodIsVoid, mappedMemberName, GetTypeName(method.DeclaringType)));
                            }

                            return Expression.Call(instance, (MethodInfo)method, args);
                        default:
                            throw ParseError(errorPos,
                                Error.Format(SRResources.AmbiguousMethodInvocation, mappedMemberName, GetTypeName(type)));
                    }
                }
                else
                {
                    // a mapped Property/Field
                    MemberInfo member = FindPropertyOrField(type, mappedMemberName, mappedMember.IsStatic);
                    if (member == null)
                    {
                        if (this._queryResolver != null)
                        {
                            MemberExpression mex = _queryResolver.ResolveMember(type, mappedMemberName, instance);
                            if (mex != null)
                            {
                                return mex;
                            }
                        }
                        throw ParseError(errorPos,
                            Error.Format(SRResources.UnknownPropertyOrField, mappedMemberName, GetTypeName(type)));
                    }

                    return member.MemberType == MemberTypes.Property ?
                        Expression.Property(instance, (PropertyInfo)member) :
                        Expression.Field(instance, (FieldInfo)member);
                }
            }

            Expression ParseMemberAccess(Type type, Expression instance)
            {
                if (instance != null)
                {
                    type = instance.Type;
                }

                int errorPos = _token.pos;
                string id = GetIdentifier();
                NextToken();

                if (_token.id == TokenId.OpenParen)
                {
                    // See if the token is a mapped function call
                    MappedMemberInfo mappedFunction = MapFunction(id);
                    if (mappedFunction != null)
                    {
                        return ParseMappedFunction(mappedFunction, errorPos);
                    }
                    else
                    {
                        throw ParseError(errorPos, Error.Format(SRResources.UnknownIdentifier, id));
                    }
                }
                else
                {
                    MemberInfo member = FindPropertyOrField(type, id, instance == null);
                    if (member == null)
                    {
                        if (this._queryResolver != null)
                        {
                            MemberExpression mex = _queryResolver.ResolveMember(type, id, instance);
                            if (mex != null)
                            {
                                return mex;
                            }
                        }

                        throw ParseError(errorPos,
                            Error.Format(SRResources.UnknownPropertyOrField, id, GetTypeName(type)));
                    }

                    return member.MemberType == MemberTypes.Property ?
                        Expression.Property(instance, (PropertyInfo)member) :
                        Expression.Field(instance, (FieldInfo)member);
                }
            }

            Expression[] ParseArgumentList()
            {
                ValidateToken(TokenId.OpenParen, SRResources.OpenParenExpected);
                NextToken();
                Expression[] args = _token.id != TokenId.CloseParen ? ParseArguments() : new Expression[0];
                ValidateToken(TokenId.CloseParen, SRResources.CloseParenOrCommaExpected);
                NextToken();
                return args;
            }

            Expression[] ParseArguments()
            {
                List<Expression> argList = new List<Expression>();
                while (true)
                {
                    argList.Add(ParseExpression());

                    if (_token.id != TokenId.Comma)
                    {
                        break;
                    }

                    NextToken();
                }
                return argList.ToArray();
            }

            static bool IsNullableType(Type type)
            {
                return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
            }

            static Type GetNonNullableType(Type type)
            {
                return IsNullableType(type) ? type.GetGenericArguments()[0] : type;
            }

            internal static string GetTypeName(Type type)
            {
                Type baseType = GetNonNullableType(type);
                string s = baseType.Name;
                if (type != baseType)
                {
                    s += '?';
                }

                return s;
            }

            static bool IsSignedIntegralType(Type type)
            {
                return GetNumericTypeKind(type) == 2;
            }

            static bool IsUnsignedIntegralType(Type type)
            {
                return GetNumericTypeKind(type) == 3;
            }

            static int GetNumericTypeKind(Type type)
            {
                type = GetNonNullableType(type);
                if (type.IsEnum)
                {
                    return 0;
                }

                switch (Type.GetTypeCode(type))
                {
                    case TypeCode.Char:
                    case TypeCode.Single:
                    case TypeCode.Double:
                    case TypeCode.Decimal:
                        return 1;
                    case TypeCode.SByte:
                    case TypeCode.Int16:
                    case TypeCode.Int32:
                    case TypeCode.Int64:
                        return 2;
                    case TypeCode.Byte:
                    case TypeCode.UInt16:
                    case TypeCode.UInt32:
                    case TypeCode.UInt64:
                        return 3;
                    default:
                        return 0;
                }
            }

            static bool IsEnumType(Type type)
            {
                return GetNonNullableType(type).IsEnum;
            }

            void CheckAndPromoteOperand(Type signatures, string opName, ref Expression expr, int errorPos)
            {
                Expression[] args = new Expression[] { expr };

                MethodBase method;
                if (FindMethod(signatures, "F", false, args, out method) != 1)
                {
                    throw ParseError(errorPos,
                        Error.Format(SRResources.IncompatibleOperand, opName, GetTypeName(args[0].Type)));
                }

                expr = args[0];
            }

            void CheckAndPromoteOperands(Type signatures, string opName, ref Expression left, ref Expression right, int errorPos)
            {
                Expression[] args = new Expression[] { left, right };

                MethodBase method;
                if (FindMethod(signatures, "F", false, args, out method) != 1)
                {
                    throw IncompatibleOperandsError(opName, left, right, errorPos);
                }

                left = args[0];
                right = args[1];
            }

            static Exception IncompatibleOperandsError(string opName, Expression left, Expression right, int pos)
            {
                return ParseError(pos,
                    Error.Format(SRResources.IncompatibleOperands, opName, GetTypeName(left.Type), GetTypeName(right.Type)));
            }

            static MemberInfo FindPropertyOrField(Type type, string memberName, bool staticAccess)
            {
                BindingFlags flags =
                    BindingFlags.Public |
                    BindingFlags.DeclaredOnly |
                    (staticAccess ? BindingFlags.Static : BindingFlags.Instance);

                foreach (Type t in SelfAndBaseTypes(type))
                {
                    MemberInfo[] members = t.FindMembers(MemberTypes.Property | MemberTypes.Field, flags, Type.FilterNameIgnoreCase, memberName);
                    if (members.Length != 0)
                    {
                        return members[0];
                    }
                }

                return null;
            }

            int FindMethod(Type type, string methodName, bool staticAccess, Expression[] args, out MethodBase method)
            {
                BindingFlags flags =
                    BindingFlags.Public |
                    BindingFlags.DeclaredOnly |
                    (staticAccess ? BindingFlags.Static : BindingFlags.Instance);

                foreach (Type t in SelfAndBaseTypes(type))
                {
                    MemberInfo[] members = t.FindMembers(MemberTypes.Method,
                        flags, Type.FilterNameIgnoreCase, methodName);

                    int count = FindBestMethod(members.Cast<MethodBase>(), args, out method);
                    if (count != 0)
                    {
                        return count;
                    }
                }

                method = null;
                return 0;
            }

            static IEnumerable<Type> SelfAndBaseTypes(Type type)
            {
                if (type.IsInterface)
                {
                    List<Type> types = new List<Type>();
                    AddInterface(types, type);
                    return types;
                }

                return SelfAndBaseClasses(type);
            }

            static IEnumerable<Type> SelfAndBaseClasses(Type type)
            {
                while (type != null)
                {
                    yield return type;
                    type = type.BaseType;
                }
            }

            static void AddInterface(List<Type> types, Type type)
            {
                if (!types.Contains(type))
                {
                    types.Add(type);

                    foreach (Type t in type.GetInterfaces())
                    {
                        AddInterface(types, t);
                    }
                }
            }

            class MethodData
            {
                public MethodBase MethodBase;
                public ParameterInfo[] Parameters;
                public Expression[] Args;
            }

            int FindBestMethod(IEnumerable<MethodBase> methods, Expression[] args, out MethodBase method)
            {
                MethodData[] applicable = methods.
                    Select(m => new MethodData
                    {
                        MethodBase = m,
                        Parameters = m.GetParameters()
                    }).
                    Where(m => IsApplicable(m, args)).
                    ToArray();
                if (applicable.Length > 1)
                {
                    applicable = applicable.
                        Where(m => applicable.All(n => m == n || IsBetterThan(args, m, n))).
                        ToArray();
                }
                if (applicable.Length == 1)
                {
                    MethodData md = applicable[0];

                    for (int i = 0; i < args.Length; i++)
                    {
                        args[i] = md.Args[i];
                    }

                    method = md.MethodBase;
                }
                else
                {
                    method = null;
                }
                return applicable.Length;
            }

            bool IsApplicable(MethodData method, Expression[] args)
            {
                if (method.Parameters.Length != args.Length)
                {
                    return false;
                }

                Expression[] promotedArgs = new Expression[args.Length];

                for (int i = 0; i < args.Length; i++)
                {
                    ParameterInfo pi = method.Parameters[i];
                    if (pi.IsOut)
                    {
                        return false;
                    }

                    Expression promoted = PromoteExpression(args[i], pi.ParameterType, false);
                    if (promoted == null)
                    {
                        return false;
                    }

                    promotedArgs[i] = promoted;
                }

                method.Args = promotedArgs;
                return true;
            }

            Expression PromoteExpression(Expression expr, Type type, bool exact)
            {
                if (expr.Type == type)
                {
                    return expr;
                }

                ConstantExpression ce = expr as ConstantExpression;
                if (ce != null)
                {
                    if (ce == _nullLiteral)
                    {
                        if (!type.IsValueType || IsNullableType(type))
                        {
                            return Expression.Constant(null, type);
                        }
                    }
                    else
                    {
                        string text;
                        if (_literals.TryGetValue(ce, out text))
                        {
                            Type target = GetNonNullableType(type);
                            Object value = null;
                            switch (Type.GetTypeCode(ce.Type))
                            {
                                case TypeCode.Int32:
                                case TypeCode.UInt32:
                                case TypeCode.Int64:
                                case TypeCode.UInt64:
                                    if (target.IsEnum)
                                    {
                                        // promoting from a number to an enum
                                        value = Enum.Parse(target, text);
                                    }
                                    else if (target == typeof(char))
                                    {
                                        // promote from a number to a char
                                        value = Convert.ToChar(ce.Value, CultureInfo.InvariantCulture);
                                    }
                                    else
                                    {
                                        value = ParseNumber(text, target);
                                    }
                                    break;
                                case TypeCode.Double:
                                    if (target == typeof(decimal))
                                    {
                                        value = ParseNumber(text, target);
                                    }
                                    break;
                                case TypeCode.String:
                                    value = ParseEnum(text, target);
                                    break;
                            }

                            if (value != null)
                            {
                                return Expression.Constant(value, type);
                            }
                        }
                    }
                }

                if (IsCompatibleWith(expr.Type, type))
                {
                    if (type.IsValueType || exact)
                    {
                        return Expression.Convert(expr, type);
                    }

                    return expr;
                }

                return null;
            }

            static object ParseNumber(string text, Type type)
            {
                switch (Type.GetTypeCode(GetNonNullableType(type)))
                {
                    case TypeCode.SByte:
                        sbyte sb;
                        if (sbyte.TryParse(text, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out sb))
                        {
                            return sb;
                        }
                        break;
                    case TypeCode.Byte:
                        byte b;
                        if (byte.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out b))
                        {
                            return b;
                        }
                        break;
                    case TypeCode.Int16:
                        short s;
                        if (short.TryParse(text, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out s))
                        {
                            return s;
                        }
                        break;
                    case TypeCode.UInt16:
                        ushort us;
                        if (ushort.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out us))
                        {
                            return us;
                        }
                        break;
                    case TypeCode.Int32:
                        int i;
                        if (int.TryParse(text, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out i))
                        {
                            return i;
                        }
                        break;
                    case TypeCode.UInt32:
                        uint ui;
                        if (uint.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out ui))
                        {
                            return ui;
                        }
                        break;
                    case TypeCode.Int64:
                        long l;
                        if (long.TryParse(text, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out l))
                        {
                            return l;
                        }
                        break;
                    case TypeCode.UInt64:
                        ulong ul;
                        if (ulong.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out ul))
                        {
                            return ul;
                        }
                        break;
                    case TypeCode.Single:
                        float f;
                        if (float.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out f))
                        {
                            return f;
                        }
                        break;
                    case TypeCode.Double:
                        double d;
                        if (double.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out d))
                        {
                            return d;
                        }
                        break;
                    case TypeCode.Decimal:
                        decimal e;
                        if (decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out e))
                        {
                            return e;
                        }
                        break;
                }

                return null;
            }

            static object ParseEnum(string name, Type type)
            {
                if (type.IsEnum)
                {
                    MemberInfo[] memberInfos = type.FindMembers(MemberTypes.Field,
                        BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.Static,
                        Type.FilterNameIgnoreCase, name);

                    if (memberInfos.Length != 0)
                    {
                        return ((FieldInfo)memberInfos[0]).GetValue(null);
                    }
                }
                return null;
            }

            [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "Legacy code.")]
            static bool IsCompatibleWith(Type source, Type target)
            {
                if (source == target)
                {
                    return true;
                }

                if (!target.IsValueType)
                {
                    return target.IsAssignableFrom(source);
                }

                Type st = GetNonNullableType(source);
                Type tt = GetNonNullableType(target);

                if (st != source && tt == target)
                {
                    return false;
                }

                TypeCode sc = st.IsEnum ? TypeCode.Object : Type.GetTypeCode(st);
                TypeCode tc = tt.IsEnum ? TypeCode.Object : Type.GetTypeCode(tt);

                switch (sc)
                {
                    case TypeCode.SByte:
                        switch (tc)
                        {
                            case TypeCode.SByte:
                            case TypeCode.Int16:
                            case TypeCode.Int32:
                            case TypeCode.Int64:
                            case TypeCode.Single:
                            case TypeCode.Double:
                            case TypeCode.Decimal:
                                return true;
                        }
                        break;
                    case TypeCode.Byte:
                        switch (tc)
                        {
                            case TypeCode.Byte:
                            case TypeCode.Int16:
                            case TypeCode.UInt16:
                            case TypeCode.Int32:
                            case TypeCode.UInt32:
                            case TypeCode.Int64:
                            case TypeCode.UInt64:
                            case TypeCode.Single:
                            case TypeCode.Double:
                            case TypeCode.Decimal:
                                return true;
                        }
                        break;
                    case TypeCode.Int16:
                        switch (tc)
                        {
                            case TypeCode.Int16:
                            case TypeCode.Int32:
                            case TypeCode.Int64:
                            case TypeCode.Single:
                            case TypeCode.Double:
                            case TypeCode.Decimal:
                                return true;
                        }
                        break;
                    case TypeCode.UInt16:
                        switch (tc)
                        {
                            case TypeCode.UInt16:
                            case TypeCode.Int32:
                            case TypeCode.UInt32:
                            case TypeCode.Int64:
                            case TypeCode.UInt64:
                            case TypeCode.Single:
                            case TypeCode.Double:
                            case TypeCode.Decimal:
                                return true;
                        }
                        break;
                    case TypeCode.Int32:
                        switch (tc)
                        {
                            case TypeCode.Int32:
                            case TypeCode.Int64:
                            case TypeCode.Single:
                            case TypeCode.Double:
                            case TypeCode.Decimal:
                                return true;
                        }
                        break;
                    case TypeCode.UInt32:
                        switch (tc)
                        {
                            case TypeCode.UInt32:
                            case TypeCode.Int64:
                            case TypeCode.UInt64:
                            case TypeCode.Single:
                            case TypeCode.Double:
                            case TypeCode.Decimal:
                                return true;
                        }
                        break;
                    case TypeCode.Int64:
                        switch (tc)
                        {
                            case TypeCode.Int64:
                            case TypeCode.Single:
                            case TypeCode.Double:
                            case TypeCode.Decimal:
                                return true;
                        }
                        break;
                    case TypeCode.UInt64:
                        switch (tc)
                        {
                            case TypeCode.UInt64:
                            case TypeCode.Single:
                            case TypeCode.Double:
                            case TypeCode.Decimal:
                                return true;
                        }
                        break;
                    case TypeCode.Single:
                        switch (tc)
                        {
                            case TypeCode.Single:
                            case TypeCode.Double:
                                return true;
                        }
                        break;
                    default:
                        if (st == tt)
                        {
                            return true;
                        }
                        break;
                }
                return false;
            }

            static bool IsBetterThan(Expression[] args, MethodData m1, MethodData m2)
            {
                bool better = false;

                for (int i = 0; i < args.Length; i++)
                {
                    int c = CompareConversions(args[i].Type,
                        m1.Parameters[i].ParameterType,
                        m2.Parameters[i].ParameterType);

                    if (c < 0)
                    {
                        return false;
                    }

                    if (c > 0)
                    {
                        better = true;
                    }
                }
                return better;
            }

            // Return 1 if s -> t1 is a better conversion than s -> t2
            // Return -1 if s -> t2 is a better conversion than s -> t1
            // Return 0 if neither conversion is better
            static int CompareConversions(Type s, Type t1, Type t2)
            {
                if (t1 == t2)
                {
                    return 0;
                }

                if (s == t1)
                {
                    return 1;
                }

                if (s == t2)
                {
                    return -1;
                }

                bool t1t2 = IsCompatibleWith(t1, t2);
                bool t2t1 = IsCompatibleWith(t2, t1);

                if (t1t2 && !t2t1)
                {
                    return 1;
                }

                if (t2t1 && !t1t2)
                {
                    return -1;
                }

                if (IsSignedIntegralType(t1) && IsUnsignedIntegralType(t2))
                {
                    return 1;
                }

                if (IsSignedIntegralType(t2) && IsUnsignedIntegralType(t1))
                {
                    return -1;
                }

                return 0;
            }

            static Expression GenerateEqual(Expression left, Expression right)
            {
                return Expression.Equal(left, right);
            }

            static Expression GenerateNotEqual(Expression left, Expression right)
            {
                return Expression.NotEqual(left, right);
            }

            static Expression GenerateGreaterThan(Expression left, Expression right)
            {
                if (left.Type == typeof(string))
                {
                    return Expression.GreaterThan(
                        GenerateStaticMethodCall("Compare", left, right),
                        Expression.Constant(0)
                    );
                }
                return Expression.GreaterThan(left, right);
            }

            static Expression GenerateGreaterThanEqual(Expression left, Expression right)
            {
                if (left.Type == typeof(string))
                {
                    return Expression.GreaterThanOrEqual(
                        GenerateStaticMethodCall("Compare", left, right),
                        Expression.Constant(0)
                    );
                }
                return Expression.GreaterThanOrEqual(left, right);
            }

            static Expression GenerateLessThan(Expression left, Expression right)
            {
                if (left.Type == typeof(string))
                {
                    return Expression.LessThan(
                        GenerateStaticMethodCall("Compare", left, right),
                        Expression.Constant(0)
                    );
                }
                return Expression.LessThan(left, right);
            }

            static Expression GenerateLessThanEqual(Expression left, Expression right)
            {
                if (left.Type == typeof(string))
                {
                    return Expression.LessThanOrEqual(
                        GenerateStaticMethodCall("Compare", left, right),
                        Expression.Constant(0)
                    );
                }
                return Expression.LessThanOrEqual(left, right);
            }

            static Expression GenerateAdd(Expression left, Expression right)
            {
                if (left.Type == typeof(string) && right.Type == typeof(string))
                {
                    return GenerateStaticMethodCall("Concat", left, right);
                }
                return Expression.Add(left, right);
            }

            static Expression GenerateSubtract(Expression left, Expression right)
            {
                return Expression.Subtract(left, right);
            }

            static MethodInfo GetStaticMethod(string methodName, Expression left, Expression right)
            {
                return left.Type.GetMethod(methodName, new[] { left.Type, right.Type });
            }

            static Expression GenerateStaticMethodCall(string methodName, Expression left, Expression right)
            {
                return Expression.Call(null, GetStaticMethod(methodName, left, right), new[] { left, right });
            }

            void SetTextPos(int pos)
            {
                _textPos = pos;
                _ch = _textPos < _textLen ? _text[_textPos] : '\0';
            }

            void NextChar()
            {
                if (_textPos < _textLen)
                {
                    _textPos++;
                }

                _ch = _textPos < _textLen ? _text[_textPos] : '\0';
            }

            [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "Legacy code.")]
            void NextToken()
            {
                while (Char.IsWhiteSpace(_ch))
                {
                    NextChar();
                }

                TokenId t;
                int tokenPos = _textPos;

                switch (_ch)
                {
                    case '(':
                        NextChar();
                        t = TokenId.OpenParen;
                        break;
                    case ')':
                        NextChar();
                        t = TokenId.CloseParen;
                        break;
                    case ',':
                        NextChar();
                        t = TokenId.Comma;
                        break;
                    case '-':
                        NextChar();
                        t = TokenId.Minus;
                        break;
                    case '/':
                        NextChar();
                        t = TokenId.Dot;
                        break;
                    case '\'':
                        char quote = _ch;
                        do
                        {
                            NextChar();
                            while (_textPos < _textLen && _ch != quote)
                            {
                                NextChar();
                            }

                            if (_textPos == _textLen)
                            {
                                throw ParseError(_textPos, SRResources.UnterminatedStringLiteral);
                            }

                            NextChar();
                        } while (_ch == quote);
                        t = TokenId.StringLiteral;
                        break;
                    default:
                        if (IsIdentifierStart(_ch) || _ch == '@' || _ch == '_')
                        {
                            do
                            {
                                NextChar();
                            } while (IsIdentifierPart(_ch) || _ch == '_');
                            t = TokenId.Identifier;
                            break;
                        }
                        if (Char.IsDigit(_ch))
                        {
                            t = TokenId.IntegerLiteral;
                            do
                            {
                                NextChar();
                            } while (Char.IsDigit(_ch));
                            if (_ch == '.')
                            {
                                t = TokenId.RealLiteral;
                                NextChar();
                                ValidateDigit();
                                do
                                {
                                    NextChar();
                                } while (Char.IsDigit(_ch));
                            }
                            if (_ch == 'E' || _ch == 'e')
                            {
                                t = TokenId.RealLiteral;
                                NextChar();
                                if (_ch == '+' || _ch == '-')
                                {
                                    NextChar();
                                }
                                ValidateDigit();
                                do
                                {
                                    NextChar();
                                } while (Char.IsDigit(_ch));
                            }
                            if (_ch == 'F' || _ch == 'f' || _ch == 'M' || _ch == 'm' || _ch == 'D' || _ch == 'd')
                            {
                                t = TokenId.RealLiteral;
                                NextChar();
                            }
                            break;
                        }
                        if (_textPos == _textLen)
                        {
                            t = TokenId.End;
                            break;
                        }
                        throw ParseError(_textPos, Error.Format(SRResources.InvalidCharacter, _ch));
                }
                _token.id = t;
                _token.text = _text.Substring(tokenPos, _textPos - tokenPos);
                _token.pos = tokenPos;

                _token.id = ReclassifyToken(_token);
            }

            static TokenId ReclassifyToken(Token token)
            {
                if (token.id == TokenId.Identifier)
                {
                    if (token.text == "or")
                    {
                        return TokenId.Or;
                    }
                    if (token.text == "add")
                    {
                        return TokenId.Add;
                    }
                    else if (token.text == "and")
                    {
                        return TokenId.And;
                    }
                    else if (token.text == "div")
                    {
                        return TokenId.Divide;
                    }
                    else if (token.text == "sub")
                    {
                        return TokenId.Subtract;
                    }
                    else if (token.text == "mul")
                    {
                        return TokenId.Multiply;
                    }
                    else if (token.text == "mod")
                    {
                        return TokenId.Modulo;
                    }
                    else if (token.text == "ne")
                    {
                        return TokenId.NotEqual;
                    }
                    else if (token.text == "not")
                    {
                        return TokenId.Not;
                    }
                    else if (token.text == "le")
                    {
                        return TokenId.LessThanEqual;
                    }
                    else if (token.text == "lt")
                    {
                        return TokenId.LessThan;
                    }
                    else if (token.text == "eq")
                    {
                        return TokenId.Equal;
                    }
                    else if (token.text == "ge")
                    {
                        return TokenId.GreaterThanEqual;
                    }
                    else if (token.text == "gt")
                    {
                        return TokenId.GreaterThan;
                    }
                }

                return token.id;
            }

            static bool IsIdentifierStart(char ch)
            {
                const int mask =
                    1 << (int)UnicodeCategory.UppercaseLetter |
                    1 << (int)UnicodeCategory.LowercaseLetter |
                    1 << (int)UnicodeCategory.TitlecaseLetter |
                    1 << (int)UnicodeCategory.ModifierLetter |
                    1 << (int)UnicodeCategory.OtherLetter |
                    1 << (int)UnicodeCategory.LetterNumber;
                return (1 << (int)Char.GetUnicodeCategory(ch) & mask) != 0;
            }

            static bool IsIdentifierPart(char ch)
            {
                const int mask =
                    1 << (int)UnicodeCategory.UppercaseLetter |
                    1 << (int)UnicodeCategory.LowercaseLetter |
                    1 << (int)UnicodeCategory.TitlecaseLetter |
                    1 << (int)UnicodeCategory.ModifierLetter |
                    1 << (int)UnicodeCategory.OtherLetter |
                    1 << (int)UnicodeCategory.LetterNumber |
                    1 << (int)UnicodeCategory.DecimalDigitNumber |
                    1 << (int)UnicodeCategory.ConnectorPunctuation |
                    1 << (int)UnicodeCategory.NonSpacingMark |
                    1 << (int)UnicodeCategory.SpacingCombiningMark |
                    1 << (int)UnicodeCategory.Format;
                return (1 << (int)Char.GetUnicodeCategory(ch) & mask) != 0;
            }

            bool TokenIdentifierIs(string id)
            {
                return _token.id == TokenId.Identifier && String.Equals(id, _token.text, StringComparison.OrdinalIgnoreCase);
            }

            string GetIdentifier()
            {
                ValidateToken(TokenId.Identifier, SRResources.IdentifierExpected);
                return _token.text;
            }

            void ValidateDigit()
            {
                if (!Char.IsDigit(_ch))
                {
                    throw ParseError(_textPos, SRResources.DigitExpected);
                }
            }

            void ValidateToken(TokenId t, string errorMessage)
            {
                if (_token.id != t)
                {
                    throw ParseError(errorMessage);
                }
            }

            void ValidateToken(TokenId t)
            {
                if (_token.id != t)
                {
                    throw ParseError(SRResources.SyntaxError);
                }
            }

            Exception ParseError(string format, params object[] args)
            {
                return ParseError(_token.pos, format, args);
            }

            static Exception ParseError(int pos, string format, params object[] args)
            {
                return new ParseException(Error.Format(format, args), pos);
            }

            static Dictionary<string, object> CreateKeywords()
            {
                Dictionary<string, object> d = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

                d.Add("true", _trueLiteral);
                d.Add("false", _falseLiteral);
                d.Add("null", _nullLiteral);

                // Type keywords
                d.Add("binary", typeof(byte[]));
                d.Add("X", typeof(byte[]));
                d.Add("time", typeof(TimeSpan));
                d.Add("datetime", typeof(DateTime));
                d.Add("datetimeoffset", typeof(DateTimeOffset));
                d.Add("guid", typeof(Guid));

                return d;
            }
        }
    }
}
