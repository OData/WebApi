// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
#if NETFX // System.Data.Linq.Binary is only supported in the AspNet version.
using System.Data.Linq;
#endif
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Xml.Linq;
using Microsoft.AspNet.OData.Adapters;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNet.OData.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNet.OData.Query.Expressions
{
    /// <summary>
    /// The base class for all expression binders.
    /// </summary>
    public abstract class ExpressionBinderBase
    {
        internal static readonly MethodInfo StringCompareMethodInfo = typeof(string).GetMethod("Compare", new[] { typeof(string), typeof(string) });
        internal static readonly MethodInfo GuidCompareMethodInfo = typeof(ExpressionBinderBase).GetMethod("GuidCompare", new[] { typeof(Guid), typeof(Guid) });
        internal static readonly string DictionaryStringObjectIndexerName = typeof(Dictionary<string, object>).GetDefaultMembers()[0].Name;

        internal static readonly Expression NullConstant = Expression.Constant(null);
        internal static readonly Expression FalseConstant = Expression.Constant(false);
        internal static readonly Expression TrueConstant = Expression.Constant(true);
        internal static readonly Expression ZeroConstant = Expression.Constant(0);

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

        internal IWebApiAssembliesResolver InternalAssembliesResolver { get; set; }

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

            // The IWebApiAssembliesResolver service is internal and can only be injected by WebApi.
            // This code path may be used in the cases when the service container available
            // but may not contain an instance of IWebApiAssembliesResolver.
            IWebApiAssembliesResolver injectedResolver = requestContainer.GetService<IWebApiAssembliesResolver>();
            InternalAssembliesResolver = (injectedResolver != null) ? injectedResolver : WebApiAssembliesResolver.Default;
        }

        internal ExpressionBinderBase(IEdmModel model, IWebApiAssembliesResolver assembliesResolver, ODataQuerySettings querySettings)
            : this(model, querySettings)
        {
            InternalAssembliesResolver = assembliesResolver;
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
            if ((TypeHelper.IsEnum(leftUnderlyingType) || TypeHelper.IsEnum(rightUnderlyingType)) && binaryOperator != BinaryOperatorKind.Has)
            {
                Type enumType = TypeHelper.IsEnum(leftUnderlyingType) ? leftUnderlyingType : rightUnderlyingType;
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

            if (left.Type == typeof(Guid) || right.Type == typeof(Guid))
            {
                left = ConvertNull(left, typeof(Guid));
                right = ConvertNull(right, typeof(Guid));

                switch (binaryOperator)
                {
                    case BinaryOperatorKind.GreaterThan:
                    case BinaryOperatorKind.GreaterThanOrEqual:
                    case BinaryOperatorKind.LessThan:
                    case BinaryOperatorKind.LessThanOrEqual:
                        left = Expression.Call(GuidCompareMethodInfo, left, right);
                        right = ZeroConstant;
                        break;
                    default:
                        break;
                }
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
                        left = Expression.Call(StringCompareMethodInfo, left, right);
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
            Type conversionType = EdmLibHelpers.GetClrType(convertNode.TypeReference, Model, InternalAssembliesResolver);

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
            else if (IsDateAndTimeRelated(conversionType) && IsDateAndTimeRelated(source.Type))
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

                if (TypeHelper.IsEnum(sourceType))
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
#if NETFX // System.Data.Linq.Binary is only supported in the AspNet version.
                            else if (sourceType == typeof(Binary))
                            {
                                convertedExpression = Expression.Call(source, "ToArray", typeArguments: null, arguments: null);
                            }
#endif
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
                    var complexNode = (SingleComplexNode)node;
                    path = complexNode.Property.Name;
                    parent = complexNode.Source;
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
            if (parameterizedConstantValue != null && TypeHelper.IsNullable(source.Type))
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

                PropertyInfo propertyInfo = memberAccess.Member as PropertyInfo;
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
            if (!TypeHelper.IsValueType(t) || (TypeHelper.IsGenericType(t) && t.GetGenericTypeDefinition() == typeof(Nullable<>)))
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

        internal static bool IsDateAndTimeRelated(Type type)
        {
            return IsType<Date>(type) ||
                IsType<DateTime>(type) ||
                IsType<DateTimeOffset>(type) ||
                IsType<TimeOfDay>(type) ||
                IsType<TimeSpan>(type);
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

        /// <summary>
        /// Compares two guids
        /// </summary>
        /// <param name="firstValue"></param>
        /// <param name="secondValue"></param>
        /// <returns>An integer value based on the Guid's CompareTo method</returns>
        public static int GuidCompare(Guid firstValue, Guid secondValue)
        {
            if (firstValue != null)
            {
                return firstValue.CompareTo(secondValue);
            }

            if (secondValue != null)
            {
                return (-1) * secondValue.CompareTo(firstValue);
            }

            return 0;
        }
    }
}
