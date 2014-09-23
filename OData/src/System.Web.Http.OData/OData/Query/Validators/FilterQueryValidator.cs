// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Web.Http.OData.Properties;
using System.Web.Http.OData.Query.Expressions;
using Microsoft.Data.Edm;
using Microsoft.Data.OData;
using Microsoft.Data.OData.Query;
using Microsoft.Data.OData.Query.SemanticAst;

namespace System.Web.Http.OData.Query.Validators
{
    /// <summary>
    /// Represents a validator used to validate a <see cref="FilterQueryOption" /> based on the <see cref="ODataValidationSettings"/>.
    /// </summary>
    /// <remarks>
    /// Please note this class is not thread safe.
    /// </remarks>
    public class FilterQueryValidator
    {
        private int _currentAnyAllExpressionDepth;
        private int _currentNodeCount;

        /// <summary>
        /// Validates a <see cref="FilterQueryOption" />.
        /// </summary>
        /// <param name="filterQueryOption">The $filter query.</param>
        /// <param name="settings">The validation settings.</param>
        /// <remarks>
        /// Please note this method is not thread safe.
        /// </remarks>
        public virtual void Validate(FilterQueryOption filterQueryOption, ODataValidationSettings settings)
        {
            if (filterQueryOption == null)
            {
                throw Error.ArgumentNull("filterQueryOption");
            }

            if (settings == null)
            {
                throw Error.ArgumentNull("settings");
            }

            _currentAnyAllExpressionDepth = 0;
            _currentNodeCount = 0;

            ValidateQueryNode(filterQueryOption.FilterClause.Expression, settings);
        }

        /// <summary>
        /// Override this method to restrict the 'all' query inside the filter query.
        /// </summary>
        /// <remarks>
        /// This method is intended to be called from method overrides in subclasses. This method also supports unit-testing scenarios and is not intended to be called from user code.
        /// Call the Validate method to validate a <see cref="FilterQueryOption"/> instance.
        /// </remarks>
        /// <param name="allNode"></param>
        /// <param name="settings"></param>
        public virtual void ValidateAllNode(AllNode allNode, ODataValidationSettings settings)
        {
            if (allNode == null)
            {
                throw Error.ArgumentNull("allNode");
            }

            if (settings == null)
            {
                throw Error.ArgumentNull("settings");
            }

            ValidateFunction("all", settings);
            EnterLambda(settings);

            try
            {
                ValidateQueryNode(allNode.Source, settings);

                ValidateQueryNode(allNode.Body, settings);
            }
            finally
            {
                ExitLambda();
            }
        }

        /// <summary>
        /// Override this method to restrict the 'any' query inside the filter query.
        /// </summary>
        /// <remarks>
        /// This method is intended to be called from method overrides in subclasses. This method also supports unit-testing scenarios and is not intended to be called from user code.
        /// Call the Validate method to validate a <see cref="FilterQueryOption"/> instance.
        /// </remarks>
        /// <param name="anyNode"></param>
        /// <param name="settings"></param>
        public virtual void ValidateAnyNode(AnyNode anyNode, ODataValidationSettings settings)
        {
            if (anyNode == null)
            {
                throw Error.ArgumentNull("anyNode");
            }

            if (settings == null)
            {
                throw Error.ArgumentNull("settings");
            }

            ValidateFunction("any", settings);
            EnterLambda(settings);

            try
            {
                ValidateQueryNode(anyNode.Source, settings);

                if (anyNode.Body != null && anyNode.Body.Kind != QueryNodeKind.Constant)
                {
                    ValidateQueryNode(anyNode.Body, settings);
                }
            }
            finally
            {
                ExitLambda();
            }
        }

