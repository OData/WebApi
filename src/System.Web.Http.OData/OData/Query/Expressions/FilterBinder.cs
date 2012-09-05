// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Http.Dispatcher;
using System.Web.Http.OData.Builder.Conventions;
using System.Web.Http.OData.Formatter;
using System.Web.Http.OData.Properties;
using Microsoft.Data.Edm;
using Microsoft.Data.OData.Query;
using Microsoft.Data.OData.Query.SemanticAst;

namespace System.Web.Http.OData.Query.Expressions
{
    /// <summary>
    /// Translates an OData $filter parse tree represented by <see cref="FilterQueryNode"/> to 
    /// an <see cref="Expression"/> and applies it to an <see cref="IQueryable"/>.
    /// </summary>
    internal class FilterBinder
    {
        private const string ODataItParameterName = "$it";

        private static readonly Expression _nullConstant = Expression.Constant(null);
        private static readonly Expression _falseConstant = Expression.Constant(false);
        private static readonly Expression _trueConstant = Expression.Constant(true);

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

        private bool _nullPropagation;
        private IAssembliesResolver _assembliesResolver;

        private FilterBinder(IEdmModel model, IAssembliesResolver assembliesResolver, bool handleNullPropagation)
        {
            Contract.Assert(model != null);
            Contract.Assert(assembliesResolver != null);

            _nullPropagation = handleNullPropagation;
            _parametersStack = new Stack<Dictionary<string, ParameterExpression>>();
            _model = model;
            _assembliesResolver = assembliesResolver;
        }

        public static Expression<Func<TEntityType, bool>> Bind<TEntityType>(FilterQueryNode filterNode, IEdmModel model, IAssembliesResolver assembliesResolver, bool handleNullPropagation)
        {
            return Bind(filterNode, typeof(TEntityType), model, assembliesResolver, handleNullPropagation) as Expression<Func<TEntityType, bool>>;
        }

