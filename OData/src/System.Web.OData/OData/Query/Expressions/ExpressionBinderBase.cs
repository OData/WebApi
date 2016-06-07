// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Data.Linq;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using System.Web.OData.Formatter;
using System.Web.OData.Properties;
using System.Xml.Linq;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace System.Web.OData.Query.Expressions
{
    internal abstract class ExpressionBinderBase
    {
        protected static readonly MethodInfo StringCompareMethodInfo = typeof(string).GetMethod("Compare", new[] { typeof(string), typeof(string), typeof(StringComparison) });

        protected static readonly Expression NullConstant = Expression.Constant(null);
        protected static readonly Expression FalseConstant = Expression.Constant(false);
        protected static readonly Expression TrueConstant = Expression.Constant(true);
        protected static readonly Expression ZeroConstant = Expression.Constant(0);
        protected static readonly Expression OrdinalStringComparisonConstant = Expression.Constant(StringComparison.Ordinal);

        protected static readonly MethodInfo EnumTryParseMethod = typeof(Enum).GetMethods()
                        .Single(m => m.Name == "TryParse" && m.GetParameters().Length == 2);

        protected static Dictionary<BinaryOperatorKind, ExpressionType> _binaryOperatorMapping = new Dictionary<BinaryOperatorKind, ExpressionType>
        {
            { BinaryOperatorKind.Add, ExpressionType.Add },
            { BinaryOperatorKind.And, ExpressionType.AndAlso },
            { BinaryOperatorKind.Divide, ExpressionType.Divide },
            { BinaryOperatorKind.Equal, ExpressionType.Equal },
            { BinaryOperatorKind.GreaterThan, ExpressionType.GreaterThan },
            { BinaryOperatorKind.GreaterThanOrEqual, ExpressionType.GreaterThanOrEqual },
            { BinaryOperatorKind.LessThan, ExpressionType.LessThan },
            { BinaryOperatorKind.LessThanOrEqual, ExpressionType.LessThanOrEqual },
            { BinaryOperatorKind.Modulo, ExpressionType.Modulo },
            { BinaryOperatorKind.Multiply, ExpressionType.Multiply },
            { BinaryOperatorKind.NotEqual, ExpressionType.NotEqual },
            { BinaryOperatorKind.Or, ExpressionType.OrElse },
            { BinaryOperatorKind.Subtract, ExpressionType.Subtract },
        };

        protected IEdmModel _model;

        protected ODataQuerySettings _querySettings;
        protected IAssembliesResolver _assembliesResolver;

        protected ExpressionBinderBase(IEdmModel model, IAssembliesResolver assembliesResolver, ODataQuerySettings querySettings)
            : this(model, querySettings)
        {
            _assembliesResolver = assembliesResolver;
        }

        protected ExpressionBinderBase(IEdmModel model, ODataQuerySettings querySettings)
        {
            Contract.Assert(model != null);
            Contract.Assert(querySettings != null);
            Contract.Assert(querySettings.HandleNullPropagation != HandleNullPropagationOption.Default);

            _querySettings = querySettings;
            _model = model;
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "These are simple conversion function and cannot be split up.")]
        protected Expression CreateBinaryExpression(BinaryOperatorKind binaryOperator, Expression left, Expression right, bool liftToNull)
        {
            ExpressionType binaryExpressionType;

            // When comparing an enum to a string, parse the string, convert both to the enum underlying type, and compare the values
            // When comparing an enum to an enum with the same type, convert both to the underlying type, and compare the values
            Type leftUnderlyingType = Nullable.GetUnderlyingType(left.Type) ?? left.Type;
            Type rightUnderlyingType = Nullable.GetUnderlyingType(right.Type) ?? right.Type;

            // Convert to integers unless Enum type is required
            if ((leftUnderlyingType.IsEnum || rightUnderlyingType.IsEnum) && binaryOperator != BinaryOperatorKind.Has)
            {
                Type enumType = leftUnderlyingType.IsEnum ? leftUnderlyingType : rightUnderlyingType;
                Type enumUnderlyingType = Enum.GetUnderlyingType(enumType);
                left = ConvertToEnumUnderlyingType(left, enumType, enumUnderlyingType);
                right = ConvertToEnumUnderlyingType(right, enumType, enumUnderlyingType);
            }
           
            if (leftUnderlyingType == typeof(DateTime) && rightUnderlyingType == typeof(DateTimeOffset))
            {
                right = DateTimeOffsetToDateTime(right);
            }
            else if (rightUnderlyingType == typeof(DateTime) && leftUnderlyingType == typeof(DateTimeOffset))
            {
                left = DateTimeOffsetToDateTime(left);
            }

            if ((IsDateOrOffset(leftUnderlyingType) && IsDate(rightUnderlyingType)) ||
                (IsDate(leftUnderlyingType) && IsDateOrOffset(rightUnderlyingType)))
            {
                left = CreateDateBinaryExpression(left);
                right = CreateDateBinaryExpression(right);
            }

            if ((IsDateOrOffset(leftUnderlyingType) && IsTimeOfDay(rightUnderlyingType)) ||
                (IsTimeOfDay(leftUnderlyingType) && IsDateOrOffset(rightUnderlyingType)) ||
                (IsTimeSpan(leftUnderlyingType) && IsTimeOfDay(rightUnderlyingType)) ||
                (IsTimeOfDay(leftUnderlyingType) && IsTimeSpan(rightUnderlyingType)))
            {
                left = CreateTimeBinaryExpression(left);
                right = CreateTimeBinaryExpression(right);
            }

            if (left.Type != right.Type)
            {
                // one of them must be nullable and the other is not.
                left = ToNullable(left);
                right = ToNullable(right);
            }

            if (left.Type == typeof(string) || right.Type == typeof(string))
            {
                // convert nulls of type object to nulls of type string to make the String.Compare call work
                left = ConvertNull(left, typeof(string));
                right = ConvertNull(right, typeof(string));

                // Use string.Compare instead of comparison for gt, ge, lt, le between two strings since direct comparisons are not supported
                switch (binaryOperator)
                {
                    case BinaryOperatorKind.GreaterThan:
                    case BinaryOperatorKind.GreaterThanOrEqual:
                    case BinaryOperatorKind.LessThan:
                    case BinaryOperatorKind.LessThanOrEqual:
                        left = Expression.Call(StringCompareMethodInfo, left, right, OrdinalStringComparisonConstant);
                        right = ZeroConstant;
                        break;
                    default:
                        break;
                }
            }

            if (_binaryOperatorMapping.TryGetValue(binaryOperator, out binaryExpressionType))
            {
                if (left.Type == typeof(byte[]) || right.Type == typeof(byte[]))
                {
                    left = ConvertNull(left, typeof(byte[]));
                    right = ConvertNull(right, typeof(byte[]));

                    switch (binaryExpressionType)
                    {
                        case ExpressionType.Equal:
                            return Expression.MakeBinary(binaryExpressionType, left, right, liftToNull, method: Linq2ObjectsComparisonMethods.AreByteArraysEqualMethodInfo);
                        case ExpressionType.NotEqual:
                            return Expression.MakeBinary(binaryExpressionType, left, right, liftToNull, method: Linq2ObjectsComparisonMethods.AreByteArraysNotEqualMethodInfo);
                        default:
                            IEdmPrimitiveType binaryType = EdmLibHelpers.GetEdmPrimitiveTypeOrNull(typeof(byte[]));
                            throw new ODataException(Error.Format(SRResources.BinaryOperatorNotSupported, binaryType.FullName(), binaryType.FullName(), binaryOperator));
                    }
                }
                else
                {
                    return Expression.MakeBinary(binaryExpressionType, left, right, liftToNull, method: null);
                }
            }
            else
            {
                // Enum has a "has" operator
                // {(c1, c2) => c1.HasFlag(Convert(c2))}
                if (TypeHelper.IsEnum(left.Type) && TypeHelper.IsEnum(right.Type) && binaryOperator == BinaryOperatorKind.Has)
                {
                    UnaryExpression flag = Expression.Convert(right, typeof(Enum));
                    return BindHas(left, flag);
                }
                else
                {
                    throw Error.NotSupported(SRResources.QueryNodeBindingNotSupported, binaryOperator, typeof(FilterBinder).Name);
                }
            }
        }

        protected Expression CreateConvertExpression(ConvertNode convertNode, Expression source)
        {
            Type conversionType = EdmLibHelpers.GetClrType(convertNode.TypeReference, _model, _assembliesResolver);

            if (conversionType == typeof(bool?) && source.Type == typeof(bool))
            {
                // we handle null propagation ourselves. So, if converting from bool to Nullable<bool> ignore.
                return source;
            }
            else if (conversionType == typeof(Date?) &&
                (source.Type == typeof(DateTimeOffset?) || source.Type == typeof(DateTime?)))
            {
                return source;
            }
            if ((conversionType == typeof(TimeOfDay?) && source.Type == typeof(TimeOfDay)) ||
                ((conversionType == typeof(Date?) && source.Type == typeof(Date))))
            {
                return source;
            }
            else if (conversionType == typeof(TimeOfDay?) &&
                (source.Type == typeof(DateTimeOffset?) || source.Type == typeof(DateTime?) || source.Type == typeof(TimeSpan?)))
            {
                return source;
            }
            else if (source == NullConstant)
            {
                return source;
            }
            else
            {
                if (TypeHelper.IsEnum(source.Type))
                {
                    // we handle enum conversions ourselves
                    return source;
                }
                else
                {
                    // if a cast is from Nullable<T> to Non-Nullable<T> we need to check if source is null
                    if (_querySettings.HandleNullPropagation == HandleNullPropagationOption.True
                        && IsNullable(source.Type) && !IsNullable(conversionType))
                    {
                        // source == null ? null : source.Value
                        return
                            Expression.Condition(
                            test: CheckForNull(source),
                            ifTrue: Expression.Constant(null, ToNullable(conversionType)),
                            ifFalse: Expression.Convert(ExtractValueFromNullableExpression(source), ToNullable(conversionType)));
                    }
                    else
                    {
                        return Expression.Convert(source, conversionType);
                    }
                }
            }
        }

        // If the expression is of non-standard edm primitive type (like uint), convert the expression to its standard edm type.
        // Also, note that only expressions generated for ushort, uint and ulong can be understood by linq2sql and EF.
        // The rest (char, char[], Binary) would cause issues with linq2sql and EF.
        protected Expression ConvertNonStandardPrimitives(Expression source)
        {
            bool isNonstandardEdmPrimitive;
            Type conversionType = EdmLibHelpers.IsNonstandardEdmPrimitive(source.Type, out isNonstandardEdmPrimitive);

            if (isNonstandardEdmPrimitive)
            {
                Type sourceType = TypeHelper.GetUnderlyingTypeOrSelf(source.Type);

                Contract.Assert(sourceType != conversionType);

                Expression convertedExpression = null;

                if (sourceType.IsEnum)
                {
                    // we handle enum conversions ourselves
                    convertedExpression = source;
                }
                else
                {
                    switch (Type.GetTypeCode(sourceType))
                    {
                        case TypeCode.UInt16:
                        case TypeCode.UInt32:
                        case TypeCode.UInt64:
                            convertedExpression = Expression.Convert(ExtractValueFromNullableExpression(source), conversionType);
                            break;

                        case TypeCode.Char:
                            convertedExpression = Expression.Call(ExtractValueFromNullableExpression(source), "ToString", typeArguments: null, arguments: null);
                            break;

                        case TypeCode.DateTime:
                            convertedExpression = source;
                            break;

                        case TypeCode.Object:
                            if (sourceType == typeof(char[]))
                            {
                                convertedExpression = Expression.New(typeof(string).GetConstructor(new[] { typeof(char[]) }), source);
                            }
                            else if (sourceType == typeof(XElement))
                            {
                                convertedExpression = Expression.Call(source, "ToString", typeArguments: null, arguments: null);
                            }
                            else if (sourceType == typeof(Binary))
                            {
                                convertedExpression = Expression.Call(source, "ToArray", typeArguments: null, arguments: null);
                            }
                            break;

                        default:
                            Contract.Assert(false, Error.Format("missing non-standard type support for {0}", sourceType.Name));
                            break;
                    }
                }

                if (_querySettings.HandleNullPropagation == HandleNullPropagationOption.True && IsNullable(source.Type))
                {
                    // source == null ? null : source
                    return Expression.Condition(
                        CheckForNull(source),
                        ifTrue: Expression.Constant(null, ToNullable(convertedExpression.Type)),
                        ifFalse: ToNullable(convertedExpression));
                }
                else
                {
                    return convertedExpression;
                }
            }

            return source;
        }

        protected Expression MakePropertyAccess(PropertyInfo propertyInfo, Expression argument)
        {
            Expression propertyArgument = argument;
            if (_querySettings.HandleNullPropagation == HandleNullPropagationOption.True)
            {
                // we don't have to check if the argument is null inside the function call as we do it already
                // before calling the function. So remove the redundant null checks.
                propertyArgument = RemoveInnerNullPropagation(argument);
            }

            // if the argument is of type Nullable<T>, then translate the argument to Nullable<T>.Value as none 
            // of the canonical functions have overloads for Nullable<> arguments.
            propertyArgument = ExtractValueFromNullableExpression(propertyArgument);

            return Expression.Property(propertyArgument, propertyInfo);
        }

        // creates an expression for the corresponding OData function.
        protected Expression MakeFunctionCall(MemberInfo member, params Expression[] arguments)
        {
            Contract.Assert(member.MemberType == MemberTypes.Property || member.MemberType == MemberTypes.Method);

            IEnumerable<Expression> functionCallArguments = arguments;
            if (_querySettings.HandleNullPropagation == HandleNullPropagationOption.True)
            {
                // we don't have to check if the argument is null inside the function call as we do it already
                // before calling the function. So remove the redundant null checks.
                functionCallArguments = arguments.Select(a => RemoveInnerNullPropagation(a));
            }

            // if the argument is of type Nullable<T>, then translate the argument to Nullable<T>.Value as none 
            // of the canonical functions have overloads for Nullable<> arguments.
            functionCallArguments = ExtractValueFromNullableArguments(functionCallArguments);

            Expression functionCall;
            if (member.MemberType == MemberTypes.Method)
            {
                MethodInfo method = member as MethodInfo;
                if (method.IsStatic)
                {
                    functionCall = Expression.Call(null, method, functionCallArguments);
                }
                else
                {
                    functionCall = Expression.Call(functionCallArguments.First(), method, functionCallArguments.Skip(1));
                }
            }
            else
            {
                // property
                functionCall = Expression.Property(functionCallArguments.First(), member as PropertyInfo);
            }

            return CreateFunctionCallWithNullPropagation(functionCall, arguments);
        }

        protected Expression CreateFunctionCallWithNullPropagation(Expression functionCall, Expression[] arguments)
        {
            if (_querySettings.HandleNullPropagation == HandleNullPropagationOption.True)
            {
                Expression test = CheckIfArgumentsAreNull(arguments);

                if (test == FalseConstant)
                {
                    // none of the arguments are/can be null.
                    // so no need to do any null propagation
                    return functionCall;
                }
                else
                {
                    // if one of the arguments is null, result is null (not defined)
                    return
                        Expression.Condition(
                        test: test,
                        ifTrue: Expression.Constant(null, ToNullable(functionCall.Type)),
                        ifFalse: ToNullable(functionCall));
                }
            }
            else
            {
                return functionCall;
            }
        }

        // we don't have to do null checks inside the function for arguments as we do the null checks before calling
        // the function when null propagation is enabled.
        // this method converts back "arg == null ? null : convert(arg)" to "arg" 
        // Also, note that we can do this generically only because none of the odata functions that we support can take null 
        // as an argument.
        protected Expression RemoveInnerNullPropagation(Expression expression)
        {
            Contract.Assert(expression != null);

            if (_querySettings.HandleNullPropagation == HandleNullPropagationOption.True)
            {
                // only null propagation generates conditional expressions
                if (expression.NodeType == ExpressionType.Conditional)
                {
                    // make sure to skip the DateTime IFF clause
                    ConditionalExpression conditionalExpr = (ConditionalExpression)expression;
                    if (conditionalExpr.Test.NodeType != ExpressionType.OrElse)
                    {
                        expression = conditionalExpr.IfFalse;
                        Contract.Assert(expression != null);

                        if (expression.NodeType == ExpressionType.Convert)
                        {
                            UnaryExpression unaryExpression = expression as UnaryExpression;
                            Contract.Assert(unaryExpression != null);

                            if (Nullable.GetUnderlyingType(unaryExpression.Type) == unaryExpression.Operand.Type)
                            {
                                // this is a cast from T to Nullable<T> which is redundant.
                                expression = unaryExpression.Operand;
                            }
                        }
                    }
                }
            }

            return expression;
        }

        private static Expression CheckIfArgumentsAreNull(Expression[] arguments)
        {
            if (arguments.Any(arg => arg == NullConstant))
            {
                return TrueConstant;
            }

            arguments =
                arguments
                .Select(arg => CheckForNull(arg))
                .Where(arg => arg != null)
                .ToArray();

            if (arguments.Any())
            {
                return arguments
                    .Aggregate((left, right) => Expression.OrElse(left, right));
            }
            else
            {
                return FalseConstant;
            }
        }

        protected static Expression CheckForNull(Expression expression)
        {
            if (IsNullable(expression.Type) && expression.NodeType != ExpressionType.Constant)
            {
                return Expression.Equal(expression, Expression.Constant(null));
            }
            else
            {
                return null;
            }
        }

        private static IEnumerable<Expression> ExtractValueFromNullableArguments(IEnumerable<Expression> arguments)
        {
            return arguments.Select(arg => ExtractValueFromNullableExpression(arg));
        }

        protected static Expression ExtractValueFromNullableExpression(Expression source)
        {
            return Nullable.GetUnderlyingType(source.Type) != null ? Expression.Property(source, "Value") : source;
        }

        protected Expression BindHas(Expression left, Expression flag)
        {
            Contract.Assert(TypeHelper.IsEnum(left.Type));
            Contract.Assert(flag.Type == typeof(Enum));

            Expression[] arguments = new[] { left, flag };
            return MakeFunctionCall(ClrCanonicalFunctions.HasFlag, arguments);
        }

        private Expression GetProperty(Expression source, string propertyName)
        {
            if (IsDateOrOffset(source.Type))
            {
                if (IsDateTime(source.Type))
                {
                    return MakePropertyAccess(ClrCanonicalFunctions.DateTimeProperties[propertyName], source);
                }
                else
                {
                    return MakePropertyAccess(ClrCanonicalFunctions.DateTimeOffsetProperties[propertyName], source);
                }
            }
            else if (IsDate(source.Type))
            {
                return MakePropertyAccess(ClrCanonicalFunctions.DateProperties[propertyName], source);
            }
            else if (IsTimeOfDay(source.Type))
            {
                return MakePropertyAccess(ClrCanonicalFunctions.TimeOfDayProperties[propertyName], source);
            }
            else if (IsTimeSpan(source.Type))
            {
                return MakePropertyAccess(ClrCanonicalFunctions.TimeSpanProperties[propertyName], source);
            }

            return source;
        }

        private Expression CreateDateBinaryExpression(Expression source)
        {
            source = ConvertToDateTimeRelatedConstExpression(source);

            // Year, Month, Day
            Expression year = GetProperty(source, ClrCanonicalFunctions.YearFunctionName);
            Expression month = GetProperty(source, ClrCanonicalFunctions.MonthFunctionName);
            Expression day = GetProperty(source, ClrCanonicalFunctions.DayFunctionName);

            // return (year * 10000 + month * 100 + day)
            Expression result =
                Expression.Add(
                    Expression.Add(Expression.Multiply(year, Expression.Constant(10000)),
                        Expression.Multiply(month, Expression.Constant(100))), day);

            return CreateFunctionCallWithNullPropagation(result, new[] { source });
        }

        private Expression CreateTimeBinaryExpression(Expression source)
        {
            source = ConvertToDateTimeRelatedConstExpression(source);

            // Hour, Minute, Second, Millisecond
            Expression hour = GetProperty(source, ClrCanonicalFunctions.HourFunctionName);
            Expression minute = GetProperty(source, ClrCanonicalFunctions.MinuteFunctionName);
            Expression second = GetProperty(source, ClrCanonicalFunctions.SecondFunctionName);
            Expression milliSecond = GetProperty(source, ClrCanonicalFunctions.MillisecondFunctionName);

            Expression hourTicks = Expression.Multiply(Expression.Convert(hour, typeof(long)), Expression.Constant(TimeOfDay.TicksPerHour));
            Expression minuteTicks = Expression.Multiply(Expression.Convert(minute, typeof(long)), Expression.Constant(TimeOfDay.TicksPerMinute));
            Expression secondTicks = Expression.Multiply(Expression.Convert(second, typeof(long)), Expression.Constant(TimeOfDay.TicksPerSecond));

            // return (hour * TicksPerHour + minute * TicksPerMinute + second * TicksPerSecond + millisecond)
            Expression result = Expression.Add(hourTicks, Expression.Add(minuteTicks, Expression.Add(secondTicks, Expression.Convert(milliSecond, typeof(long)))));

            return CreateFunctionCallWithNullPropagation(result, new[] { source });
        }

        private static Expression ConvertToDateTimeRelatedConstExpression(Expression source)
        {
            var parameterizedConstantValue = ExtractParameterizedConstant(source);
            if (parameterizedConstantValue != null && source.Type.IsNullable())
            {
                var dateTimeOffset = parameterizedConstantValue as DateTimeOffset?;
                if (dateTimeOffset != null)
                {
                    return Expression.Constant(dateTimeOffset.Value, typeof(DateTimeOffset));
                }

                var dateTime = parameterizedConstantValue as DateTime?;
                if (dateTime != null)
                {
                    return Expression.Constant(dateTime.Value, typeof(DateTime));
                }

                var date = parameterizedConstantValue as Date?;
                if (date != null)
                {
                    return Expression.Constant(date.Value, typeof(Date));
                }

                var timeOfDay = parameterizedConstantValue as TimeOfDay?;
                if (timeOfDay != null)
                {
                    return Expression.Constant(timeOfDay.Value, typeof(TimeOfDay));
                }
            }

            return source;
        }

        protected static Expression ConvertToEnumUnderlyingType(Expression expression, Type enumType, Type enumUnderlyingType)
        {
            object parameterizedConstantValue = ExtractParameterizedConstant(expression);
            if (parameterizedConstantValue != null)
            {
                string enumStringValue = parameterizedConstantValue as string;
                if (enumStringValue != null)
                {
                    return Expression.Constant(
                        Convert.ChangeType(
                            Enum.Parse(enumType, enumStringValue), enumUnderlyingType, CultureInfo.InvariantCulture));
                }
                else
                {
                    // enum member value
                    return Expression.Constant(
                        Convert.ChangeType(
                            parameterizedConstantValue, enumUnderlyingType, CultureInfo.InvariantCulture));
                }
            }
            else if (expression.Type == enumType)
            {
                return Expression.Convert(expression, enumUnderlyingType);
            }
            else if (Nullable.GetUnderlyingType(expression.Type) == enumType)
            {
                return Expression.Convert(expression, typeof(Nullable<>).MakeGenericType(enumUnderlyingType));
            }
            else if (expression.NodeType == ExpressionType.Constant && ((ConstantExpression)expression).Value == null)
            {
                return expression;
            }
            else
            {
                throw Error.NotSupported(SRResources.ConvertToEnumFailed, enumType, expression.Type);
            }
        }

        // Extract the constant that would have been encapsulated into LinqParameterContainer if this
        // expression represents it else return null.
        protected static object ExtractParameterizedConstant(Expression expression)
        {
            if (expression.NodeType == ExpressionType.MemberAccess)
            {
                MemberExpression memberAccess = expression as MemberExpression;
                Contract.Assert(memberAccess != null);
                if (memberAccess.Expression.NodeType == ExpressionType.Constant)
                {
                    ConstantExpression constant = memberAccess.Expression as ConstantExpression;
                    Contract.Assert(constant != null);
                    Contract.Assert(constant.Value != null);
                    LinqParameterContainer value = constant.Value as LinqParameterContainer;
                    Contract.Assert(value != null, "Constants are already embedded into LinqParameterContainer");

                    return value.Property;
                }
            }

            return null;
        }

        protected static Expression DateTimeOffsetToDateTime(Expression expression)
        {
            var unaryExpression = expression as UnaryExpression;
            if (unaryExpression != null)
            {
                if (Nullable.GetUnderlyingType(unaryExpression.Type) == unaryExpression.Operand.Type)
                {
                    // this is a cast from T to Nullable<T> which is redundant.
                    expression = unaryExpression.Operand;
                }
            }
            var parameterizedConstantValue = ExtractParameterizedConstant(expression);
            var dto = parameterizedConstantValue as DateTimeOffset?;
            if (dto != null)
            {
                expression = Expression.Constant(EdmPrimitiveHelpers.ConvertPrimitiveValue(dto.Value, typeof(DateTime)));
            }
            return expression;
        }

        protected static bool IsNullable(Type t)
        {
            if (!t.IsValueType || (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>)))
            {
                return true;
            }

            return false;
        }

        protected static Type ToNullable(Type t)
        {
            if (IsNullable(t))
            {
                return t;
            }
            else
            {
                return typeof(Nullable<>).MakeGenericType(t);
            }
        }

        protected static Expression ToNullable(Expression expression)
        {
            if (!IsNullable(expression.Type))
            {
                return Expression.Convert(expression, ToNullable(expression.Type));
            }

            return expression;
        }

        protected static bool IsIQueryable(Type type)
        {
            return typeof(IQueryable).IsAssignableFrom(type);
        }

        protected static bool IsDoubleOrDecimal(Type type)
        {
            return IsType<double>(type) || IsType<decimal>(type);
        }

        protected static bool IsDateRelated(Type type)
        {
            return IsType<Date>(type) || IsType<DateTime>(type) || IsType<DateTimeOffset>(type);
        }

        protected static bool IsTimeRelated(Type type)
        {
            return IsType<TimeOfDay>(type) || IsType<DateTime>(type) || IsType<DateTimeOffset>(type) || IsType<TimeSpan>(type);
        }

        protected static bool IsDateOrOffset(Type type)
        {
            return IsType<DateTime>(type) || IsType<DateTimeOffset>(type);
        }

        protected static bool IsDateTime(Type type)
        {
            return IsType<DateTime>(type);
        }

        protected static bool IsTimeSpan(Type type)
        {
            return IsType<TimeSpan>(type);
        }

        protected static bool IsTimeOfDay(Type type)
        {
            return IsType<TimeOfDay>(type);
        }

        protected static bool IsDate(Type type)
        {
            return IsType<Date>(type);
        }

        protected static bool IsInteger(Type type)
        {
            return IsType<short>(type) || IsType<int>(type) || IsType<long>(type);
        }

        protected static bool IsType<T>(Type type) where T : struct
        {
            return type == typeof(T) || type == typeof(T?);
        }

        protected static Expression ConvertNull(Expression expression, Type type)
        {
            ConstantExpression constantExpression = expression as ConstantExpression;
            if (constantExpression != null && constantExpression.Value == null)
            {
                return Expression.Constant(null, type);
            }
            else
            {
                return expression;
            }
        }
    }
}