        /// <summary>
        /// override this method to restrict the binary operators inside the filter query. That includes all the logical operators except 'not' and all math operators.
        /// </summary>
        /// <remarks>
        /// This method is intended to be called from method overrides in subclasses. This method also supports unit-testing scenarios and is not intended to be called from user code.
        /// Call the Validate method to validate a <see cref="FilterQueryOption"/> instance.
        /// </remarks>
        /// <param name="binaryOperatorNode"></param>
        /// <param name="settings"></param>
        public virtual void ValidateBinaryOperatorNode(BinaryOperatorNode binaryOperatorNode, ODataValidationSettings settings)
        {
            if (binaryOperatorNode == null)
            {
                throw Error.ArgumentNull("binaryOperatorNode");
            }

            if (settings == null)
            {
                throw Error.ArgumentNull("settings");
            }

            // base case goes
            switch (binaryOperatorNode.OperatorKind)
            {
                case BinaryOperatorKind.Equal:
                case BinaryOperatorKind.NotEqual:
                case BinaryOperatorKind.And:
                case BinaryOperatorKind.GreaterThan:
                case BinaryOperatorKind.GreaterThanOrEqual:
                case BinaryOperatorKind.LessThan:
                case BinaryOperatorKind.LessThanOrEqual:
                case BinaryOperatorKind.Or:
                    // binary logical operators
                    ValidateLogicalOperator(binaryOperatorNode, settings);
                    break;
                default:
                    // math operators
                    ValidateArithmeticOperator(binaryOperatorNode, settings);
                    break;
            }
        }

        /// <summary>
        /// Override this method to validate the LogicalOperators such as 'eq', 'ne', 'gt', 'ge', 'lt', 'le', 'and', 'or'.
        /// 
        /// Please note that 'not' is not included here. Please override ValidateUnaryOperatorNode to customize 'not'.
        /// </summary>
        /// <remarks>
        /// This method is intended to be called from method overrides in subclasses. This method also supports unit-testing scenarios and is not intended to be called from user code.
        /// Call the Validate method to validate a <see cref="FilterQueryOption"/> instance.
        /// </remarks>
        /// <param name="binaryNode"></param>
        /// <param name="settings"></param>
        public virtual void ValidateLogicalOperator(BinaryOperatorNode binaryNode, ODataValidationSettings settings)
        {
            if (binaryNode == null)
            {
                throw Error.ArgumentNull("binaryNode");
            }

            if (settings == null)
            {
                throw Error.ArgumentNull("settings");
            }

            AllowedLogicalOperators logicalOperator = ToLogicalOperator(binaryNode);

            if ((settings.AllowedLogicalOperators & logicalOperator) != logicalOperator)
            {
                // this means the given logical operator is not allowed
                throw new ODataException(Error.Format(SRResources.NotAllowedLogicalOperator, logicalOperator, "AllowedLogicalOperators"));
            }

            // recursion case goes here
            ValidateQueryNode(binaryNode.Left, settings);
            ValidateQueryNode(binaryNode.Right, settings);
        }

        /// <summary>
        /// Override this method for the Arithmetic operators, including add, sub, mul, div, mod.
        /// </summary>
        /// <remarks>
        /// This method is intended to be called from method overrides in subclasses. This method also supports unit-testing scenarios and is not intended to be called from user code.
        /// Call the Validate method to validate a <see cref="FilterQueryOption"/> instance.
        /// </remarks>
        /// <param name="binaryNode"></param>
        /// <param name="settings"></param>
        public virtual void ValidateArithmeticOperator(BinaryOperatorNode binaryNode, ODataValidationSettings settings)
        {
            if (binaryNode == null)
            {
                throw Error.ArgumentNull("binaryNode");
            }

            if (settings == null)
            {
                throw Error.ArgumentNull("settings");
            }

            AllowedArithmeticOperators arithmeticOperator = ToArithmeticOperator(binaryNode);

            if ((settings.AllowedArithmeticOperators & arithmeticOperator) != arithmeticOperator)
            {
                // this means the given logical operator is not allowed
                throw new ODataException(Error.Format(SRResources.NotAllowedArithmeticOperator, arithmeticOperator, "AllowedArithmeticOperators"));
            }

            // recursion case goes here
            ValidateQueryNode(binaryNode.Left, settings);
            ValidateQueryNode(binaryNode.Right, settings);
        }

