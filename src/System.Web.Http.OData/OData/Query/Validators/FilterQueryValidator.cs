// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Web.Http.OData.Properties;
using System.Web.Http.OData.Query.Expressions;
using Microsoft.Data.Edm;
using Microsoft.Data.OData;
using Microsoft.Data.OData.Query;
using Microsoft.Data.OData.Query.SemanticAst;

namespace System.Web.Http.OData.Query.Validators
{   
    /// <summary>
    /// Define a validator class used to validate a FilterQueryOption based on the settings
    /// </summary>
    public class FilterQueryValidator
    {  
        /// <summary>
        /// The entry point of this validator class. Use this method to validate the FilterQueryOption
        /// </summary>
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

            ValidateQueryNode(filterQueryOption.QueryNode.Expression, settings);
        }
       
        /// <summary>
        /// Override this method to restrict the 'all' query inside the filter query
        /// </summary>
        /// <param name="allQueryNode"></param>
        /// <param name="settings"></param>
        public virtual void ValidateAllQueryNode(AllQueryNode allQueryNode, ODataValidationSettings settings)
        {
            if (allQueryNode == null)
            {
                throw Error.ArgumentNull("allQueryNode");
            }

            if (settings == null)
            {
                throw Error.ArgumentNull("settings");
            }

            ValidateQueryNode(allQueryNode.Source, settings);

            ValidateQueryNode(allQueryNode.Body, settings);
        }

        /// <summary>
        /// Override this method to restrict the 'any' query inside the filter query
        /// </summary>
        /// <param name="anyQueryNode"></param>
        /// <param name="settings"></param>
        public virtual void ValidateAnyQueryNode(AnyQueryNode anyQueryNode, ODataValidationSettings settings)
        {
            if (anyQueryNode == null)
            {
                throw Error.ArgumentNull("anyQueryNode");
            }

            if (settings == null)
            {
                throw Error.ArgumentNull("settings");
            }

            ValidateQueryNode(anyQueryNode.Source, settings);

            if (anyQueryNode.Body != null && anyQueryNode.Body.Kind != QueryNodeKind.Constant)
            {
                ValidateQueryNode(anyQueryNode.Body, settings);
            }
        }

        /// <summary>
        /// override this method to restrict the binary operators inside the filter query. That includes all the logical operators except 'not' and all math operators.
        /// </summary>
        /// <param name="binaryOperatorNode"></param>
        /// <param name="settings"></param>
        public virtual void ValidateBinaryOperatorQueryNode(BinaryOperatorQueryNode binaryOperatorNode, ODataValidationSettings settings)
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
        /// Please note that 'not' is not included here. Please override ValidateUnaryOperatorQueryNode to customize 'not'.
        /// </summary>
        /// <param name="binaryNode"></param>
        /// <param name="settings"></param>
        public virtual void ValidateLogicalOperator(BinaryOperatorQueryNode binaryNode, ODataValidationSettings settings)
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
        /// Override this method for the Arithmetic operators, including add, sub, mul, div, mod
        /// </summary>
        /// <param name="binaryNode"></param>
        /// <param name="settings"></param>
        public virtual void ValidateArithmeticOperator(BinaryOperatorQueryNode binaryNode, ODataValidationSettings settings)
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
        /// <param name="constantNode"></param>
        /// <param name="settings"></param>
        public virtual void ValidateConstantQueryNode(ConstantQueryNode constantNode, ODataValidationSettings settings)
        {
            if (constantNode == null)
            {
                throw Error.ArgumentNull("constantNode");
            }

            if (settings == null)
            {
                throw Error.ArgumentNull("settings");
            }

           // no default validation logic here
        }

        /// <summary>
        /// Override this method to restrict the 'cast' inside the filter query.
        /// </summary>
        /// <param name="convertQueryNode"></param>
        /// <param name="settings"></param>
        public virtual void ValidateConvertQueryNode(ConvertQueryNode convertQueryNode, ODataValidationSettings settings)
        {
            if (convertQueryNode == null)
            {
                throw Error.ArgumentNull("convertQueryNode");
            }

            if (settings == null)
            {
                throw Error.ArgumentNull("settings");
            }

            // no default validation logic here
            ValidateQueryNode(convertQueryNode.Source, settings);
        }

        /// <summary>
        /// Override this method for the navigation property node
        /// </summary>
        /// <param name="sourceNode"></param>
        /// <param name="navigationProperty"></param>
        /// <param name="settings"></param>
        public virtual void ValidateNavigationPropertyNode(QueryNode sourceNode, IEdmNavigationProperty navigationProperty, ODataValidationSettings settings)
        {  
            if (settings == null)
            {
                throw Error.ArgumentNull("settings");
            }

            // no default validation logic here

            // recursion
            if (sourceNode != null)
            {
                ValidateQueryNode(sourceNode, settings);
            }
        }