        public static Expression Bind(FilterQueryNode filterNode, Type filterType, IEdmModel model, IAssembliesResolver assembliesResolver, bool handleNullPropagation)
        {
            if (filterNode == null)
            {
                throw Error.ArgumentNull("filterNode");
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

            FilterBinder binder = new FilterBinder(model, assembliesResolver, handleNullPropagation);
            Expression filter = binder.BindFilterQueryNode(filterNode);

            Type expectedFilterType = typeof(Func<,>).MakeGenericType(filterType, typeof(bool));
            if (filter.Type != expectedFilterType)
            {
                throw Error.InvalidOperation(SRResources.CannotCastFilter, filter.Type.FullName, expectedFilterType.FullName);
            }

            return filter;
        }

        private Expression Bind(QueryNode node)
        {
            CollectionQueryNode collectionNode = node as CollectionQueryNode;
            SingleValueQueryNode singleValueNode = node as SingleValueQueryNode;

            if (collectionNode != null)
            {
                switch (node.Kind)
                {
                    case QueryNodeKind.Filter:
                        return BindFilterQueryNode(node as FilterQueryNode);

                    case QueryNodeKind.Segment:
                        CollectionNavigationNode navigationNode = node as CollectionNavigationNode;
                        return BindNavigationPropertyNode(navigationNode.Source, navigationNode.NavigationProperty());

                    default:
                        throw Error.NotSupported(SRResources.QueryNodeBindingNotSupported, node.Kind, typeof(FilterBinder).Name);
                }
            }
            else if (singleValueNode != null)
            {
                switch (node.Kind)
                {
                    case QueryNodeKind.BinaryOperator:
                        return BindBinaryOperatorQueryNode(node as BinaryOperatorQueryNode);

                    case QueryNodeKind.Constant:
                        return BindConstantQueryNode(node as ConstantQueryNode);

                    case QueryNodeKind.Convert:
                        return BindConvertQueryNode(node as ConvertQueryNode);

                    case QueryNodeKind.Parameter:
                        return BindParameterQueryNode(node as ParameterQueryNode);

                    case QueryNodeKind.PropertyAccess:
                        return BindPropertyAccessQueryNode(node as PropertyAccessQueryNode);

                    case QueryNodeKind.UnaryOperator:
                        return BindUnaryOperatorQueryNode(node as UnaryOperatorQueryNode);

                    case QueryNodeKind.SingleValueFunctionCall:
                        return BindSingleValueFunctionCallQueryNode(node as SingleValueFunctionCallQueryNode);

                    case QueryNodeKind.Segment:
                        SingletonNavigationNode navigationNode = node as SingletonNavigationNode;
                        return BindNavigationPropertyNode(navigationNode.Source, navigationNode.NavigationProperty);

                    case QueryNodeKind.Any:
                        return BindAnyQueryNode(node as AnyQueryNode);

                    case QueryNodeKind.All:
                        return BindAllQueryNode(node as AllQueryNode);

                    default:
                        throw Error.NotSupported(SRResources.QueryNodeBindingNotSupported, node.Kind, typeof(FilterBinder).Name);
                }
            }
            else
            {
                throw Error.NotSupported(SRResources.QueryNodeBindingNotSupported, node.Kind, typeof(FilterBinder).Name);
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

            return CreatePropertyAccessExpression(source, navigationProperty.Name);
        }

        private Expression BindBinaryOperatorQueryNode(BinaryOperatorQueryNode binaryOperatorNode)
        {
            Expression left = Bind(binaryOperatorNode.Left);
            Expression right = Bind(binaryOperatorNode.Right);

            // handle null propagation only if either of the operands can be null
            bool isNullPropagationRequired = _nullPropagation && (IsNullable(left.Type) || IsNullable(right.Type));
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

        private Expression BindConstantQueryNode(ConstantQueryNode constantNode)
        {
            Contract.Assert(constantNode != null);

            if (constantNode.Value == null)
            {
                return _nullConstant;
            }

            return Expression.Constant(constantNode.Value, EdmLibHelpers.GetClrType(constantNode.TypeReference, _model, _assembliesResolver));
        }

        private Expression BindConvertQueryNode(ConvertQueryNode convertQueryNode)
        {
            Contract.Assert(convertQueryNode != null);
            Contract.Assert(convertQueryNode.TypeReference != null);

            Expression source = Bind(convertQueryNode.Source);

            Type conversionType = EdmLibHelpers.GetClrType(convertQueryNode.TypeReference, _model, _assembliesResolver);

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
                return Expression.Convert(source, conversionType);
            }
        }

        private Expression BindFilterQueryNode(FilterQueryNode filterNode)
        {
            Type filterType = EdmLibHelpers.GetClrType(filterNode.ItemType, _model, _assembliesResolver);
            ParameterExpression filterParameter = Expression.Parameter(filterType, filterNode.Parameter.Name);
            _lambdaParameters = new Dictionary<string, ParameterExpression>();
            _lambdaParameters.Add(filterNode.Parameter.Name, filterParameter);

            Expression body = Bind(filterNode.Expression);

            body = ApplyNullPropagationForFilterBody(body);

            Expression filterExpression = Expression.Lambda(body, filterParameter);
            if (_parametersStack.Count != 0)
            {
                _lambdaParameters = _parametersStack.Pop();
            }
            else
            {
                _lambdaParameters = null;
            }

            return filterExpression;
        }

        private Expression ApplyNullPropagationForFilterBody(Expression body)
        {
            if (IsNullable(body.Type))
            {
                if (_nullPropagation)
                {
                    // handle null as false
                    // if(body == null) return false; else return body;
                    body = Expression.Condition(
                                test: Expression.Equal(body, Expression.Constant(null, typeof(bool?))),
                                ifTrue: _falseConstant,
                                ifFalse: Expression.Convert(body, typeof(bool)),
                                type: typeof(bool));
                }
                else
                {
                    body = Expression.Convert(body, typeof(bool));
                }
            }

            return body;
        }

        private Expression BindParameterQueryNode(ParameterQueryNode parameterNode)
        {
            return _lambdaParameters[parameterNode.Name];
        }

        private Expression BindPropertyAccessQueryNode(PropertyAccessQueryNode propertyAccessNode)
        {
            Expression source = Bind(propertyAccessNode.Source);
            return CreatePropertyAccessExpression(source, propertyAccessNode.Property.Name);
        }

        private Expression CreatePropertyAccessExpression(Expression source, string propertyName)
        {
            Expression propertyAccessExpression = Expression.Property(source, propertyName);

            if (_nullPropagation && IsNullable(source.Type) && source != _lambdaParameters[ODataItParameterName])
            {
                // source.property => source == null ? null : [CastToNullable]source.property
                return
                    Expression.Condition(
                        test: Expression.Equal(source, _nullConstant),
                        ifTrue: Expression.Constant(null, ToNullable(propertyAccessExpression.Type)),
                        ifFalse: ToNullable(propertyAccessExpression));
            }
            else
            {
                return propertyAccessExpression;
            }
        }

        private Expression BindUnaryOperatorQueryNode(UnaryOperatorQueryNode unaryOperatorQueryNode)
        {
            // No need to handle null-propagation here as CLR already handles it.
            // !(null) = null
            // -(null) = null
            Expression inner = Bind(unaryOperatorQueryNode.Operand);
            switch (unaryOperatorQueryNode.OperatorKind)
            {
                case UnaryOperatorKind.Negate:
                    return Expression.Negate(inner);

                case UnaryOperatorKind.Not:
                    return Expression.Not(inner);

                default:
                    throw Error.NotSupported(SRResources.QueryNodeBindingNotSupported, unaryOperatorQueryNode.Kind, typeof(FilterBinder).Name);
            }
        }

        private Expression BindSingleValueFunctionCallQueryNode(SingleValueFunctionCallQueryNode node)
        {
            switch (node.Name)
            {
                case "startswith":
                    return BindStartsWith(node);

                case "endswith":
                    return BindEndsWith(node);

                case "substringof":
                    return BindSubstringOf(node);

                case "substring":
                    return BindSubstring(node);

                case "length":
                    return BindLength(node);

                case "indexof":
                    return BindIndexOf(node);

                case "tolower":
                    return BindToLower(node);

                case "toupper":
                    return BindToUpper(node);

                case "trim":
                    return BindTrim(node);

                case "concat":
                    return BindConcat(node);

                case "year":
                case "month":
                case "day":
                case "hour":
                case "minute":
                case "second":
                    return BindDateProperty(node);

                case "round":
                    return BindRound(node);

                case "floor":
                    return BindFloor(node);

                case "ceiling":
                    return BindCeiling(node);

                default:
                    throw new NotImplementedException(Error.Format(SRResources.ODataFunctionNotSupported, node.Name));
            }
        }

        private Expression CreateFunctionCallWithNullPropagation(Expression functionCall, Expression[] arguments)
        {
            if (_nullPropagation)
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

        private Expression BindCeiling(SingleValueFunctionCallQueryNode node)
        {
            Contract.Assert("ceiling" == node.Name);

            Expression[] arguments = BindArguments(node.Arguments);

            Contract.Assert(arguments.Length == 1 && IsDoubleOrDecimal(arguments[0].Type));

            Expression functionCall = Expression.Call(null, ClrCanonicalFunctions.Ceiling, ExtractValueFromNullableArguments(arguments));

            return CreateFunctionCallWithNullPropagation(functionCall, arguments);
        }

        private Expression BindFloor(SingleValueFunctionCallQueryNode node)
        {
            Contract.Assert("floor" == node.Name);

            Expression[] arguments = BindArguments(node.Arguments);

            Contract.Assert(arguments.Length == 1 && IsDoubleOrDecimal(arguments[0].Type));

            Expression functionCall = Expression.Call(null, ClrCanonicalFunctions.Floor, ExtractValueFromNullableArguments(arguments));
            return CreateFunctionCallWithNullPropagation(functionCall, arguments);
        }

        private Expression BindRound(SingleValueFunctionCallQueryNode node)
        {
            Contract.Assert("round" == node.Name);

            Expression[] arguments = BindArguments(node.Arguments);

            Contract.Assert(arguments.Length == 1 && IsDoubleOrDecimal(arguments[0].Type));

            Expression functionCall = Expression.Call(null, ClrCanonicalFunctions.Round, ExtractValueFromNullableArguments(arguments));
            return CreateFunctionCallWithNullPropagation(functionCall, arguments);
        }

        private Expression BindDateProperty(SingleValueFunctionCallQueryNode node)
        {
            Contract.Assert(new string[] { "year", "month", "day", "hour", "minute", "second" }.Contains(node.Name));

            Expression[] arguments = BindArguments(node.Arguments);

            Contract.Assert(arguments.Length == 1 && IsDateOrOffset(arguments[0].Type));

            string propertyName = node.Name.Substring(0, 1).ToUpperInvariant() + node.Name.Substring(1);

            Expression functionCall = Expression.Property(ExtractValueFromNullableArguments(arguments)[0], propertyName);

            return CreateFunctionCallWithNullPropagation(functionCall, arguments);
        }

        private Expression BindConcat(SingleValueFunctionCallQueryNode node)
        {
            Contract.Assert("concat" == node.Name);

            Expression[] arguments = BindArguments(node.Arguments);

            Contract.Assert(arguments.Length == 2 && arguments[0].Type == typeof(string) && arguments[1].Type == typeof(string));

            Expression functionCall = Expression.Call(null, ClrCanonicalFunctions.Concat, arguments);
            return CreateFunctionCallWithNullPropagation(functionCall, arguments);
        }

        private Expression BindTrim(SingleValueFunctionCallQueryNode node)
        {
            Contract.Assert("trim" == node.Name);

            Expression[] arguments = BindArguments(node.Arguments);

            Contract.Assert(arguments.Length == 1 && arguments[0].Type == typeof(string));

            Expression functionCall = Expression.Call(arguments[0], ClrCanonicalFunctions.Trim);
            return CreateFunctionCallWithNullPropagation(functionCall, arguments);
        }

        private Expression BindToUpper(SingleValueFunctionCallQueryNode node)
        {
            Contract.Assert("toupper" == node.Name);

            Expression[] arguments = BindArguments(node.Arguments);

            Contract.Assert(arguments.Length == 1 && arguments[0].Type == typeof(string));

            Expression functionCall = Expression.Call(arguments[0], ClrCanonicalFunctions.ToUpper);
            return CreateFunctionCallWithNullPropagation(functionCall, arguments);
        }

        private Expression BindToLower(SingleValueFunctionCallQueryNode node)
        {
            Contract.Assert("tolower" == node.Name);

            Expression[] arguments = BindArguments(node.Arguments);

            Contract.Assert(arguments.Length == 1 && arguments[0].Type == typeof(string));

            Expression functionCall = Expression.Call(arguments[0], ClrCanonicalFunctions.ToLower);
            return CreateFunctionCallWithNullPropagation(functionCall, arguments);
        }

        private Expression BindIndexOf(SingleValueFunctionCallQueryNode node)
        {
            Contract.Assert("indexof" == node.Name);

            Expression[] arguments = BindArguments(node.Arguments);

            Contract.Assert(arguments.Length == 2 && arguments[0].Type == typeof(string) && arguments[1].Type == typeof(string));

            Expression functionCall = Expression.Call(arguments[0], ClrCanonicalFunctions.IndexOf, arguments[1]);
            return CreateFunctionCallWithNullPropagation(functionCall, arguments);
        }

        private Expression BindSubstring(SingleValueFunctionCallQueryNode node)
        {
            Contract.Assert("substring" == node.Name);

            Expression[] arguments = BindArguments(node.Arguments);

            Contract.Assert((arguments.Length == 2 && arguments[0].Type == typeof(string) && IsInteger(arguments[1].Type)) ||
                (arguments.Length == 3 && arguments[0].Type == typeof(string) && IsInteger(arguments[1].Type) && IsInteger(arguments[2].Type)));

            Expression functionCall;
            if (arguments.Length == 2)
            {
                // When null propagation is allowed, we use a safe version of String.Substring(int).
                // But for providers that would not recognize custom expressions like this, we map 
                // directly to String.Substring(int)
                if (_nullPropagation)
                {
                    // Safe function is static and takes string "this" as first argument
                    functionCall = Expression.Call(null, ClrCanonicalFunctions.SubstringStartNoThrow, arguments[0], arguments[1]);
                }
                else
                {
                    functionCall = Expression.Call(arguments[0], ClrCanonicalFunctions.SubstringStart, arguments[1]);
                }
            }
            else
            {
                // arguments.Length == 3 implies String.Substring(int, int)

                // When null propagation is allowed, we use a safe version of String.Substring(int, int).
                // But for providers that would not recognize custom expressions like this, we map 
                // directly to String.Substring(int, int)
                if (_nullPropagation)
                {
                    // Safe function is static and takes string "this" as first argument
                    functionCall = Expression.Call(null, ClrCanonicalFunctions.SubstringStartAndLengthNoThrow, arguments[0], arguments[1], arguments[2]);
                }
                else
                {
                    functionCall = Expression.Call(arguments[0], ClrCanonicalFunctions.SubstringStartAndLength, arguments[1], arguments[2]);
                }
            }

            return CreateFunctionCallWithNullPropagation(functionCall, arguments);
        }

        private Expression BindLength(SingleValueFunctionCallQueryNode node)
        {
            Contract.Assert("length" == node.Name);

            Expression[] arguments = BindArguments(node.Arguments);

            Contract.Assert(arguments.Length == 1 && arguments[0].Type == typeof(string));

            Expression functionCall = Expression.Property(arguments[0], "Length");
            return CreateFunctionCallWithNullPropagation(functionCall, arguments);
        }

        private Expression BindSubstringOf(SingleValueFunctionCallQueryNode node)
        {
            Contract.Assert("substringof" == node.Name);

            Expression[] arguments = BindArguments(node.Arguments);

            Contract.Assert(arguments.Length == 2 && arguments[0].Type == typeof(string) && arguments[1].Type == typeof(string));

            // NOTE: this is reversed because it is reverse in WCF DS and in the OData spec
            Expression functionCall = Expression.Call(arguments[1], ClrCanonicalFunctions.Contains, arguments[0]);
            return CreateFunctionCallWithNullPropagation(functionCall, arguments);
        }

        private Expression BindStartsWith(SingleValueFunctionCallQueryNode node)
        {
            Contract.Assert("startswith" == node.Name);

            Expression[] arguments = BindArguments(node.Arguments);

            Contract.Assert(arguments.Length == 2 && arguments[0].Type == typeof(string) && arguments[1].Type == typeof(string));

            Expression functionCall = Expression.Call(arguments[0], ClrCanonicalFunctions.StartsWith, arguments[1]);
            return CreateFunctionCallWithNullPropagation(functionCall, arguments);
        }

        private Expression BindEndsWith(SingleValueFunctionCallQueryNode node)
        {
            Contract.Assert("endswith" == node.Name);

            Expression[] arguments = BindArguments(node.Arguments);

            Contract.Assert(arguments.Length == 2 && arguments[0].Type == typeof(string) && arguments[1].Type == typeof(string));

            Expression functionCall = Expression.Call(arguments[0], ClrCanonicalFunctions.EndsWith, arguments[1]);
            return CreateFunctionCallWithNullPropagation(functionCall, arguments);
        }

        private Expression[] BindArguments(IEnumerable<QueryNode> nodes)
        {
            return nodes.OfType<SingleValueQueryNode>().Select(n => Bind(n)).ToArray();
        }

        private Expression BindAllQueryNode(AllQueryNode allQueryNode)
        {
            ParameterExpression allIt = HandleLambdaParameters(allQueryNode.Parameters);

            Expression source;
            Contract.Assert(allQueryNode.Source != null);
            source = Bind(allQueryNode.Source);

            Expression body = source;
            Contract.Assert(allQueryNode.Body != null);

            body = Bind(allQueryNode.Body);
            body = ApplyNullPropagationForFilterBody(body);
            body = Expression.Lambda(body, allIt);

            Expression all = All(source, body);

            if (_nullPropagation && IsNullable(source.Type))
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

        private Expression BindAnyQueryNode(AnyQueryNode anyQueryNode)
        {
            ParameterExpression anyIt = HandleLambdaParameters(anyQueryNode.Parameters);

            Expression source;
            Contract.Assert(anyQueryNode.Source != null);
            source = Bind(anyQueryNode.Source);

            Expression body = null;
            // uri parser places an Constant node with value true for empty any() body
            if (anyQueryNode.Body != null && anyQueryNode.Body.Kind != QueryNodeKind.Constant)
            {
                body = Bind(anyQueryNode.Body);
                body = ApplyNullPropagationForFilterBody(body);
                body = Expression.Lambda(body, anyIt);
            }

            Expression any = Any(source, body);

            if (_nullPropagation && IsNullable(source.Type))
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

        private ParameterExpression HandleLambdaParameters(IEnumerable<ParameterQueryNode> parameters)
        {
            ParameterExpression lambdaIt = null;

            Contract.Assert(_lambdaParameters != null);
            _parametersStack.Push(_lambdaParameters);

            Dictionary<string, ParameterExpression> newParameters = new Dictionary<string, ParameterExpression>();
            foreach (ParameterQueryNode parameterNode in parameters)
            {
                ParameterExpression parameter;
                if (!_lambdaParameters.TryGetValue(parameterNode.Name, out parameter))
                {
                    // Work-around issue 481323 where UriParser yields a collection parameter type
                    // for primitive collections rather than the inner element type of the collection.
                    // Remove this block of code when 481323 is resolved.
                    IEdmTypeReference edmTypeReference = parameterNode.ParameterType;
                    IEdmCollectionTypeReference collectionTypeReference = edmTypeReference as IEdmCollectionTypeReference;
                    if (collectionTypeReference != null)
                    {
                        IEdmCollectionType collectionType = collectionTypeReference.Definition as IEdmCollectionType;
                        if (collectionType != null)
                        {
                            edmTypeReference = collectionType.ElementType;
                        }
                    }

                    parameter = Expression.Parameter(EdmLibHelpers.GetClrType(edmTypeReference, _model, _assembliesResolver), parameterNode.Name);
                    Contract.Assert(lambdaIt == null, "There can be only one parameter in an Any/All lambda");
                    lambdaIt = parameter;
                }
                newParameters.Add(parameterNode.Name, parameter);
            }

            _lambdaParameters = newParameters;
            return lambdaIt;
        }

        private static Expression CreateBinaryExpression(BinaryOperatorKind binaryOperator, Expression left, Expression right, bool liftToNull)
        {
            ExpressionType binaryExpressionType;
            if (_binaryOperatorMapping.TryGetValue(binaryOperator, out binaryExpressionType))
            {
                return Expression.MakeBinary(binaryExpressionType, left, right, liftToNull, method: null);
            }
            else
            {
                throw Error.NotSupported(SRResources.QueryNodeBindingNotSupported, binaryOperator, typeof(FilterBinder).Name);
            }
        }

        private static Expression[] ExtractValueFromNullableArguments(Expression[] arguments)
        {
            return arguments.Select(arg => IsNullable(arg.Type) ? Expression.Property(arg, "Value") : arg).ToArray();
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
    }
}