        /// <summary>
        /// Override this method to restrict the 'constant' inside the filter query.
        /// </summary>
        /// <remarks>
        /// This method is intended to be called from method overrides in subclasses. This method also supports unit-testing scenarios and is not intended to be called from user code.
        /// Call the Validate method to validate a <see cref="FilterQueryOption"/> instance.
        /// </remarks>
        /// <param name="constantNode"></param>
        /// <param name="settings"></param>
        public virtual void ValidateConstantNode(ConstantNode constantNode, ODataValidationSettings settings)
        {
            if (constantNode == null)
            {
                throw Error.ArgumentNull("constantNode");
            }

            if (settings == null)
            {
                throw Error.ArgumentNull("settings");
            }

            // No default validation logic here.
        }

        /// <summary>
        /// Override this method to restrict the 'cast' inside the filter query.
        /// </summary>
        /// <remarks>
        /// This method is intended to be called from method overrides in subclasses. This method also supports unit-testing scenarios and is not intended to be called from user code.
        /// Call the Validate method to validate a <see cref="FilterQueryOption"/> instance.
        /// </remarks>
        /// <param name="convertNode"></param>
        /// <param name="settings"></param>
        public virtual void ValidateConvertNode(ConvertNode convertNode, ODataValidationSettings settings)
        {
            if (convertNode == null)
            {
                throw Error.ArgumentNull("convertNode");
            }

            if (settings == null)
            {
                throw Error.ArgumentNull("settings");
            }

            // Validate child nodes but not the ConvertNode itself.
            ValidateQueryNode(convertNode.Source, settings);
        }

        /// <summary>
        /// Override this method for the navigation property node.
        /// </summary>
        /// <remarks>
        /// This method is intended to be called from method overrides in subclasses. This method also supports unit-testing scenarios and is not intended to be called from user code.
        /// Call the Validate method to validate a <see cref="FilterQueryOption"/> instance.
        /// </remarks>
        /// <param name="sourceNode"></param>
        /// <param name="navigationProperty"></param>
        /// <param name="settings"></param>
        public virtual void ValidateNavigationPropertyNode(QueryNode sourceNode, IEdmNavigationProperty navigationProperty, ODataValidationSettings settings)
        {
            if (settings == null)
            {
                throw Error.ArgumentNull("settings");
            }

            // No default validation logic here.

            // recursion
            if (sourceNode != null)
            {
                ValidateQueryNode(sourceNode, settings);
            }
        }

        /// <summary>
        /// Override this method to validate the parameter used in the filter query.
        /// </summary>
        /// <remarks>
        /// This method is intended to be called from method overrides in subclasses. This method also supports unit-testing scenarios and is not intended to be called from user code.
        /// Call the Validate method to validate a <see cref="FilterQueryOption"/> instance.
        /// </remarks>
        /// <param name="rangeVariable"></param>
        /// <param name="settings"></param>
        public virtual void ValidateRangeVariable(RangeVariable rangeVariable, ODataValidationSettings settings)
        {
            if (rangeVariable == null)
            {
                throw Error.ArgumentNull("rangeVariable");
            }

            if (settings == null)
            {
                throw Error.ArgumentNull("settings");
            }

            // No default validation logic here.
        }

        /// <summary>
        /// Override this method to validate property accessor.
        /// </summary>
        /// <remarks>
        /// This method is intended to be called from method overrides in subclasses. This method also supports unit-testing scenarios and is not intended to be called from user code.
        /// Call the Validate method to validate a <see cref="FilterQueryOption"/> instance.
        /// </remarks>
        /// <param name="propertyAccessNode"></param>
        /// <param name="settings"></param>
        public virtual void ValidateSingleValuePropertyAccessNode(SingleValuePropertyAccessNode propertyAccessNode, ODataValidationSettings settings)
        {
            if (propertyAccessNode == null)
            {
                throw Error.ArgumentNull("propertyAccessNode");
            }

            if (settings == null)
            {
                throw Error.ArgumentNull("settings");
            }

            // No default validation logic here.
            ValidateQueryNode(propertyAccessNode.Source, settings);
        }