        /// <summary>
        /// Override this method to validate the parameter used in the filter query
        /// </summary>
        /// <param name="parameterQueryNode"></param>
        /// <param name="settings"></param>
        public virtual void ValidateParameterQueryNode(ParameterQueryNode parameterQueryNode, ODataValidationSettings settings)
        {
            if (parameterQueryNode == null)
            {
                throw Error.ArgumentNull("parameterQueryNode");
            }

            if (settings == null)
            {
                throw Error.ArgumentNull("settings");
            }

            // no default validation logic here
        }

        /// <summary>
        /// Override this method to validate property accesser
        /// </summary>
        /// <param name="propertyAccessNode"></param>
        /// <param name="settings"></param>
        public virtual void ValidatePropertyAccessQueryNode(PropertyAccessQueryNode propertyAccessNode, ODataValidationSettings settings)
        {
            if (propertyAccessNode == null)
            {
                throw Error.ArgumentNull("propertyAccessNode");
            }

            if (settings == null)
            {
                throw Error.ArgumentNull("settings");
            }

            // no default validation logic here 
            ValidateQueryNode(propertyAccessNode.Source, settings);
        }

        /// <summary>
        /// Override this method to validate Function calls, such as 'length', 'years', etc.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="settings"></param>
        public virtual void ValidateSingleValueFunctionCallQueryNode(SingleValueFunctionCallQueryNode node, ODataValidationSettings settings)
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
        /// Override this method to validate the Not operator
        /// </summary>
        /// <param name="unaryOperatorQueryNode"></param>
        /// <param name="settings"></param>
        public virtual void ValidateUnaryOperatorQueryNode(UnaryOperatorQueryNode unaryOperatorQueryNode, ODataValidationSettings settings)
        {
            ValidateQueryNode(unaryOperatorQueryNode.Operand, settings);

            switch (unaryOperatorQueryNode.OperatorKind)
            {
                case UnaryOperatorKind.Negate:
                case UnaryOperatorKind.Not:
                    if ((settings.AllowedLogicalOperators & AllowedLogicalOperators.Not) != AllowedLogicalOperators.Not)
                    {
                        throw new ODataException(Error.Format(SRResources.NotAllowedLogicalOperator, unaryOperatorQueryNode.OperatorKind, "AllowedLogicalOperators"));
                    }
                    break;
            }
        }

        /// <summary>
        /// Override this method if you want to visit each query node. 
        /// </summary>
        /// <param name="node"></param>
        /// <param name="settings"></param>
        public virtual void ValidateQueryNode(QueryNode node, ODataValidationSettings settings)
        {
            SingleValueQueryNode singleNode = node as SingleValueQueryNode;
            CollectionQueryNode collectionNode = node as CollectionQueryNode;

            if (singleNode != null)
            {
                ValidateSingleValueQueryNode(singleNode, settings);
            }
            else if (collectionNode != null)
            {
                ValidateCollectionQueryNode(collectionNode, settings);
            }
        }

        private void ValidateCollectionQueryNode(CollectionQueryNode node, ODataValidationSettings settings)
        {
            switch (node.Kind)
            {
                case QueryNodeKind.Filter:
                    FilterQueryNode filterNode = node as FilterQueryNode;
                    ValidateSingleValueQueryNode(filterNode.Expression, settings);
                    break;

                case QueryNodeKind.Segment:
                    CollectionNavigationNode navigationNode = node as CollectionNavigationNode;
                    ValidateNavigationPropertyNode(navigationNode.Source, navigationNode.NavigationProperty(), settings);
                    break;
            }
        }

        /// <summary>
        /// The recursive method that validate most of the query node type is of SingleValueQueryNode type.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="settings"></param>
        private void ValidateSingleValueQueryNode(SingleValueQueryNode node, ODataValidationSettings settings)
        {
            switch (node.Kind)
            {
                case QueryNodeKind.BinaryOperator:
                    ValidateBinaryOperatorQueryNode(node as BinaryOperatorQueryNode, settings);
                    break;

                case QueryNodeKind.Constant:
                    ValidateConstantQueryNode(node as ConstantQueryNode, settings);
                    break;

                case QueryNodeKind.Convert:
                    ValidateConvertQueryNode(node as ConvertQueryNode, settings);
                    break;

                case QueryNodeKind.Parameter:
                    ValidateParameterQueryNode(node as ParameterQueryNode, settings);
                    break;

                case QueryNodeKind.PropertyAccess:
                    ValidatePropertyAccessQueryNode(node as PropertyAccessQueryNode, settings);
                    break;

                case QueryNodeKind.UnaryOperator:
                    ValidateUnaryOperatorQueryNode(node as UnaryOperatorQueryNode, settings);
                    break;

                case QueryNodeKind.SingleValueFunctionCall:
                    ValidateSingleValueFunctionCallQueryNode(node as SingleValueFunctionCallQueryNode, settings);
                    break;

                case QueryNodeKind.Segment:
                    SingletonNavigationNode navigationNode = node as SingletonNavigationNode;
                    ValidateNavigationPropertyNode(navigationNode.Source, navigationNode.NavigationProperty, settings);
                    break;

                case QueryNodeKind.Any:
                    ValidateAnyQueryNode(node as AnyQueryNode, settings);
                    break;

                case QueryNodeKind.All:
                    ValidateAllQueryNode(node as AllQueryNode, settings);
                    break;

                default:
                    throw new ODataException(Error.Format(SRResources.QueryNodeBindingNotSupported, node.Kind, typeof(FilterQueryValidator).Name));
            }
        }

