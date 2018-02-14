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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace System.Web.OData.Query.Expressions
{
    /// <summary>
    /// The base class for all expression binders.
    /// </summary>
    [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Relies on many ODataLib classes.")]
    public abstract class ExpressionBinderBase
    {
        internal static readonly MethodInfo StringCompareMethodInfo = typeof(string).GetMethod("Compare", new[] { typeof(string), typeof(string), typeof(StringComparison) });
        internal static readonly string DictionaryStringObjectIndexerName = typeof(Dictionary<string, object>).GetDefaultMembers()[0].Name;

        internal static readonly Expression NullConstant = Expression.Constant(null);
        internal static readonly Expression FalseConstant = Expression.Constant(false);
        internal static readonly Expression TrueConstant = Expression.Constant(true);
        internal static readonly Expression ZeroConstant = Expression.Constant(0);
        internal static readonly Expression OrdinalStringComparisonConstant = Expression.Constant(StringComparison.Ordinal);

        internal static readonly MethodInfo EnumTryParseMethod = typeof(Enum).GetMethods()
                        .Single(m => m.Name == "TryParse" && m.GetParameters().Length == 2);

        internal static readonly Dictionary<BinaryOperatorKind, ExpressionType> BinaryOperatorMapping = new Dictionary<BinaryOperatorKind, ExpressionType>
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

        internal IEdmModel Model { get; set; }

        internal ODataQuerySettings QuerySettings { get; set; }

        internal IAssembliesResolver AssembliesResolver { get; set; }

        /// <summary>
        /// Base query used for the binder.
        /// </summary>
        internal IQueryable BaseQuery;

        /// <summary>
        /// Flattened list of properties from base query, for case when binder is applied for aggregated query.
        /// </summary>
        internal IDictionary<string, Expression> FlattenedPropertyContainer;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpressionBinderBase"/> class.
        /// </summary>
        /// <param name="requestContainer">The request container.</param>
        protected ExpressionBinderBase(IServiceProvider requestContainer)
        {
            Contract.Assert(requestContainer != null);

            QuerySettings = requestContainer.GetRequiredService<ODataQuerySettings>();
            Model = requestContainer.GetRequiredService<IEdmModel>();
            AssembliesResolver = requestContainer.GetRequiredService<IAssembliesResolver>();
        }

        internal ExpressionBinderBase(IEdmModel model, IAssembliesResolver assembliesResolver, ODataQuerySettings querySettings)
            : this(model, querySettings)
        {
            AssembliesResolver = assembliesResolver;
        }

        internal ExpressionBinderBase(IEdmModel model, ODataQuerySettings querySettings)
        {
            Contract.Assert(model != null);
            Contract.Assert(querySettings != null);
            Contract.Assert(querySettings.HandleNullPropagation != HandleNullPropagationOption.Default);

            QuerySettings = querySettings;
            Model = model;
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "These are simple conversion function and cannot be split up.")]
        internal Expression CreateBinaryExpression(BinaryOperatorKind binaryOperator, Expression left, Expression right, bool liftToNull)
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

            if (BinaryOperatorMapping.TryGetValue(binaryOperator, out binaryExpressionType))
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

        internal Expression CreateConvertExpression(ConvertNode convertNode, Expression source)
        {
            Type conversionType = EdmLibHelpers.GetClrType(convertNode.TypeReference, Model, AssembliesResolver);

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
                    if (QuerySettings.HandleNullPropagation == HandleNullPropagationOption.True
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
        internal Expression ConvertNonStandardPrimitives(Expression source)
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

                if (QuerySettings.HandleNullPropagation == HandleNullPropagationOption.True && IsNullable(source.Type))
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

        internal Expression MakePropertyAccess(PropertyInfo propertyInfo, Expression argument)
        {
            Expression propertyArgument = argument;
            if (QuerySettings.HandleNullPropagation == HandleNullPropagationOption.True)
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
        internal Expression MakeFunctionCall(MemberInfo member, params Expression[] arguments)
        {
            Contract.Assert(member.MemberType == MemberTypes.Property || member.MemberType == MemberTypes.Method);

            IEnumerable<Expression> functionCallArguments = arguments;
            if (QuerySettings.HandleNullPropagation == HandleNullPropagationOption.True)
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

        internal Expression CreateFunctionCallWithNullPropagation(Expression functionCall, Expression[] arguments)
        {
            if (QuerySettings.HandleNullPropagation == HandleNullPropagationOption.True)
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
        internal Expression RemoveInnerNullPropagation(Expression expression)
        {
            Contract.Assert(expression != null);

            if (QuerySettings.HandleNullPropagation == HandleNullPropagationOption.True)
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

        internal string GetFullPropertyPath(SingleValueNode node)
        {
            string path = null;
            SingleValueNode parent = null;
            switch (node.Kind)
            {
                case QueryNodeKind.SingleComplexNode:
                    path = ((SingleComplexNode)node).Property.Name;
                    break;
                case QueryNodeKind.SingleValuePropertyAccess:
                    var propertyNode = ((SingleValuePropertyAccessNode)node);
                    path = propertyNode.Property.Name;
                    parent = propertyNode.Source;
                    break;
                case QueryNodeKind.SingleNavigationNode:
                    var navNode = ((SingleNavigationNode)node);
                    path = navNode.NavigationProperty.Name;
                    parent = navNode.Source;
                    break;
            }

            if (parent != null)
            {
                var parentPath = GetFullPropertyPath(parent);
                if (parentPath != null)
                {
                    path = parentPath + "\\" + path;
                }
            }

            return path;
        }

        /// <summary>
        /// Gets property for dynamic properties dictionary.
        /// </summary>
        /// <param name="openNode"></param>
        /// <returns>Returns CLR property for dynamic properties container.</returns>
        protected PropertyInfo GetDynamicPropertyContainer(SingleValueOpenPropertyAccessNode openNode)
        {
            IEdmStructuredType edmStructuredType;
            IEdmTypeReference edmTypeReference = openNode.Source.TypeReference;
            if (edmTypeReference.IsEntity())
            {
                edmStructuredType = edmTypeReference.AsEntity().EntityDefinition();
            }
            else if (edmTypeReference.IsComplex())
            {
                edmStructuredType = edmTypeReference.AsComplex().ComplexDefinition();
            }
            else
            {
                throw Error.NotSupported(SRResources.QueryNodeBindingNotSupported, openNode.Kind, typeof(FilterBinder).Name);
            }

            return EdmLibHelpers.GetDynamicPropertyDictionary(edmStructuredType, Model);
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

        internal static Expression CheckForNull(Expression expression)
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

        internal static Expression ExtractValueFromNullableExpression(Expression source)
        {
            return Nullable.GetUnderlyingType(source.Type) != null ? Expression.Property(source, "Value") : source;
        }

        internal Expression BindHas(Expression left, Expression flag)
        {
            Contract.Assert(TypeHelper.IsEnum(left.Type));
            Contract.Assert(flag.Type == typeof(Enum));

            Expression[] arguments = new[] { left, flag };
            return MakeFunctionCall(ClrCanonicalFunctions.HasFlag, arguments);
        }

        /// <summary>
        /// Analyze previous query and extract grouped properties.
        /// </summary>
        /// <param name="source"></param>
        protected void EnsureFlattenedPropertyContainer(ParameterExpression source)
        {
            if (this.BaseQuery != null)
            {
                this.FlattenedPropertyContainer = this.FlattenedPropertyContainer ?? this.GetFlattenedProperties(source);
            }
        }

        internal IDictionary<string, Expression> GetFlattenedProperties(ParameterExpression source)
        {
            if (this.BaseQuery == null)
            {
                return null;
            }

            if (!typeof(GroupByWrapper).IsAssignableFrom(BaseQuery.ElementType))
            {
                return null;
            }

            var expression = BaseQuery.Expression as MethodCallExpression;
            if (expression == null)
            {
                return null;
            }

            // After $apply we could have other clauses, like $filter, $orderby etc.
            // Skip of filter expressions
            while (expression.Method.Name == "Where")
            {
                expression = expression.Arguments.FirstOrDefault() as MethodCallExpression;
            }

            if (expression == null)
            {
                return null;
            }

            var result = new Dictionary<string, Expression>();
            CollectAssigments(result, Expression.Property(source, "GroupByContainer"), ExtractContainerExpression(expression.Arguments.FirstOrDefault() as MethodCallExpression, "GroupByContainer"));
            CollectAssigments(result, Expression.Property(source, "Container"), ExtractContainerExpression(expression, "Container"));

            return result;
        }

        private static MemberInitExpression ExtractContainerExpression(MethodCallExpression expression, string containerName)
        {
            var memberInitExpression = ((expression.Arguments[1] as UnaryExpression).Operand as LambdaExpression).Body as MemberInitExpression;
            if (memberInitExpression != null)
            {
                var containerAssigment = memberInitExpression.Bindings.FirstOrDefault(m => m.Member.Name == containerName) as MemberAssignment;
                if (containerAssigment != null)
                {
                    return containerAssigment.Expression as MemberInitExpression;
                }
            }
            return null;
        }

        private static void CollectAssigments(IDictionary<string, Expression> flattenPropertyContainer, Expression source, MemberInitExpression expression, string prefix = null)
        {
            if (expression == null)
            {
                return;
            }

            string nameToAdd = null;
            Type resultType = null;
            MemberInitExpression nextExpression = null;
            Expression nestedExpression = null;
            foreach (var expr in expression.Bindings.OfType<MemberAssignment>())
            {
                var initExpr = expr.Expression as MemberInitExpression;
                if (initExpr != null && expr.Member.Name == "Next")
                {
                    nextExpression = initExpr;
                }
                else if (expr.Member.Name == "Name")
                {
                    nameToAdd = (expr.Expression as ConstantExpression).Value as string;
                }
                else if (expr.Member.Name == "Value" || expr.Member.Name == "NestedValue")
                {
                    resultType = expr.Expression.Type;
                    if (resultType == typeof(object) && expr.Expression.NodeType == ExpressionType.Convert)
                    {
                        resultType = ((UnaryExpression)expr.Expression).Operand.Type;
                    }

                    if (typeof(GroupByWrapper).IsAssignableFrom(resultType))
                    {
                        nestedExpression = expr.Expression;
                    }
                }
            }

            if (prefix != null)
            {
                nameToAdd = prefix + "\\" + nameToAdd;
            }

            if (typeof(GroupByWrapper).IsAssignableFrom(resultType))
            {
                flattenPropertyContainer.Add(nameToAdd, Expression.Property(source, "NestedValue"));
            }
            else
            {
                flattenPropertyContainer.Add(nameToAdd, Expression.Convert(Expression.Property(source, "Value"), resultType));
            }

            if (nextExpression != null)
            {
                CollectAssigments(flattenPropertyContainer, Expression.Property(source, "Next"), nextExpression, prefix);
            }

            if (nestedExpression != null)
            {
                var nestedAccessor = ((nestedExpression as MemberInitExpression).Bindings.First() as MemberAssignment).Expression as MemberInitExpression;
                var newSource = Expression.Property(Expression.Property(source, "NestedValue"), "GroupByContainer");
                CollectAssigments(flattenPropertyContainer, newSource, nestedAccessor, nameToAdd);
            }
        }

        /// <summary>
        /// Gets expression for property from previously aggregated query
        /// </summary>
        /// <param name="propertyPath"></param>
        /// <returns>Returns null if no aggregations were used so far</returns>
        protected Expression GetFlattenedPropertyExpression(string propertyPath)
        {
            if (FlattenedPropertyContainer == null)
            {
                return null;
            }

            Expression expression;
            if (FlattenedPropertyContainer.TryGetValue(propertyPath, out expression))
            {
                return expression;
            }

            throw new ODataException(Error.Format(SRResources.PropertyOrPathWasRemovedFromContext, propertyPath));
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

        internal static Expression ConvertToEnumUnderlyingType(Expression expression, Type enumType, Type enumUnderlyingType)
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
        internal static object ExtractParameterizedConstant(Expression expression)
        {
            if (expression.NodeType == ExpressionType.MemberAccess)
            {
                MemberExpression memberAccess = expression as MemberExpression;
                Contract.Assert(memberAccess != null);

                var propertyInfo = memberAccess.Member as PropertyInfo;
                if (propertyInfo != null && propertyInfo.GetMethod.IsStatic)
                {
                    return propertyInfo.GetValue(new object());
                }

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

        internal static Expression DateTimeOffsetToDateTime(Expression expression)
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

        internal static bool IsNullable(Type t)
        {
            if (!t.IsValueType || (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>)))
            {
                return true;
            }

            return false;
        }

        internal static Type ToNullable(Type t)
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

        internal static Expression ToNullable(Expression expression)
        {
            if (!IsNullable(expression.Type))
            {
                return Expression.Convert(expression, ToNullable(expression.Type));
            }

            return expression;
        }

        internal static bool IsIQueryable(Type type)
        {
            return typeof(IQueryable).IsAssignableFrom(type);
        }

        internal static bool IsDoubleOrDecimal(Type type)
        {
            return IsType<double>(type) || IsType<decimal>(type);
        }

        internal static bool IsDateRelated(Type type)
        {
            return IsType<Date>(type) || IsType<DateTime>(type) || IsType<DateTimeOffset>(type);
        }

        internal static bool IsTimeRelated(Type type)
        {
            return IsType<TimeOfDay>(type) || IsType<DateTime>(type) || IsType<DateTimeOffset>(type) || IsType<TimeSpan>(type);
        }

        internal static bool IsDateOrOffset(Type type)
        {
            return IsType<DateTime>(type) || IsType<DateTimeOffset>(type);
        }

        internal static bool IsDateTime(Type type)
        {
            return IsType<DateTime>(type);
        }

        internal static bool IsTimeSpan(Type type)
        {
            return IsType<TimeSpan>(type);
        }

        internal static bool IsTimeOfDay(Type type)
        {
            return IsType<TimeOfDay>(type);
        }

        internal static bool IsDate(Type type)
        {
            return IsType<Date>(type);
        }

        internal static bool IsInteger(Type type)
        {
            return IsType<short>(type) || IsType<int>(type) || IsType<long>(type);
        }

        internal static bool IsType<T>(Type type) where T : struct
        {
            return type == typeof(T) || type == typeof(T?);
        }

        internal static Expression ConvertNull(Expression expression, Type type)
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

        internal static Expression GetPropertyExpression(Expression source, string propertyPath)
        {
            string[] propertyNameParts = propertyPath.Split('\\');
            Expression propertyValue = source;
            foreach (var propertyName in propertyNameParts)
            {
                propertyValue = Expression.Property(propertyValue, propertyName);
            }
            return propertyValue;
        }

        /// <summary>
        /// Binds a <see cref="QueryNode"/> to create a LINQ <see cref="Expression"/> that represents the semantics
        /// of the <see cref="QueryNode"/>.
        /// </summary>
        /// <param name="node">The node to bind.</param>
        /// <returns>The LINQ <see cref="Expression"/> created.</returns>
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity",
            Justification = "These are simple conversion function and cannot be split up.")]
        public abstract Expression Bind(QueryNode node);

        /// <summary>
        /// Gets $it parameter
        /// </summary>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Meant to be abstract.")]
        protected abstract ParameterExpression GetParameter();

        /// <summary>
        /// Binds a <see cref="ConstantNode"/> to create a LINQ <see cref="Expression"/> that
        /// represents the semantics of the <see cref="ConstantNode"/>.
        /// </summary>
        /// <param name="constantNode">The node to bind.</param>
        /// <returns>The LINQ <see cref="Expression"/> created.</returns>
        public virtual Expression BindConstantNode(ConstantNode constantNode)
        {
            Contract.Assert(constantNode != null);

            // no need to parameterize null's as there cannot be multiple values for null.
            if (constantNode.Value == null)
            {
                return NullConstant;
            }

            Type constantType = EdmLibHelpers.GetClrType(constantNode.TypeReference, Model, AssembliesResolver);
            object value = constantNode.Value;

            if (constantNode.TypeReference != null && constantNode.TypeReference.IsEnum())
            {
                ODataEnumValue odataEnumValue = (ODataEnumValue)value;
                string strValue = odataEnumValue.Value;
                Contract.Assert(strValue != null);

                constantType = Nullable.GetUnderlyingType(constantType) ?? constantType;
                value = Enum.Parse(constantType, strValue);
            }

            if (constantNode.TypeReference != null &&
                constantNode.TypeReference.IsNullable &&
                (constantNode.TypeReference.IsDate() || constantNode.TypeReference.IsTimeOfDay()))
            {
                constantType = Nullable.GetUnderlyingType(constantType) ?? constantType;
            }

            if (QuerySettings.EnableConstantParameterization)
            {
                return LinqParameterContainer.Parameterize(constantType, value);
            }
            else
            {
                return Expression.Constant(value, constantType);
            }
        }

        /// <summary>
        /// Binds a <see cref="SingleValueFunctionCallNode"/> to create a LINQ <see cref="Expression"/> that
        /// represents the semantics of the <see cref="SingleValueFunctionCallNode"/>.
        /// </summary>
        /// <param name="node">The node to bind.</param>
        /// <returns>The LINQ <see cref="Expression"/> created.</returns>
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity",
            Justification = "The complexity come from the number of case statements, i.e. the number of values in ClrCanonicalFunctions. The function is simple in concept and reducing further would obfuscate the purpose of the code.")]
        public virtual Expression BindSingleValueFunctionCallNode(SingleValueFunctionCallNode node)
        {
            switch (node.Name)
            {
                case ClrCanonicalFunctions.StartswithFunctionName:
                    return BindStartsWith(node);

                case ClrCanonicalFunctions.EndswithFunctionName:
                    return BindEndsWith(node);

                case ClrCanonicalFunctions.ContainsFunctionName:
                    return BindContains(node);

                case ClrCanonicalFunctions.SubstringFunctionName:
                    return BindSubstring(node);

                case ClrCanonicalFunctions.LengthFunctionName:
                    return BindLength(node);

                case ClrCanonicalFunctions.IndexofFunctionName:
                    return BindIndexOf(node);

                case ClrCanonicalFunctions.TolowerFunctionName:
                    return BindToLower(node);

                case ClrCanonicalFunctions.ToupperFunctionName:
                    return BindToUpper(node);

                case ClrCanonicalFunctions.TrimFunctionName:
                    return BindTrim(node);

                case ClrCanonicalFunctions.ConcatFunctionName:
                    return BindConcat(node);

                case ClrCanonicalFunctions.YearFunctionName:
                case ClrCanonicalFunctions.MonthFunctionName:
                case ClrCanonicalFunctions.DayFunctionName:
                    return BindDateRelatedProperty(node); // Date & DateTime & DateTimeOffset

                case ClrCanonicalFunctions.HourFunctionName:
                case ClrCanonicalFunctions.MinuteFunctionName:
                case ClrCanonicalFunctions.SecondFunctionName:
                    return BindTimeRelatedProperty(node); // TimeOfDay & DateTime & DateTimeOffset

                case ClrCanonicalFunctions.FractionalSecondsFunctionName:
                    return BindFractionalSeconds(node);

                case ClrCanonicalFunctions.RoundFunctionName:
                    return BindRound(node);

                case ClrCanonicalFunctions.FloorFunctionName:
                    return BindFloor(node);

                case ClrCanonicalFunctions.CeilingFunctionName:
                    return BindCeiling(node);

                case ClrCanonicalFunctions.CastFunctionName:
                    return BindCastSingleValue(node);

                case ClrCanonicalFunctions.IsofFunctionName:
                    return BindIsOf(node);

                case ClrCanonicalFunctions.DateFunctionName:
                    return BindDate(node);

                case ClrCanonicalFunctions.TimeFunctionName:
                    return BindTime(node);

                case ClrCanonicalFunctions.NowFunctionName:
                    return BindNow(node);

                default:
                    // Get Expression of custom binded method.
                    Expression expression = BindCustomMethodExpressionOrNull(node);
                    if (expression != null)
                    {
                        return expression;
                    }

                    throw new NotImplementedException(Error.Format(SRResources.ODataFunctionNotSupported, node.Name));
            }
        }

        private Expression BindCastSingleValue(SingleValueFunctionCallNode node)
        {
            Contract.Assert(ClrCanonicalFunctions.CastFunctionName == node.Name);

            Expression[] arguments = BindArguments(node.Parameters);
            Contract.Assert(arguments.Length == 1 || arguments.Length == 2);

            Expression source = arguments.Length == 1 ? this.GetParameter() : arguments[0];
            string targetTypeName = (string)((ConstantNode)node.Parameters.Last()).Value;
            IEdmType targetEdmType = Model.FindType(targetTypeName);
            Type targetClrType = null;

            if (targetEdmType != null)
            {
                IEdmTypeReference targetEdmTypeReference = targetEdmType.ToEdmTypeReference(false);
                targetClrType = EdmLibHelpers.GetClrType(targetEdmTypeReference, Model);

                if (source != NullConstant)
                {
                    if (source.Type == targetClrType)
                    {
                        return source;
                    }

                    if ((!targetEdmTypeReference.IsPrimitive() && !targetEdmTypeReference.IsEnum()) ||
                        (EdmLibHelpers.GetEdmPrimitiveTypeOrNull(source.Type) == null && !TypeHelper.IsEnum(source.Type)))
                    {
                        // Cast fails and return null.
                        return NullConstant;
                    }
                }
            }

            if (targetClrType == null || source == NullConstant)
            {
                return NullConstant;
            }

            if (targetClrType == typeof(string))
            {
                return BindCastToStringType(source);
            }
            else if (TypeHelper.IsEnum(targetClrType))
            {
                return BindCastToEnumType(source.Type, targetClrType, node.Parameters.First(), arguments.Length);
            }
            else
            {
                if (source.Type.IsNullable() && !targetClrType.IsNullable())
                {
                    // Make the target Clr type nullable to avoid failure while casting
                    // nullable source, whose value may be null, to a non-nullable type.
                    // For example: cast(NullableInt32Property,Edm.Int64)
                    // The target Clr type should be Nullable<Int64> rather than Int64.
                    targetClrType = typeof(Nullable<>).MakeGenericType(targetClrType);
                }

                try
                {
                    return Expression.Convert(source, targetClrType);
                }
                catch (InvalidOperationException)
                {
                    // Cast fails and return null.
                    return NullConstant;
                }
            }
        }

        private static Expression BindCastToStringType(Expression source)
        {
            Expression sourceValue;

            if (source.Type.IsGenericType && source.Type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                if (TypeHelper.IsEnum(source.Type))
                {
                    // Entity Framework doesn't have ToString method for enum types.
                    // Convert enum types to their underlying numeric types.
                    sourceValue = Expression.Convert(
                        Expression.Property(source, "Value"),
                        Enum.GetUnderlyingType(TypeHelper.GetUnderlyingTypeOrSelf(source.Type)));
                }
                else
                {
                    // Entity Framework has ToString method for numeric types.
                    sourceValue = Expression.Property(source, "Value");
                }

                // Entity Framework doesn't have ToString method for nullable numeric types.
                // Call ToString method on non-nullable numeric types.
                return Expression.Condition(
                    Expression.Property(source, "HasValue"),
                    Expression.Call(sourceValue, "ToString", typeArguments: null, arguments: null),
                    Expression.Constant(null, typeof(string)));
            }
            else
            {
                sourceValue = TypeHelper.IsEnum(source.Type) ?
                    Expression.Convert(source, Enum.GetUnderlyingType(source.Type)) :
                    source;
                return Expression.Call(sourceValue, "ToString", typeArguments: null, arguments: null);
            }
        }

        private Expression BindCastToEnumType(Type sourceType, Type targetClrType, QueryNode firstParameter, int parameterLength)
        {
            Type enumType = TypeHelper.GetUnderlyingTypeOrSelf(targetClrType);
            ConstantNode sourceNode = firstParameter as ConstantNode;

            if (parameterLength == 1 || sourceNode == null || sourceType != typeof(string))
            {
                // We only support to cast Enumeration type from constant string now,
                // because LINQ to Entities does not recognize the method Enum.TryParse.
                return NullConstant;
            }
            else
            {
                object[] parameters = new[] { sourceNode.Value, Enum.ToObject(enumType, 0) };
                bool isSuccessful = (bool)EnumTryParseMethod.MakeGenericMethod(enumType).Invoke(null, parameters);

                if (isSuccessful)
                {
                    if (QuerySettings.EnableConstantParameterization)
                    {
                        return LinqParameterContainer.Parameterize(targetClrType, parameters[1]);
                    }
                    else
                    {
                        return Expression.Constant(parameters[1], targetClrType);
                    }
                }
                else
                {
                    return NullConstant;
                }
            }
        }

        private Expression BindIsOf(SingleValueFunctionCallNode node)
        {
            Contract.Assert(ClrCanonicalFunctions.IsofFunctionName == node.Name);

            Expression[] arguments = BindArguments(node.Parameters);

            // Edm.Boolean isof(type)  or
            // Edm.Boolean isof(expression,type)
            Contract.Assert(arguments.Length == 1 || arguments.Length == 2);

            Expression source = arguments.Length == 1 ? this.GetParameter() : arguments[0];
            if (source == NullConstant)
            {
                return FalseConstant;
            }

            string typeName = (string)((ConstantNode)node.Parameters.Last()).Value;

            IEdmType edmType = Model.FindType(typeName);
            Type clrType = null;
            if (edmType != null)
            {
                // bool nullable = source.Type.IsNullable();
                IEdmTypeReference edmTypeReference = edmType.ToEdmTypeReference(false);
                clrType = EdmLibHelpers.GetClrType(edmTypeReference, Model);
            }

            if (clrType == null)
            {
                return FalseConstant;
            }

            bool isSourcePrimitiveOrEnum = EdmLibHelpers.GetEdmPrimitiveTypeOrNull(source.Type) != null ||
                                           TypeHelper.IsEnum(source.Type);

            bool isTargetPrimitiveOrEnum = EdmLibHelpers.GetEdmPrimitiveTypeOrNull(clrType) != null ||
                                           TypeHelper.IsEnum(clrType);

            if (isSourcePrimitiveOrEnum && isTargetPrimitiveOrEnum)
            {
                if (source.Type.IsNullable())
                {
                    clrType = clrType.ToNullable();
                }
            }

            // Be caution: Type method of LINQ to Entities only supports entity type.
            return Expression.Condition(Expression.TypeIs(source, clrType), TrueConstant, FalseConstant);
        }

        private Expression BindCeiling(SingleValueFunctionCallNode node)
        {
            Contract.Assert("ceiling" == node.Name);

            Expression[] arguments = BindArguments(node.Parameters);

            Contract.Assert(arguments.Length == 1 && IsDoubleOrDecimal(arguments[0].Type));

            MethodInfo ceiling = IsType<double>(arguments[0].Type)
                ? ClrCanonicalFunctions.CeilingOfDouble
                : ClrCanonicalFunctions.CeilingOfDecimal;
            return MakeFunctionCall(ceiling, arguments);
        }

        private Expression BindFloor(SingleValueFunctionCallNode node)
        {
            Contract.Assert("floor" == node.Name);

            Expression[] arguments = BindArguments(node.Parameters);

            Contract.Assert(arguments.Length == 1 && IsDoubleOrDecimal(arguments[0].Type));

            MethodInfo floor = IsType<double>(arguments[0].Type)
                ? ClrCanonicalFunctions.FloorOfDouble
                : ClrCanonicalFunctions.FloorOfDecimal;
            return MakeFunctionCall(floor, arguments);
        }

        private Expression BindRound(SingleValueFunctionCallNode node)
        {
            Contract.Assert("round" == node.Name);

            Expression[] arguments = BindArguments(node.Parameters);

            Contract.Assert(arguments.Length == 1 && IsDoubleOrDecimal(arguments[0].Type));

            MethodInfo round = IsType<double>(arguments[0].Type)
                ? ClrCanonicalFunctions.RoundOfDouble
                : ClrCanonicalFunctions.RoundOfDecimal;
            return MakeFunctionCall(round, arguments);
        }

        private Expression BindDate(SingleValueFunctionCallNode node)
        {
            Contract.Assert("date" == node.Name);

            Expression[] arguments = BindArguments(node.Parameters);

            // We should support DateTime & DateTimeOffset even though DateTime is not part of OData v4 Spec.
            Contract.Assert(arguments.Length == 1 && IsDateOrOffset(arguments[0].Type));

            // EF doesn't support new Date(int, int, int), also doesn't support other property access, for example DateTime.Date.
            // Therefore, we just return the source (DateTime or DateTimeOffset).
            return arguments[0];
        }

        private Expression BindNow(SingleValueFunctionCallNode node)
        {
            Contract.Assert("now" == node.Name);

            // Function Now() does not take any arguemnts.
            Expression[] arguments = BindArguments(node.Parameters);
            Contract.Assert(arguments.Length == 0);

            return Expression.Property(null, typeof(DateTimeOffset), "UtcNow");
        }

        private Expression BindTime(SingleValueFunctionCallNode node)
        {
            Contract.Assert("time" == node.Name);

            Expression[] arguments = BindArguments(node.Parameters);

            // We should support DateTime & DateTimeOffset even though DateTime is not part of OData v4 Spec.
            Contract.Assert(arguments.Length == 1 && IsDateOrOffset(arguments[0].Type));

            // EF doesn't support new TimeOfDay(int, int, int, int), also doesn't support other property access, for example DateTimeOffset.DateTime.
            // Therefore, we just return the source (DateTime or DateTimeOffset).
            return arguments[0];
        }

        private Expression BindFractionalSeconds(SingleValueFunctionCallNode node)
        {
            Contract.Assert("fractionalseconds" == node.Name);

            Expression[] arguments = BindArguments(node.Parameters);
            Contract.Assert(arguments.Length == 1 && (IsTimeRelated(arguments[0].Type)));

            // We should support DateTime & DateTimeOffset even though DateTime is not part of OData v4 Spec.
            Expression parameter = arguments[0];

            PropertyInfo property;
            if (IsTimeOfDay(parameter.Type))
            {
                property = ClrCanonicalFunctions.TimeOfDayProperties[ClrCanonicalFunctions.MillisecondFunctionName];
            }
            else if (IsDateTime(parameter.Type))
            {
                property = ClrCanonicalFunctions.DateTimeProperties[ClrCanonicalFunctions.MillisecondFunctionName];
            }
            else if (IsTimeSpan(parameter.Type))
            {
                property = ClrCanonicalFunctions.TimeSpanProperties[ClrCanonicalFunctions.MillisecondFunctionName];
            }
            else
            {
                property = ClrCanonicalFunctions.DateTimeOffsetProperties[ClrCanonicalFunctions.MillisecondFunctionName];
            }

            // Millisecond
            Expression milliSecond = MakePropertyAccess(property, parameter);
            Expression decimalMilliSecond = Expression.Convert(milliSecond, typeof(decimal));
            Expression fractionalSeconds = Expression.Divide(decimalMilliSecond, Expression.Constant(1000m, typeof(decimal)));

            return CreateFunctionCallWithNullPropagation(fractionalSeconds, arguments);
        }

        private Expression BindDateRelatedProperty(SingleValueFunctionCallNode node)
        {
            Expression[] arguments = BindArguments(node.Parameters);
            Contract.Assert(arguments.Length == 1 && IsDateRelated(arguments[0].Type));

            // We should support DateTime & DateTimeOffset even though DateTime is not part of OData v4 Spec.
            Expression parameter = arguments[0];

            PropertyInfo property;
            if (IsDate(parameter.Type))
            {
                Contract.Assert(ClrCanonicalFunctions.DateProperties.ContainsKey(node.Name));
                property = ClrCanonicalFunctions.DateProperties[node.Name];
            }
            else if (IsDateTime(parameter.Type))
            {
                Contract.Assert(ClrCanonicalFunctions.DateTimeProperties.ContainsKey(node.Name));
                property = ClrCanonicalFunctions.DateTimeProperties[node.Name];
            }
            else
            {
                Contract.Assert(ClrCanonicalFunctions.DateTimeOffsetProperties.ContainsKey(node.Name));
                property = ClrCanonicalFunctions.DateTimeOffsetProperties[node.Name];
            }

            return MakeFunctionCall(property, parameter);
        }

        private Expression BindTimeRelatedProperty(SingleValueFunctionCallNode node)
        {
            Expression[] arguments = BindArguments(node.Parameters);
            Contract.Assert(arguments.Length == 1 && (IsTimeRelated(arguments[0].Type)));

            // We should support DateTime & DateTimeOffset even though DateTime is not part of OData v4 Spec.
            Expression parameter = arguments[0];

            PropertyInfo property;
            if (IsTimeOfDay(parameter.Type))
            {
                Contract.Assert(ClrCanonicalFunctions.TimeOfDayProperties.ContainsKey(node.Name));
                property = ClrCanonicalFunctions.TimeOfDayProperties[node.Name];
            }
            else if (IsDateTime(parameter.Type))
            {
                Contract.Assert(ClrCanonicalFunctions.DateTimeProperties.ContainsKey(node.Name));
                property = ClrCanonicalFunctions.DateTimeProperties[node.Name];
            }
            else if (IsTimeSpan(parameter.Type))
            {
                Contract.Assert(ClrCanonicalFunctions.TimeSpanProperties.ContainsKey(node.Name));
                property = ClrCanonicalFunctions.TimeSpanProperties[node.Name];
            }
            else
            {
                Contract.Assert(ClrCanonicalFunctions.DateTimeOffsetProperties.ContainsKey(node.Name));
                property = ClrCanonicalFunctions.DateTimeOffsetProperties[node.Name];
            }

            return MakeFunctionCall(property, parameter);
        }

        private Expression BindConcat(SingleValueFunctionCallNode node)
        {
            Contract.Assert("concat" == node.Name);

            Expression[] arguments = BindArguments(node.Parameters);
            ValidateAllStringArguments(node.Name, arguments);

            Contract.Assert(arguments.Length == 2 && arguments[0].Type == typeof(string) && arguments[1].Type == typeof(string));

            return MakeFunctionCall(ClrCanonicalFunctions.Concat, arguments);
        }

        private Expression BindTrim(SingleValueFunctionCallNode node)
        {
            Contract.Assert("trim" == node.Name);

            Expression[] arguments = BindArguments(node.Parameters);
            ValidateAllStringArguments(node.Name, arguments);

            Contract.Assert(arguments.Length == 1 && arguments[0].Type == typeof(string));

            return MakeFunctionCall(ClrCanonicalFunctions.Trim, arguments);
        }

        private Expression BindToUpper(SingleValueFunctionCallNode node)
        {
            Contract.Assert("toupper" == node.Name);

            Expression[] arguments = BindArguments(node.Parameters);
            ValidateAllStringArguments(node.Name, arguments);

            Contract.Assert(arguments.Length == 1 && arguments[0].Type == typeof(string));

            return MakeFunctionCall(ClrCanonicalFunctions.ToUpper, arguments);
        }

        private Expression BindToLower(SingleValueFunctionCallNode node)
        {
            Contract.Assert("tolower" == node.Name);

            Expression[] arguments = BindArguments(node.Parameters);
            ValidateAllStringArguments(node.Name, arguments);

            Contract.Assert(arguments.Length == 1 && arguments[0].Type == typeof(string));

            return MakeFunctionCall(ClrCanonicalFunctions.ToLower, arguments);
        }

        private Expression BindIndexOf(SingleValueFunctionCallNode node)
        {
            Contract.Assert("indexof" == node.Name);

            Expression[] arguments = BindArguments(node.Parameters);
            ValidateAllStringArguments(node.Name, arguments);

            Contract.Assert(arguments.Length == 2 && arguments[0].Type == typeof(string) && arguments[1].Type == typeof(string));

            return MakeFunctionCall(ClrCanonicalFunctions.IndexOf, arguments);
        }

        private Expression BindSubstring(SingleValueFunctionCallNode node)
        {
            Contract.Assert("substring" == node.Name);

            Expression[] arguments = BindArguments(node.Parameters);
            if (arguments[0].Type != typeof(string))
            {
                throw new ODataException(Error.Format(SRResources.FunctionNotSupportedOnEnum, node.Name));
            }

            Expression functionCall;
            if (arguments.Length == 2)
            {
                Contract.Assert(IsInteger(arguments[1].Type));

                // When null propagation is allowed, we use a safe version of String.Substring(int).
                // But for providers that would not recognize custom expressions like this, we map
                // directly to String.Substring(int)
                if (QuerySettings.HandleNullPropagation == HandleNullPropagationOption.True)
                {
                    // Safe function is static and takes string "this" as first argument
                    functionCall = MakeFunctionCall(ClrCanonicalFunctions.SubstringStartNoThrow, arguments);
                }
                else
                {
                    functionCall = MakeFunctionCall(ClrCanonicalFunctions.SubstringStart, arguments);
                }
            }
            else
            {
                // arguments.Length == 3 implies String.Substring(int, int)
                Contract.Assert(arguments.Length == 3 && IsInteger(arguments[1].Type) && IsInteger(arguments[2].Type));

                // When null propagation is allowed, we use a safe version of String.Substring(int, int).
                // But for providers that would not recognize custom expressions like this, we map
                // directly to String.Substring(int, int)
                if (QuerySettings.HandleNullPropagation == HandleNullPropagationOption.True)
                {
                    // Safe function is static and takes string "this" as first argument
                    functionCall = MakeFunctionCall(ClrCanonicalFunctions.SubstringStartAndLengthNoThrow, arguments);
                }
                else
                {
                    functionCall = MakeFunctionCall(ClrCanonicalFunctions.SubstringStartAndLength, arguments);
                }
            }

            return functionCall;
        }

        private Expression BindLength(SingleValueFunctionCallNode node)
        {
            Contract.Assert("length" == node.Name);

            Expression[] arguments = BindArguments(node.Parameters);
            ValidateAllStringArguments(node.Name, arguments);

            Contract.Assert(arguments.Length == 1 && arguments[0].Type == typeof(string));

            return MakeFunctionCall(ClrCanonicalFunctions.Length, arguments);
        }

        private Expression BindContains(SingleValueFunctionCallNode node)
        {
            Contract.Assert("contains" == node.Name);

            Expression[] arguments = BindArguments(node.Parameters);
            ValidateAllStringArguments(node.Name, arguments);

            Contract.Assert(arguments.Length == 2 && arguments[0].Type == typeof(string) && arguments[1].Type == typeof(string));

            return MakeFunctionCall(ClrCanonicalFunctions.Contains, arguments[0], arguments[1]);
        }

        private Expression BindStartsWith(SingleValueFunctionCallNode node)
        {
            Contract.Assert("startswith" == node.Name);

            Expression[] arguments = BindArguments(node.Parameters);
            ValidateAllStringArguments(node.Name, arguments);

            Contract.Assert(arguments.Length == 2 && arguments[0].Type == typeof(string) && arguments[1].Type == typeof(string));

            return MakeFunctionCall(ClrCanonicalFunctions.StartsWith, arguments);
        }

        private Expression BindEndsWith(SingleValueFunctionCallNode node)
        {
            Contract.Assert("endswith" == node.Name);

            Expression[] arguments = BindArguments(node.Parameters);
            ValidateAllStringArguments(node.Name, arguments);

            Contract.Assert(arguments.Length == 2 && arguments[0].Type == typeof(string) && arguments[1].Type == typeof(string));

            return MakeFunctionCall(ClrCanonicalFunctions.EndsWith, arguments);
        }

        /// <summary>
        /// Bind function arguments
        /// </summary>
        /// <param name="nodes"></param>
        /// <returns></returns>
        protected Expression[] BindArguments(IEnumerable<QueryNode> nodes)
        {
            return nodes.OfType<SingleValueNode>().Select(n => Bind(n)).ToArray();
        }

        private static void ValidateAllStringArguments(string functionName, Expression[] arguments)
        {
            if (arguments.Any(arg => arg.Type != typeof(string)))
            {
                throw new ODataException(Error.Format(SRResources.FunctionNotSupportedOnEnum, functionName));
            }
        }

        private Expression BindCustomMethodExpressionOrNull(SingleValueFunctionCallNode node)
        {
            Expression[] arguments = BindArguments(node.Parameters);
            IEnumerable<Type> methodArgumentsType = arguments.Select(argument => argument.Type);

            // Search for custom method info that are binded to the node name
            MethodInfo methodInfo;
            if (UriFunctionsBinder.TryGetMethodInfo(node.Name, methodArgumentsType, out methodInfo))
            {
                return MakeFunctionCall(methodInfo, arguments);
            }

            return null;
        }
    }
}