        /// <summary>
        /// Override this method to validate collection property accessor.
        /// </summary>
        /// <remarks>
        /// This method is intended to be called from method overrides in subclasses. This method also supports unit-testing scenarios and is not intended to be called from user code.
        /// Call the Validate method to validate a <see cref="FilterQueryOption"/> instance.
        /// </remarks>
        /// <param name="propertyAccessNode"></param>
        /// <param name="settings"></param>
        public virtual void ValidateCollectionPropertyAccessNode(CollectionPropertyAccessNode propertyAccessNode, ODataValidationSettings settings)
        {
            if (propertyAccessNode == null)
            {
                throw Error.ArgumentNull("propertyAccessNode");
            }

            if (settings == null)
            {
                throw Error.ArgumentNull("settings");
            }

            // No default validation logic here.
            ValidateQueryNode(propertyAccessNode.Source, settings);
        }

        /// <summary>
        /// Override this method to validate Function calls, such as 'length', 'years', etc.
        /// </summary>
        /// <remarks>
        /// This method is intended to be called from method overrides in subclasses. This method also supports unit-testing scenarios and is not intended to be called from user code.
        /// Call the Validate method to validate a <see cref="FilterQueryOption"/> instance.
        /// </remarks>
        /// <param name="node"></param>
        /// <param name="settings"></param>
        public virtual void ValidateSingleValueFunctionCallNode(SingleValueFunctionCallNode node, ODataValidationSettings settings)
        {
            if (node == null)
            {
                throw Error.ArgumentNull("node");
            }

            if (settings == null)
            {
                throw Error.ArgumentNull("settings");
            }

            ValidateFunction(node.Name, settings);

            foreach (QueryNode argumentNode in node.Arguments)
            {
                ValidateQueryNode(argumentNode, settings);
            }
        }

        /// <summary>
        /// Override this method to validate the Not operator.
        /// </summary>
        /// <remarks>
        /// This method is intended to be called from method overrides in subclasses. This method also supports unit-testing scenarios and is not intended to be called from user code.
        /// Call the Validate method to validate a <see cref="FilterQueryOption"/> instance.
        /// </remarks>
        /// <param name="unaryOperatorNode"></param>
        /// <param name="settings"></param>
        public virtual void ValidateUnaryOperatorNode(UnaryOperatorNode unaryOperatorNode, ODataValidationSettings settings)
        {
            ValidateQueryNode(unaryOperatorNode.Operand, settings);

            switch (unaryOperatorNode.OperatorKind)
            {
                case UnaryOperatorKind.Negate:
                case UnaryOperatorKind.Not:
                    if ((settings.AllowedLogicalOperators & AllowedLogicalOperators.Not) != AllowedLogicalOperators.Not)
                    {
                        throw new ODataException(Error.Format(SRResources.NotAllowedLogicalOperator, unaryOperatorNode.OperatorKind, "AllowedLogicalOperators"));
                    }
                    break;

                default:
                    throw Error.NotSupported(SRResources.UnaryNodeValidationNotSupported, unaryOperatorNode.OperatorKind, typeof(FilterQueryValidator).Name);
            }
        }

        /// <summary>
        /// Override this method if you want to visit each query node. 
        /// </summary>
        /// <remarks>
        /// This method is intended to be called from method overrides in subclasses. This method also supports unit-testing scenarios and is not intended to be called from user code.
        /// Call the Validate method to validate a <see cref="FilterQueryOption"/> instance.
        /// </remarks>
        /// <param name="node"></param>
        /// <param name="settings"></param>
        public virtual void ValidateQueryNode(QueryNode node, ODataValidationSettings settings)
        {
            // Recursion guard to avoid stack overflows
            RuntimeHelpers.EnsureSufficientExecutionStack();

            SingleValueNode singleNode = node as SingleValueNode;
            CollectionNode collectionNode = node as CollectionNode;

            IncrementNodeCount(settings);

            if (singleNode != null)
            {
                ValidateSingleValueNode(singleNode, settings);
            }
            else if (collectionNode != null)
            {
                ValidateCollectionNode(collectionNode, settings);
            }
        }