        private static void ValidateFunction(string functionName, ODataValidationSettings settings)
        {
            AllowedFunctionNames convertedFunctionName = ToODataFunctionNames(functionName);
            if ((settings.AllowedFunctionNames & convertedFunctionName) != convertedFunctionName)
            {
                // this means the given function is not allowed
                throw new ODataException(Error.Format(SRResources.NotAllowedFunctionName, functionName, "AllowedFunctionNames"));
            }
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "These are simple conversion function and cannot be split up.")]
        private static AllowedFunctionNames ToODataFunctionNames(string functionName)
        {
            AllowedFunctionNames result = AllowedFunctionNames.None;

            switch (functionName)
            {
                case "any":
                    result = AllowedFunctionNames.Any;
                    break;
                case "all":
                    result = AllowedFunctionNames.All;
                    break;
                case "cast":
                    result = AllowedFunctionNames.Cast;
                    break;
                case ClrCanonicalFunctions.CeilingFunctionName:
                    result = AllowedFunctionNames.Ceiling;
                    break;
                case ClrCanonicalFunctions.ConcatFunctionName:
                    result = AllowedFunctionNames.Concat;
                    break;
                case ClrCanonicalFunctions.DayFunctionName:
                    result = AllowedFunctionNames.Day;
                    break;
                case ClrCanonicalFunctions.DaysFunctionName:
                    result = AllowedFunctionNames.Days;
                    break;
                case ClrCanonicalFunctions.EndswithFunctionName:
                    result = AllowedFunctionNames.EndsWith;
                    break;
                case ClrCanonicalFunctions.FloorFunctionName:
                    result = AllowedFunctionNames.Floor;
                    break;
                case ClrCanonicalFunctions.HourFunctionName:
                    result = AllowedFunctionNames.Hour;
                    break;
                case ClrCanonicalFunctions.HoursFunctionName:
                    result = AllowedFunctionNames.Hours;
                    break;
                case ClrCanonicalFunctions.IndexofFunctionName:
                    result = AllowedFunctionNames.IndexOf;
                    break;
                case "IsOf":
                    result = AllowedFunctionNames.IsOf;
                    break;
                case ClrCanonicalFunctions.LengthFunctionName:
                    result = AllowedFunctionNames.Length;
                    break;
                case ClrCanonicalFunctions.MinuteFunctionName:
                    result = AllowedFunctionNames.Minute;
                    break;
                case ClrCanonicalFunctions.MinutesFunctionName:
                    result = AllowedFunctionNames.Minutes;
                    break;
                case ClrCanonicalFunctions.MonthFunctionName:
                    result = AllowedFunctionNames.Month;
                    break;
                case ClrCanonicalFunctions.MonthsFunctionName:
                    result = AllowedFunctionNames.Months;
                    break;
                case ClrCanonicalFunctions.RoundFunctionName:
                    result = AllowedFunctionNames.Round;
                    break;
                case ClrCanonicalFunctions.SecondFunctionName:
                    result = AllowedFunctionNames.Second;
                    break;
                case ClrCanonicalFunctions.SecondsFunctionName:
                    result = AllowedFunctionNames.Seconds;
                    break;
                case ClrCanonicalFunctions.StartswithFunctionName:
                    result = AllowedFunctionNames.StartsWith;
                    break;
                case ClrCanonicalFunctions.SubstringFunctionName:
                    result = AllowedFunctionNames.Substring;
                    break;
                case ClrCanonicalFunctions.SubstringofFunctionName:
                    result = AllowedFunctionNames.SubstringOf;
                    break;
                case ClrCanonicalFunctions.TolowerFunctionName:
                    result = AllowedFunctionNames.ToLower;
                    break;
                case ClrCanonicalFunctions.ToupperFunctionName:
                    result = AllowedFunctionNames.ToUpper;
                    break;
                case ClrCanonicalFunctions.TrimFunctionName:
                    result = AllowedFunctionNames.Trim;
                    break;
                case ClrCanonicalFunctions.YearFunctionName:
                    result = AllowedFunctionNames.Year;
                    break;
                case ClrCanonicalFunctions.YearsFunctionName:
                    result = AllowedFunctionNames.Years;
                    break;
                default:
                    // should never be here
                    Contract.Assert(true, "ToODataFunctionNames should never be here.");
                    break;
            }

            return result;
        }

        private static AllowedLogicalOperators ToLogicalOperator(BinaryOperatorQueryNode binaryNode)
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

        private static AllowedArithmeticOperators ToArithmeticOperator(BinaryOperatorQueryNode binaryNode)
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
