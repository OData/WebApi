// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Data.Linq;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using System.Web.OData.Formatter;
using System.Web.OData.Properties;
using System.Xml.Linq;
using Microsoft.OData.Core;
using Microsoft.OData.Core.UriParser.Semantic;
using Microsoft.OData.Core.UriParser.TreeNodeKinds;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Library.Values;
using Microsoft.OData.Edm.Values;

namespace System.Web.OData.Query.Expressions
{
    /// <summary>
    /// Translates an OData $filter parse tree represented by <see cref="FilterClause"/> to 
    /// an <see cref="Expression"/> and applies it to an <see cref="IQueryable"/>.
    /// </summary>
    [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Relies on many ODataLib classes.")]
    internal class FilterBinder
    {
        private const string ODataItParameterName = "$it";

        private static readonly MethodInfo _stringCompareMethodInfo = typeof(string).GetMethod("Compare", new[] { typeof(string), typeof(string), typeof(StringComparison) });

        private static readonly Expression _nullConstant = Expression.Constant(null);
        private static readonly Expression _falseConstant = Expression.Constant(false);
        private static readonly Expression _trueConstant = Expression.Constant(true);
        private static readonly Expression _zeroConstant = Expression.Constant(0);
        private static readonly Expression _ordinalStringComparisonConstant = Expression.Constant(StringComparison.Ordinal);

        private static readonly MethodInfo _enumTryParseMethod = typeof(Enum).GetMethods()
                        .Single(m => m.Name == "TryParse" && m.GetParameters().Length == 2);

        private static Dictionary<BinaryOperatorKind, ExpressionType> _binaryOperatorMapping = new Dictionary<BinaryOperatorKind, ExpressionType>
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

        private IEdmModel _model;

        private Stack<Dictionary<string, ParameterExpression>> _parametersStack;
        private Dictionary<string, ParameterExpression> _lambdaParameters;

        private ODataQuerySettings _querySettings;
        private IAssembliesResolver _assembliesResolver;

        private FilterBinder(IEdmModel model, IAssembliesResolver assembliesResolver, ODataQuerySettings querySettings)
            : this(model, querySettings)
        {
            _assembliesResolver = assembliesResolver;
        }

        private FilterBinder(IEdmModel model, ODataQuerySettings querySettings)
        {
            Contract.Assert(model != null);
            Contract.Assert(querySettings != null);
            Contract.Assert(querySettings.HandleNullPropagation != HandleNullPropagationOption.Default);

            _querySettings = querySettings;
            _parametersStack = new Stack<Dictionary<string, ParameterExpression>>();
            _model = model;
        }

        public static Expression<Func<TEntityType, bool>> Bind<TEntityType>(FilterClause filterClause, IEdmModel model,
            IAssembliesResolver assembliesResolver, ODataQuerySettings querySettings)
        {
            return Bind(filterClause, typeof(TEntityType), model, assembliesResolver, querySettings) as Expression<Func<TEntityType, bool>>;
        }

        public static Expression Bind(FilterClause filterClause, Type filterType, IEdmModel model,
            IAssembliesResolver assembliesResolver, ODataQuerySettings querySettings)
        {
            if (filterClause == null)
            {
                throw Error.ArgumentNull("filterClause");
            }
            if (filterType == null)
            {
                throw Error.ArgumentNull("filterType");
            }
            if (model == null)
            {
                throw Error.ArgumentNull("model");
            }
            if (assembliesResolver == null)
            {
                throw Error.ArgumentNull("assembliesResolver");
            }

            FilterBinder binder = new FilterBinder(model, assembliesResolver, querySettings);
            LambdaExpression filter = binder.BindExpression(filterClause.Expression, filterClause.RangeVariable, filterType);
            filter = Expression.Lambda(binder.ApplyNullPropagationForFilterBody(filter.Body), filter.Parameters);

            Type expectedFilterType = typeof(Func<,>).MakeGenericType(filterType, typeof(bool));
            if (filter.Type != expectedFilterType)
            {
                throw Error.Argument("filterType", SRResources.CannotCastFilter, filter.Type.FullName, expectedFilterType.FullName);
            }

            return filter;
        }

        public static LambdaExpression Bind(OrderByClause orderBy, Type elementType,
            IEdmModel model, ODataQuerySettings querySettings)
        {
            Contract.Assert(orderBy != null);
            Contract.Assert(elementType != null);
            Contract.Assert(model != null);
            Contract.Assert(querySettings != null);

            FilterBinder binder = new FilterBinder(model, querySettings);
            LambdaExpression orderByLambda = binder.BindExpression(orderBy.Expression, orderBy.RangeVariable, elementType);
            return orderByLambda;
        }

        private Expression Bind(QueryNode node)
        {
            // Recursion guard to avoid stack overflows
            RuntimeHelpers.EnsureSufficientExecutionStack();

            CollectionNode collectionNode = node as CollectionNode;
            SingleValueNode singleValueNode = node as SingleValueNode;

            if (collectionNode != null)
            {
                switch (node.Kind)
                {
                    case QueryNodeKind.CollectionNavigationNode:
                        CollectionNavigationNode navigationNode = node as CollectionNavigationNode;
                        return BindNavigationPropertyNode(navigationNode.Source, navigationNode.NavigationProperty);

                    case QueryNodeKind.CollectionPropertyAccess:
                        return BindCollectionPropertyAccessNode(node as CollectionPropertyAccessNode);

                    case QueryNodeKind.EntityCollectionCast:
                        return BindEntityCollectionCastNode(node as EntityCollectionCastNode);

                    default:
                        throw Error.NotSupported(SRResources.QueryNodeBindingNotSupported, node.Kind, typeof(FilterBinder).Name);
                }
            }
            else if (singleValueNode != null)
            {
                switch (node.Kind)
                {
                    case QueryNodeKind.BinaryOperator:
                        return BindBinaryOperatorNode(node as BinaryOperatorNode);

                    case QueryNodeKind.Constant:
                        return BindConstantNode(node as ConstantNode);

                    case QueryNodeKind.Convert:
                        return BindConvertNode(node as ConvertNode);

                    case QueryNodeKind.EntityRangeVariableReference:
                        return BindRangeVariable((node as EntityRangeVariableReferenceNode).RangeVariable);

                    case QueryNodeKind.NonentityRangeVariableReference:
                        return BindRangeVariable((node as NonentityRangeVariableReferenceNode).RangeVariable);

                    case QueryNodeKind.SingleValuePropertyAccess:
                        return BindPropertyAccessQueryNode(node as SingleValuePropertyAccessNode);

                    case QueryNodeKind.UnaryOperator:
                        return BindUnaryOperatorNode(node as UnaryOperatorNode);

                    case QueryNodeKind.SingleValueFunctionCall:
                        return BindSingleValueFunctionCallNode(node as SingleValueFunctionCallNode);

                    case QueryNodeKind.SingleNavigationNode:
                        SingleNavigationNode navigationNode = node as SingleNavigationNode;
                        return BindNavigationPropertyNode(navigationNode.Source, navigationNode.NavigationProperty);

                    case QueryNodeKind.Any:
                        return BindAnyNode(node as AnyNode);

                    case QueryNodeKind.All:
                        return BindAllNode(node as AllNode);

                    case QueryNodeKind.SingleEntityCast:
                        return BindSingleEntityCastNode(node as SingleEntityCastNode);

                    case QueryNodeKind.SingleEntityFunctionCall:
                        return BindSingleEntityFunctionCallNode(node as SingleEntityFunctionCallNode);

                    default:
                        throw Error.NotSupported(SRResources.QueryNodeBindingNotSupported, node.Kind, typeof(FilterBinder).Name);
                }
            }
            else
            {
                throw Error.NotSupported(SRResources.QueryNodeBindingNotSupported, node.Kind, typeof(FilterBinder).Name);
            }
        }

        private Expression BindSingleEntityFunctionCallNode(SingleEntityFunctionCallNode node)
        {
            switch (node.Name)
            {
                case ClrCanonicalFunctions.CastFunctionName:
                    return BindSingleEntityCastFunctionCall(node);
                default:
                    throw Error.NotSupported(SRResources.ODataFunctionNotSupported, node.Name);
            }
        }

        private Expression BindSingleEntityCastFunctionCall(SingleEntityFunctionCallNode node)
        {
            Contract.Assert(ClrCanonicalFunctions.CastFunctionName == node.Name);

            Expression[] arguments = BindArguments(node.Parameters);

            Contract.Assert(arguments.Length == 2);

            string targetEdmTypeName = (string)((ConstantNode)node.Parameters.Last()).Value;
            IEdmType targetEdmType = _model.FindType(targetEdmTypeName);
            Type targetClrType = null;

            if (targetEdmType != null)
            {
                targetClrType = EdmLibHelpers.GetClrType(targetEdmType.ToEdmTypeReference(false), _model);
            }

            if (arguments[0].Type == targetClrType)
            {
                // We only support to cast Entity type to the same type now.
                return arguments[0];
            }
            else
            {
                // Cast fails and return null.
                return _nullConstant;
            }
        }

        private Expression BindSingleEntityCastNode(SingleEntityCastNode node)
        {
            IEdmEntityTypeReference entity = node.EntityTypeReference;
            Contract.Assert(entity != null, "NS casts can contain only entity types");

            Type clrType = EdmLibHelpers.GetClrType(entity, _model);

            Expression source = BindCastSourceNode(node.Source);
            return Expression.TypeAs(source, clrType);
        }

        private Expression BindEntityCollectionCastNode(EntityCollectionCastNode node)
        {
            IEdmEntityTypeReference entity = node.EntityItemType;
            Contract.Assert(entity != null, "NS casts can contain only entity types");

            Type clrType = EdmLibHelpers.GetClrType(entity, _model);

            Expression source = BindCastSourceNode(node.Source);
            return OfType(source, clrType);
        }

        private Expression BindCastSourceNode(QueryNode sourceNode)
        {
            Expression source;
            if (sourceNode == null)
            {
                // if the cast is on the root i.e $it (~/Products?$filter=NS.PopularProducts/.....), 
                // source would be null. So bind null to '$it'.
                source = _lambdaParameters[ODataItParameterName];
            }
            else
            {
                source = Bind(sourceNode);
            }

            return source;
        }

        private static Expression OfType(Expression source, Type elementType)
        {
            Contract.Assert(source != null);
            Contract.Assert(elementType != null);

            if (IsIQueryable(source.Type))
            {
                return Expression.Call(null, ExpressionHelperMethods.QueryableOfType.MakeGenericMethod(elementType), source);
            }
            else
            {
                return Expression.Call(null, ExpressionHelperMethods.EnumerableOfType.MakeGenericMethod(elementType), source);
            }
        }

        private Expression BindNavigationPropertyNode(QueryNode sourceNode, IEdmNavigationProperty navigationProperty)
        {
            Expression source;

            // TODO: bug in uri parser is causing this property to be null for the root property.
            if (sourceNode == null)
            {
                source = _lambdaParameters[ODataItParameterName];
            }
            else
            {
                source = Bind(sourceNode);
            }

            return CreatePropertyAccessExpression(source, navigationProperty);
        }

        private Expression BindBinaryOperatorNode(BinaryOperatorNode binaryOperatorNode)
        {
            Expression left = Bind(binaryOperatorNode.Left);
            Expression right = Bind(binaryOperatorNode.Right);

            // handle null propagation only if either of the operands can be null
            bool isNullPropagationRequired = _querySettings.HandleNullPropagation == HandleNullPropagationOption.True && (IsNullable(left.Type) || IsNullable(right.Type));
            if (isNullPropagationRequired)
            {
                // |----------------------------------------------------------------|
                // |SQL 3VL truth table.                                            |
                // |----------------------------------------------------------------|
                // |p       |    q      |    p OR q     |    p AND q    |    p = q  |
                // |----------------------------------------------------------------|
                // |True    |   True    |   True        |   True        |   True    |
                // |True    |   False   |   True        |   False       |   False   |
                // |True    |   NULL    |   True        |   NULL        |   NULL    |
                // |False   |   True    |   True        |   False       |   False   |
                // |False   |   False   |   False       |   False       |   True    |
                // |False   |   NULL    |   NULL        |   False       |   NULL    |
                // |NULL    |   True    |   True        |   NULL        |   NULL    |
                // |NULL    |   False   |   NULL        |   False       |   NULL    |
                // |NULL    |   NULL    |   Null        |   NULL        |   NULL    |
                // |--------|-----------|---------------|---------------|-----------|

                // before we start with null propagation, convert the operators to nullable if already not.
                left = ToNullable(left);
                right = ToNullable(right);

                bool liftToNull = true;
                if (left == _nullConstant || right == _nullConstant)
                {
                    liftToNull = false;
                }

                // Expression trees do a very good job of handling the 3VL truth table if we pass liftToNull true.
                return CreateBinaryExpression(binaryOperatorNode.OperatorKind, left, right, liftToNull: liftToNull);
            }
            else
            {
                return CreateBinaryExpression(binaryOperatorNode.OperatorKind, left, right, liftToNull: false);
            }
        }

        private Expression BindConstantNode(ConstantNode constantNode)
        {
            Contract.Assert(constantNode != null);

            // no need to parameterize null's as there cannot be multiple values for null.
            if (constantNode.Value == null)
            {
                return _nullConstant;
            }

            Type constantType = EdmLibHelpers.GetClrType(constantNode.TypeReference, _model, _assembliesResolver);
            object value = constantNode.Value;

            if (constantNode.TypeReference != null && constantNode.TypeReference.IsEnum())
            {
                ODataEnumValue odataEnumValue = (ODataEnumValue)value;
                string strValue = odataEnumValue.Value;
                Contract.Assert(strValue != null);
                value = Enum.Parse(constantType, strValue);
            }

            if (_querySettings.EnableConstantParameterization)
            {
                return LinqParameterContainer.Parameterize(constantType, value);
            }
            else
            {
                return Expression.Constant(value, constantType);
            }
        }

        private Expression BindConvertNode(ConvertNode convertNode)
        {
            Contract.Assert(convertNode != null);
            Contract.Assert(convertNode.TypeReference != null);

            Expression source = Bind(convertNode.Source);

            Type conversionType = EdmLibHelpers.GetClrType(convertNode.TypeReference, _model, _assembliesResolver);

            if (conversionType == typeof(bool?) && source.Type == typeof(bool))
            {
                // we handle null propagation ourselves. So, if converting from bool to Nullable<bool> ignore.
                return source;
            }
            else if (source == _nullConstant)
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

        private LambdaExpression BindExpression(SingleValueNode expression, RangeVariable rangeVariable, Type elementType)
        {
            ParameterExpression filterParameter = Expression.Parameter(elementType, rangeVariable.Name);
            _lambdaParameters = new Dictionary<string, ParameterExpression>();
            _lambdaParameters.Add(rangeVariable.Name, filterParameter);

            Expression body = Bind(expression);
            return Expression.Lambda(body, filterParameter);
        }

        private Expression ApplyNullPropagationForFilterBody(Expression body)
        {
            if (IsNullable(body.Type))
            {
                if (_querySettings.HandleNullPropagation == HandleNullPropagationOption.True)
                {
                    // handle null as false
                    // body => body == true. passing liftToNull:false would convert null to false.
                    body = Expression.Equal(body, Expression.Constant(true, typeof(bool?)), liftToNull: false, method: null);
                }
                else
                {
                    body = Expression.Convert(body, typeof(bool));
                }
            }

            return body;
        }

        private Expression BindRangeVariable(RangeVariable rangeVariable)
        {
            ParameterExpression parameter = _lambdaParameters[rangeVariable.Name];
            return ConvertNonStandardPrimitives(parameter);
        }

        private Expression BindCollectionPropertyAccessNode(CollectionPropertyAccessNode propertyAccessNode)
        {
            Expression source = Bind(propertyAccessNode.Source);
            return CreatePropertyAccessExpression(source, propertyAccessNode.Property);
        }

        private Expression BindPropertyAccessQueryNode(SingleValuePropertyAccessNode propertyAccessNode)
        {
            Expression source = Bind(propertyAccessNode.Source);
            return CreatePropertyAccessExpression(source, propertyAccessNode.Property);
        }

        private Expression CreatePropertyAccessExpression(Expression source, IEdmProperty property)
        {
            string propertyName = EdmLibHelpers.GetClrPropertyName(property, _model);
            if (_querySettings.HandleNullPropagation == HandleNullPropagationOption.True && IsNullable(source.Type) && source != _lambdaParameters[ODataItParameterName])
            {
                Expression propertyAccessExpression = Expression.Property(RemoveInnerNullPropagation(source), propertyName);

                // source.property => source == null ? null : [CastToNullable]RemoveInnerNullPropagation(source).property
                // Notice that we are checking if source is null already. so we can safely remove any null checks when doing source.Property

                Expression ifFalse = ToNullable(ConvertNonStandardPrimitives(propertyAccessExpression));
                return
                    Expression.Condition(
                        test: Expression.Equal(source, _nullConstant),
                        ifTrue: Expression.Constant(null, ifFalse.Type),
                        ifFalse: ifFalse);
            }
            else
            {
                return ConvertNonStandardPrimitives(Expression.Property(source, propertyName));
            }
        }

        private Expression BindUnaryOperatorNode(UnaryOperatorNode unaryOperatorNode)
        {
            // No need to handle null-propagation here as CLR already handles it.
            // !(null) = null
            // -(null) = null
            Expression inner = Bind(unaryOperatorNode.Operand);
            switch (unaryOperatorNode.OperatorKind)
            {
                case UnaryOperatorKind.Negate:
                    return Expression.Negate(inner);

                case UnaryOperatorKind.Not:
                    return Expression.Not(inner);

                default:
                    throw Error.NotSupported(SRResources.QueryNodeBindingNotSupported, unaryOperatorNode.Kind, typeof(FilterBinder).Name);
            }
        }

        private Expression BindSingleValueFunctionCallNode(SingleValueFunctionCallNode node)
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
                case ClrCanonicalFunctions.HourFunctionName:
                case ClrCanonicalFunctions.MinuteFunctionName:
                case ClrCanonicalFunctions.SecondFunctionName:
                    return BindDateOrDateTimeOffsetProperty(node);

                case ClrCanonicalFunctions.YearsFunctionName:
                case ClrCanonicalFunctions.MonthsFunctionName:
                case ClrCanonicalFunctions.DaysFunctionName:
                case ClrCanonicalFunctions.HoursFunctionName:
                case ClrCanonicalFunctions.MinutesFunctionName:
                case ClrCanonicalFunctions.SecondsFunctionName:
                    return BindTimeSpanProperty(node);

                case ClrCanonicalFunctions.RoundFunctionName:
                    return BindRound(node);

                case ClrCanonicalFunctions.FloorFunctionName:
                    return BindFloor(node);

                case ClrCanonicalFunctions.CeilingFunctionName:
                    return BindCeiling(node);

                case ClrCanonicalFunctions.CastFunctionName:
                    return BindCastSingleValue(node);

                default:
                    throw new NotImplementedException(Error.Format(SRResources.ODataFunctionNotSupported, node.Name));
            }
        }

        private Expression CreateFunctionCallWithNullPropagation(Expression functionCall, Expression[] arguments)
        {
            if (_querySettings.HandleNullPropagation == HandleNullPropagationOption.True)
            {
                Expression test = CheckIfArgumentsAreNull(arguments);

                if (test == _falseConstant)
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
        private Expression RemoveInnerNullPropagation(Expression expression)
        {
            Contract.Assert(expression != null);

            if (_querySettings.HandleNullPropagation == HandleNullPropagationOption.True)
            {
                // only null propagation generates conditional expressions
                if (expression.NodeType == ExpressionType.Conditional)
                {
                    expression = (expression as ConditionalExpression).IfFalse;
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

            return expression;
        }

        // creates an expression for the corresponding OData function.
        private Expression MakeFunctionCall(MemberInfo member, params Expression[] arguments)
        {
            Contract.Assert(member.MemberType == MemberTypes.Property || member.MemberType == MemberTypes.Method);

            IEnumerable<Expression> functionCallArguments = arguments;
            if (_querySettings.HandleNullPropagation == HandleNullPropagationOption.True)
            {
                // we don't have to check if the argument is null inside the function call as we do it already
                // before calling the function. So remove the redunadant null checks.
                functionCallArguments = arguments.Select(a => RemoveInnerNullPropagation(a));
            }

            // if the argument is of type Nullable<T>, then translate the argument to Nullable<T>.Value as none 
            // of the cannonical functions have overloads for Nullable<> arguments.
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

        private Expression BindCastSingleValue(SingleValueFunctionCallNode node)
        {
            Contract.Assert(ClrCanonicalFunctions.CastFunctionName == node.Name);

            Expression[] arguments = BindArguments(node.Parameters);
            Contract.Assert(arguments.Length == 1 || arguments.Length == 2);

            Expression source = arguments.Length == 1 ? _lambdaParameters[ODataItParameterName] : arguments[0];
            string targetTypeName = (string)((ConstantNode)node.Parameters.Last()).Value;
            IEdmType targetEdmType = _model.FindType(targetTypeName);
            Type targetClrType = null;

            if (targetEdmType != null)
            {
                IEdmTypeReference targetEdmTypeReference = targetEdmType.ToEdmTypeReference(false);
                targetClrType = EdmLibHelpers.GetClrType(targetEdmTypeReference, _model);

                if (source != _nullConstant)
                {
                    if (source.Type == targetClrType)
                    {
                        return source;
                    }

                    if ((!targetEdmTypeReference.IsPrimitive() && !targetEdmTypeReference.IsEnum()) ||
                        (EdmLibHelpers.GetEdmPrimitiveTypeOrNull(source.Type) == null && !TypeHelper.IsEnum(source.Type)))
                    {
                        // Cast fails and return null.
                        return _nullConstant;
                    }
                }
            }

            if (targetClrType == null || source == _nullConstant)
            {
                return _nullConstant;
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
                    return _nullConstant;
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
                return _nullConstant;
            }
            else
            {
                object[] parameters = new[] { sourceNode.Value, Enum.ToObject(enumType, 0) };
                bool isSuccessful = (bool)_enumTryParseMethod.MakeGenericMethod(enumType).Invoke(null, parameters);

                if (isSuccessful)
                {
                    if (_querySettings.EnableConstantParameterization)
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
                    return _nullConstant;
                }
            }
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

        private Expression BindDateOrDateTimeOffsetProperty(SingleValueFunctionCallNode node)
        {
            Expression[] arguments = BindArguments(node.Parameters);

            Contract.Assert(arguments.Length == 1 && IsDateOrOffset(arguments[0].Type));

            PropertyInfo property;
            if (IsDate(arguments[0].Type))
            {
                Contract.Assert(ClrCanonicalFunctions.DateProperties.ContainsKey(node.Name));
                property = ClrCanonicalFunctions.DateProperties[node.Name];
            }
            else
            {
                Contract.Assert(ClrCanonicalFunctions.DateTimeOffsetProperties.ContainsKey(node.Name));
                property = ClrCanonicalFunctions.DateTimeOffsetProperties[node.Name];
            }

            return MakeFunctionCall(property, arguments);
        }

        private Expression BindTimeSpanProperty(SingleValueFunctionCallNode node)
        {
            Expression[] arguments = BindArguments(node.Parameters);

            Contract.Assert(arguments.Length == 1 && IsDateOrOffset(arguments[0].Type));
            Contract.Assert(ClrCanonicalFunctions.TimeSpanProperties.ContainsKey(node.Name));

            return MakeFunctionCall(ClrCanonicalFunctions.TimeSpanProperties[node.Name], arguments);
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
                if (_querySettings.HandleNullPropagation == HandleNullPropagationOption.True)
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
                if (_querySettings.HandleNullPropagation == HandleNullPropagationOption.True)
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

        private Expression BindHas(Expression left, Expression flag)
        {
            Contract.Assert(TypeHelper.IsEnum(left.Type));
            Contract.Assert(flag.Type == typeof(Enum));

            Expression[] arguments = new[] { left, flag };
            return MakeFunctionCall(ClrCanonicalFunctions.HasFlag, arguments);
        }

        private Expression[] BindArguments(IEnumerable<QueryNode> nodes)
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

        private Expression BindAllNode(AllNode allNode)
        {
            ParameterExpression allIt = HandleLambdaParameters(allNode.RangeVariables);

            Expression source;
            Contract.Assert(allNode.Source != null);
            source = Bind(allNode.Source);

            Expression body = source;
            Contract.Assert(allNode.Body != null);

            body = Bind(allNode.Body);
            body = ApplyNullPropagationForFilterBody(body);
            body = Expression.Lambda(body, allIt);

            Expression all = All(source, body);

            ExitLamdbaScope();

            if (_querySettings.HandleNullPropagation == HandleNullPropagationOption.True && IsNullable(source.Type))
            {
                // IFF(source == null) null; else Any(body);
                all = ToNullable(all);
                return Expression.Condition(
                    test: Expression.Equal(source, _nullConstant),
                    ifTrue: Expression.Constant(null, all.Type),
                    ifFalse: all);
            }
            else
            {
                return all;
            }
        }

        private Expression BindAnyNode(AnyNode anyNode)
        {
            ParameterExpression anyIt = HandleLambdaParameters(anyNode.RangeVariables);

            Expression source;
            Contract.Assert(anyNode.Source != null);
            source = Bind(anyNode.Source);

            Expression body = null;
            // uri parser places an Constant node with value true for empty any() body
            if (anyNode.Body != null && anyNode.Body.Kind != QueryNodeKind.Constant)
            {
                body = Bind(anyNode.Body);
                body = ApplyNullPropagationForFilterBody(body);
                body = Expression.Lambda(body, anyIt);
            }

            Expression any = Any(source, body);

            ExitLamdbaScope();

            if (_querySettings.HandleNullPropagation == HandleNullPropagationOption.True && IsNullable(source.Type))
            {
                // IFF(source == null) null; else Any(body);
                any = ToNullable(any);
                return Expression.Condition(
                    test: Expression.Equal(source, _nullConstant),
                    ifTrue: Expression.Constant(null, any.Type),
                    ifFalse: any);
            }
            else
            {
                return any;
            }
        }

        private ParameterExpression HandleLambdaParameters(IEnumerable<RangeVariable> rangeVariables)
        {
            ParameterExpression lambdaIt = null;

            EnterLambdaScope();

            Dictionary<string, ParameterExpression> newParameters = new Dictionary<string, ParameterExpression>();
            foreach (RangeVariable rangeVariable in rangeVariables)
            {
                ParameterExpression parameter;
                if (!_lambdaParameters.TryGetValue(rangeVariable.Name, out parameter))
                {
                    // Work-around issue 481323 where UriParser yields a collection parameter type
                    // for primitive collections rather than the inner element type of the collection.
                    // Remove this block of code when 481323 is resolved.
                    IEdmTypeReference edmTypeReference = rangeVariable.TypeReference;
                    IEdmCollectionTypeReference collectionTypeReference = edmTypeReference as IEdmCollectionTypeReference;
                    if (collectionTypeReference != null)
                    {
                        IEdmCollectionType collectionType = collectionTypeReference.Definition as IEdmCollectionType;
                        if (collectionType != null)
                        {
                            edmTypeReference = collectionType.ElementType;
                        }
                    }

                    parameter = Expression.Parameter(EdmLibHelpers.GetClrType(edmTypeReference, _model, _assembliesResolver), rangeVariable.Name);
                    Contract.Assert(lambdaIt == null, "There can be only one parameter in an Any/All lambda");
                    lambdaIt = parameter;
                }
                newParameters.Add(rangeVariable.Name, parameter);
            }

            _lambdaParameters = newParameters;
            return lambdaIt;
        }

        // If the expression is of non-standard edm primitive type (like uint), convert the expression to its standard edm type.
        // Also, note that only expressions generated for ushort, uint and ulong can be understood by linq2sql and EF.
        // The rest (char, char[], Binary) would cause issues with linq2sql and EF.
        private Expression ConvertNonStandardPrimitives(Expression source)
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

        private void EnterLambdaScope()
        {
            Contract.Assert(_lambdaParameters != null);
            _parametersStack.Push(_lambdaParameters);
        }

        private void ExitLamdbaScope()
        {
            if (_parametersStack.Count != 0)
            {
                _lambdaParameters = _parametersStack.Pop();
            }
            else
            {
                _lambdaParameters = null;
            }
        }

        private Expression CreateBinaryExpression(BinaryOperatorKind binaryOperator, Expression left, Expression right, bool liftToNull)
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
                        left = Expression.Call(_stringCompareMethodInfo, left, right, _ordinalStringComparisonConstant);
                        right = _zeroConstant;
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

        private static Expression ConvertToEnumUnderlyingType(Expression expression, Type enumType, Type enumUnderlyingType)
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
            else if (expression.NodeType == ExpressionType.Constant)
            {
                // only null constants are not parameterized.
                Contract.Assert((expression as ConstantExpression).Value == null);
                return expression;
            }
            else if (expression.Type == enumType)
            {
                return Expression.Convert(expression, enumUnderlyingType);
            }
            else if (Nullable.GetUnderlyingType(expression.Type) == enumType)
            {
                return Expression.Convert(expression, typeof(Nullable<>).MakeGenericType(enumUnderlyingType));
            }
            else
            {
                throw Error.NotSupported(SRResources.ConvertToEnumFailed, enumType, expression.Type);
            }
        }

        // Extract the constant that would have been encapsulated into LinqParameterContainer if this
        // expression represents it else return null.
        private static object ExtractParameterizedConstant(Expression expression)
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

        private static IEnumerable<Expression> ExtractValueFromNullableArguments(IEnumerable<Expression> arguments)
        {
            return arguments.Select(arg => ExtractValueFromNullableExpression(arg));
        }

        private static Expression ExtractValueFromNullableExpression(Expression source)
        {
            return Nullable.GetUnderlyingType(source.Type) != null ? Expression.Property(source, "Value") : source;
        }

        private static Expression CheckIfArgumentsAreNull(Expression[] arguments)
        {
            if (arguments.Any(arg => arg == _nullConstant))
            {
                return _trueConstant;
            }

            arguments =
                arguments
                .Select(arg => CheckForNull(arg))
                .Where(arg => arg != null)
                .ToArray();

            if (arguments.Any())
            {
                return arguments
                    .Aggregate((left, right) => Expression.Or(left, right));
            }
            else
            {
                return _falseConstant;
            }
        }

        private static Expression CheckForNull(Expression expression)
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

        private static Expression Any(Expression source, Expression filter)
        {
            Contract.Assert(source != null);
            Type elementType;
            source.Type.IsCollection(out elementType);
            Contract.Assert(elementType != null);

            if (filter == null)
            {
                if (IsIQueryable(source.Type))
                {
                    return Expression.Call(null, ExpressionHelperMethods.QueryableEmptyAnyGeneric.MakeGenericMethod(elementType), source);
                }
                else
                {
                    return Expression.Call(null, ExpressionHelperMethods.EnumerableEmptyAnyGeneric.MakeGenericMethod(elementType), source);
                }
            }
            else
            {
                if (IsIQueryable(source.Type))
                {
                    return Expression.Call(null, ExpressionHelperMethods.QueryableNonEmptyAnyGeneric.MakeGenericMethod(elementType), source, filter);
                }
                else
                {
                    return Expression.Call(null, ExpressionHelperMethods.EnumerableNonEmptyAnyGeneric.MakeGenericMethod(elementType), source, filter);
                }
            }
        }

        private static Expression All(Expression source, Expression filter)
        {
            Contract.Assert(source != null);
            Contract.Assert(filter != null);

            Type elementType;
            source.Type.IsCollection(out elementType);
            Contract.Assert(elementType != null);

            if (IsIQueryable(source.Type))
            {
                return Expression.Call(null, ExpressionHelperMethods.QueryableAllGeneric.MakeGenericMethod(elementType), source, filter);
            }
            else
            {
                return Expression.Call(null, ExpressionHelperMethods.EnumerableAllGeneric.MakeGenericMethod(elementType), source, filter);
            }
        }

        private static bool IsNullable(Type t)
        {
            if (!t.IsValueType || (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>)))
            {
                return true;
            }

            return false;
        }

        private static Type ToNullable(Type t)
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

        private static Expression ToNullable(Expression expression)
        {
            if (!IsNullable(expression.Type))
            {
                return Expression.Convert(expression, ToNullable(expression.Type));
            }

            return expression;
        }

        private static bool IsIQueryable(Type type)
        {
            return typeof(IQueryable).IsAssignableFrom(type);
        }

        private static bool IsDoubleOrDecimal(Type type)
        {
            return IsType<double>(type) || IsType<decimal>(type);
        }

        private static bool IsDate(Type type)
        {
            return IsType<DateTime>(type);
        }

        private static bool IsDateOrOffset(Type type)
        {
            return IsType<DateTime>(type) || IsType<DateTimeOffset>(type);
        }

        private static bool IsInteger(Type type)
        {
            return IsType<short>(type) || IsType<int>(type) || IsType<long>(type);
        }

        private static bool IsType<T>(Type type) where T : struct
        {
            return type == typeof(T) || type == typeof(T?);
        }

        private static Expression ConvertNull(Expression expression, Type type)
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