        /// <summary>
        /// Override this method if you want to validate casts on entity collections.
        /// </summary>
        /// <remarks>
        /// This method is intended to be called from method overrides in subclasses. This method also supports unit-testing scenarios and is not intended to be called from user code.
        /// Call the Validate method to validate a <see cref="FilterQueryOption"/> instance.
        /// </remarks>
        /// <param name="entityCollectionCastNode"></param>
        /// <param name="settings"></param>
        public virtual void ValidateEntityCollectionCastNode(EntityCollectionCastNode entityCollectionCastNode, ODataValidationSettings settings)
        {
            if (entityCollectionCastNode == null)
            {
                throw Error.ArgumentNull("entityCollectionCastNode");
            }

            ValidateQueryNode(entityCollectionCastNode.Source, settings);
        }

        /// <summary>
        /// Override this method if you want to validate casts on single entities.
        /// </summary>
        /// <remarks>
        /// This method is intended to be called from method overrides in subclasses. This method also supports unit-testing scenarios and is not intended to be called from user code.
        /// Call the Validate method to validate a <see cref="FilterQueryOption"/> instance.
        /// </remarks>
        /// <param name="singleEntityCastNode"></param>
        /// <param name="settings"></param>
        public virtual void ValidateSingleEntityCastNode(SingleEntityCastNode singleEntityCastNode, ODataValidationSettings settings)
        {
            if (singleEntityCastNode == null)
            {
                throw Error.ArgumentNull("singleEntityCastNode");
            }

            ValidateQueryNode(singleEntityCastNode.Source, settings);
        }

        private void EnterLambda(ODataValidationSettings validationSettings)
        {
            if (_currentAnyAllExpressionDepth >= validationSettings.MaxAnyAllExpressionDepth)
            {
                throw new ODataException(Error.Format(SRResources.MaxAnyAllExpressionLimitExceeded, validationSettings.MaxAnyAllExpressionDepth, "MaxAnyAllExpressionDepth"));
            }

            _currentAnyAllExpressionDepth++;
        }

        private void ExitLambda()
        {
            Contract.Assert(_currentAnyAllExpressionDepth > 0);
            _currentAnyAllExpressionDepth--;
        }

        private void IncrementNodeCount(ODataValidationSettings validationSettings)
        {
            if (_currentNodeCount >= validationSettings.MaxNodeCount)
            {
                throw new ODataException(Error.Format(SRResources.MaxNodeLimitExceeded, validationSettings.MaxNodeCount, "MaxNodeCount"));
            }

            _currentNodeCount++;
        }

        private void ValidateCollectionNode(CollectionNode node, ODataValidationSettings settings)
        {
            switch (node.Kind)
            {
                case QueryNodeKind.CollectionPropertyAccess:
                    CollectionPropertyAccessNode propertyAccessNode = node as CollectionPropertyAccessNode;
                    ValidateCollectionPropertyAccessNode(propertyAccessNode, settings);
                    break;

                case QueryNodeKind.CollectionNavigationNode:
                    CollectionNavigationNode navigationNode = node as CollectionNavigationNode;
                    ValidateNavigationPropertyNode(navigationNode.Source, navigationNode.NavigationProperty, settings);
                    break;

                case QueryNodeKind.EntityCollectionCast:
                    ValidateEntityCollectionCastNode(node as EntityCollectionCastNode, settings);
                    break;

                case QueryNodeKind.CollectionFunctionCall:
                case QueryNodeKind.EntityCollectionFunctionCall:
                    // Unused or have unknown uses.
                default:
                    throw Error.NotSupported(SRResources.QueryNodeValidationNotSupported, node.Kind, typeof(FilterQueryValidator).Name);
            }
        }

        /// <summary>
        /// The recursive method that validate most of the query node type is of SingleValueNode type.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="settings"></param>
        private void ValidateSingleValueNode(SingleValueNode node, ODataValidationSettings settings)
        {
            switch (node.Kind)
            {
                case QueryNodeKind.BinaryOperator:
                    ValidateBinaryOperatorNode(node as BinaryOperatorNode, settings);
                    break;

                case QueryNodeKind.Constant:
                    ValidateConstantNode(node as ConstantNode, settings);
                    break;

                case QueryNodeKind.Convert:
                    ValidateConvertNode(node as ConvertNode, settings);
                    break;

                case QueryNodeKind.EntityRangeVariableReference:
                    ValidateRangeVariable((node as EntityRangeVariableReferenceNode).RangeVariable, settings);
                    break;

                case QueryNodeKind.NonentityRangeVariableReference:
                    ValidateRangeVariable((node as NonentityRangeVariableReferenceNode).RangeVariable, settings);
                    break;

                case QueryNodeKind.SingleValuePropertyAccess:
                    ValidateSingleValuePropertyAccessNode(node as SingleValuePropertyAccessNode, settings);
                    break;

                case QueryNodeKind.UnaryOperator:
                    ValidateUnaryOperatorNode(node as UnaryOperatorNode, settings);
                    break;

                case QueryNodeKind.SingleValueFunctionCall:
                    ValidateSingleValueFunctionCallNode(node as SingleValueFunctionCallNode, settings);
                    break;

                case QueryNodeKind.SingleNavigationNode:
                    SingleNavigationNode navigationNode = node as SingleNavigationNode;
                    ValidateNavigationPropertyNode(navigationNode.Source, navigationNode.NavigationProperty, settings);
                    break;

                case QueryNodeKind.SingleEntityCast:
                    ValidateSingleEntityCastNode(node as SingleEntityCastNode, settings);
                    break;

                case QueryNodeKind.Any:
                    ValidateAnyNode(node as AnyNode, settings);
                    break;

                case QueryNodeKind.All:
                    ValidateAllNode(node as AllNode, settings);
                    break;

                case QueryNodeKind.NamedFunctionParameter:
                case QueryNodeKind.SingleValueOpenPropertyAccess:
                    // Unused or have unknown uses.
                case QueryNodeKind.SingleEntityFunctionCall:
                    // Used for some 'cast' calls but not supported here or in FilterBinder.
                default:
                    throw Error.NotSupported(SRResources.QueryNodeValidationNotSupported, node.Kind, typeof(FilterQueryValidator).Name);
            }
        }

        private static void ValidateFunction(string functionName, ODataValidationSettings settings)
        {
            AllowedFunctions convertedFunction = ToODataFunction(functionName);
            if ((settings.AllowedFunctions & convertedFunction) != convertedFunction)
            {
                // this means the given function is not allowed
                throw new ODataException(Error.Format(SRResources.NotAllowedFunction, functionName, "AllowedFunctions"));
            }
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "These are simple conversion function and cannot be split up.")]
        private static AllowedFunctions ToODataFunction(string functionName)
        {
            AllowedFunctions result = AllowedFunctions.None;

            switch (functionName)
            {
                case "any":
                    result = AllowedFunctions.Any;
                    break;
                case "all":
                    result = AllowedFunctions.All;
                    break;
                case "cast":
                    result = AllowedFunctions.Cast;
                    break;
                case ClrCanonicalFunctions.CeilingFunctionName:
                    result = AllowedFunctions.Ceiling;
                    break;
                case ClrCanonicalFunctions.ConcatFunctionName:
                    result = AllowedFunctions.Concat;
                    break;
                case ClrCanonicalFunctions.DayFunctionName:
                    result = AllowedFunctions.Day;
                    break;
                case ClrCanonicalFunctions.DaysFunctionName:
                    result = AllowedFunctions.Days;
                    break;
                case ClrCanonicalFunctions.EndswithFunctionName:
                    result = AllowedFunctions.EndsWith;
                    break;
                case ClrCanonicalFunctions.FloorFunctionName:
                    result = AllowedFunctions.Floor;
                    break;
                case ClrCanonicalFunctions.HourFunctionName:
                    result = AllowedFunctions.Hour;
                    break;
                case ClrCanonicalFunctions.HoursFunctionName:
                    result = AllowedFunctions.Hours;
                    break;
                case ClrCanonicalFunctions.IndexofFunctionName:
                    result = AllowedFunctions.IndexOf;
                    break;
                case "isof":
                    result = AllowedFunctions.IsOf;
                    break;
                case ClrCanonicalFunctions.LengthFunctionName:
                    result = AllowedFunctions.Length;
                    break;
                case ClrCanonicalFunctions.MinuteFunctionName:
                    result = AllowedFunctions.Minute;
                    break;
                case ClrCanonicalFunctions.MinutesFunctionName:
                    result = AllowedFunctions.Minutes;
                    break;
                case ClrCanonicalFunctions.MonthFunctionName:
                    result = AllowedFunctions.Month;
                    break;
                case ClrCanonicalFunctions.MonthsFunctionName:
                    result = AllowedFunctions.Months;
                    break;
                case ClrCanonicalFunctions.RoundFunctionName:
                    result = AllowedFunctions.Round;
                    break;
                case ClrCanonicalFunctions.SecondFunctionName:
                    result = AllowedFunctions.Second;
                    break;
                case ClrCanonicalFunctions.SecondsFunctionName:
                    result = AllowedFunctions.Seconds;
                    break;
                case ClrCanonicalFunctions.StartswithFunctionName:
                    result = AllowedFunctions.StartsWith;
                    break;
                case ClrCanonicalFunctions.SubstringFunctionName:
                    result = AllowedFunctions.Substring;
                    break;
                case ClrCanonicalFunctions.SubstringofFunctionName:
                    result = AllowedFunctions.SubstringOf;
                    break;
                case ClrCanonicalFunctions.TolowerFunctionName:
                    result = AllowedFunctions.ToLower;
                    break;
                case ClrCanonicalFunctions.ToupperFunctionName:
                    result = AllowedFunctions.ToUpper;
                    break;
                case ClrCanonicalFunctions.TrimFunctionName:
                    result = AllowedFunctions.Trim;
                    break;
                case ClrCanonicalFunctions.YearFunctionName:
                    result = AllowedFunctions.Year;
                    break;
                case ClrCanonicalFunctions.YearsFunctionName:
                    result = AllowedFunctions.Years;
                    break;
                default:
                    // should never be here
                    Contract.Assert(true, "ToODataFunction should never be here.");
                    break;
            }

            return result;
        }

        private static AllowedLogicalOperators ToLogicalOperator(BinaryOperatorNode binaryNode)
        {
            AllowedLogicalOperators result = AllowedLogicalOperators.None;

            switch (binaryNode.OperatorKind)
            {
                case BinaryOperatorKind.Equal:
                    result = AllowedLogicalOperators.Equal;
                    break;

                case BinaryOperatorKind.NotEqual:
                    result = AllowedLogicalOperators.NotEqual;
                    break;

                case BinaryOperatorKind.And:
                    result = AllowedLogicalOperators.And;
                    break;

                case BinaryOperatorKind.GreaterThan:
                    result = AllowedLogicalOperators.GreaterThan;
                    break;

                case BinaryOperatorKind.GreaterThanOrEqual:
                    result = AllowedLogicalOperators.GreaterThanOrEqual;
                    break;

                case BinaryOperatorKind.LessThan:
                    result = AllowedLogicalOperators.LessThan;
                    break;

                case BinaryOperatorKind.LessThanOrEqual:
                    result = AllowedLogicalOperators.LessThanOrEqual;
                    break;

                case BinaryOperatorKind.Or:
                    result = AllowedLogicalOperators.Or;
                    break;

                default:
                    // should never be here
                    Contract.Assert(false, "ToLogicalOperator should never be here.");
                    break;
            }

            return result;
        }

        private static AllowedArithmeticOperators ToArithmeticOperator(BinaryOperatorNode binaryNode)
        {
            AllowedArithmeticOperators result = AllowedArithmeticOperators.None;

            switch (binaryNode.OperatorKind)
            {
                case BinaryOperatorKind.Add:
                    result = AllowedArithmeticOperators.Add;
                    break;

                case BinaryOperatorKind.Divide:
                    result = AllowedArithmeticOperators.Divide;
                    break;

                case BinaryOperatorKind.Modulo:
                    result = AllowedArithmeticOperators.Modulo;
                    break;

                case BinaryOperatorKind.Multiply:
                    result = AllowedArithmeticOperators.Multiply;
                    break;

                case BinaryOperatorKind.Subtract:
                    result = AllowedArithmeticOperators.Subtract;
                    break;

                default:
                    // should never be here
                    Contract.Assert(false, "ToArithmeticOperator should never be here.");
                    break;
            }

            return result;
        }
    }
}