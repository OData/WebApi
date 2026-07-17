//-----------------------------------------------------------------------------
// <copyright file="TransformationBinderBase.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNet.OData.Interfaces;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNet.OData.Query.Expressions
{
    internal class TransformationBinderBase : ExpressionBinderBase
    {

        internal TransformationBinderBase(ODataQuerySettings settings, IWebApiAssembliesResolver assembliesResolver, Type elementType,
            IEdmModel model) : base(model, assembliesResolver, settings)
        {
            Contract.Assert(elementType != null);
            LambdaParameter = Expression.Parameter(elementType, "$it");
        }

        protected Type ElementType { get { return this.LambdaParameter.Type; } }

        protected ParameterExpression LambdaParameter { get; set; }

        protected bool ClassicEF { get; private set; }

        /// <summary>
        /// Gets CLR type returned from the query.
        /// </summary>
        public Type ResultClrType
        {
            get; protected set;
        }

        /// <summary>
        /// Checks IQueryable provider for need of EF6 oprimization
        /// </summary>
        /// <param name="query"></param>
        /// <returns>True if EF6 optimization are needed.</returns>
        internal virtual bool IsClassicEF(IQueryable query)
        {
            var providerNS = query.Provider.GetType().Namespace;
            return (providerNS == HandleNullPropagationOptionHelper.ObjectContextQueryProviderNamespaceEF6
                || providerNS == HandleNullPropagationOptionHelper.EntityFrameworkQueryProviderNamespace);
        }

        protected void PreprocessQuery(IQueryable query)
        {
            Contract.Assert(query != null);

            this.ClassicEF = IsClassicEF(query);
            this.BaseQuery = query;
            EnsureFlattenedPropertyContainer(this.LambdaParameter);
        }

        protected Expression WrapConvert(Expression expression)
        {
            // Expression that we are generating looks like Value = $it.PropertyName where Value is defined as object and PropertyName can be value 
            // Proper .NET expression must look like as Value = (object) $it.PropertyName for proper boxing or AccessViolationException will be thrown
            // Cast to object isn't translatable by EF6 as a result skipping (object) in that case
            return (this.ClassicEF || !expression.Type.IsValueType)
                ? expression
                : Expression.Convert(expression, typeof(object));
        }

        public override Expression Bind(QueryNode node)
        {
            SingleValueNode singleValueNode = node as SingleValueNode;
            if (node != null)
            {
                return BindAccessor(singleValueNode);
            }

            throw new ArgumentException("Only SigleValueNode supported", "node");
        }

        protected override ParameterExpression Parameter
        {
            get
            {
                return this.LambdaParameter;
            }
        }

        protected Expression BindAccessor(QueryNode node, Expression baseElement = null)
        {
            switch (node.Kind)
            {
                case QueryNodeKind.ResourceRangeVariableReference:
                    return this.LambdaParameter.Type.IsGenericType && this.LambdaParameter.Type.GetGenericTypeDefinition() == typeof(FlatteningWrapper<>)
                        ? (Expression)Expression.Property(this.LambdaParameter, "Source")
                        : this.LambdaParameter;
                case QueryNodeKind.SingleValuePropertyAccess:
                    var propAccessNode = node as SingleValuePropertyAccessNode;
                    return CreatePropertyAccessExpression(BindAccessor(propAccessNode.Source, baseElement), propAccessNode.Property, GetFullPropertyPath(propAccessNode));
                case QueryNodeKind.AggregatedCollectionPropertyNode:
                    var aggPropAccessNode = node as AggregatedCollectionPropertyNode;
                    return CreatePropertyAccessExpression(BindAccessor(aggPropAccessNode.Source, baseElement), aggPropAccessNode.Property);
                case QueryNodeKind.SingleComplexNode:
                    var singleComplexNode = node as SingleComplexNode;
                    return CreatePropertyAccessExpression(BindAccessor(singleComplexNode.Source, baseElement), singleComplexNode.Property, GetFullPropertyPath(singleComplexNode));
                case QueryNodeKind.SingleValueOpenPropertyAccess:
                    var openNode = node as SingleValueOpenPropertyAccessNode;
                    return GetFlattenedPropertyExpression(openNode.Name) ?? CreateOpenPropertyAccessExpression(openNode);
                case QueryNodeKind.None:
                case QueryNodeKind.SingleNavigationNode:
                    var navNode = (SingleNavigationNode)node;
                    return CreatePropertyAccessExpression(BindAccessor(navNode.Source), navNode.NavigationProperty, GetFullPropertyPath(navNode));
                case QueryNodeKind.BinaryOperator:
                    var binaryNode = (BinaryOperatorNode)node;
                    var leftExpression = BindAccessor(binaryNode.Left, baseElement);
                    var rightExpression = BindAccessor(binaryNode.Right, baseElement);
                    return CreateBinaryExpression(binaryNode.OperatorKind, leftExpression, rightExpression,
                        liftToNull: true);
                case QueryNodeKind.Convert:
                    var convertNode = (ConvertNode)node;
                    return CreateConvertExpression(convertNode, BindAccessor(convertNode.Source, baseElement));
                case QueryNodeKind.CollectionNavigationNode:
                    return baseElement ?? this.LambdaParameter;
                case QueryNodeKind.SingleValueFunctionCall:
                    return BindSingleValueFunctionCallNode(node as SingleValueFunctionCallNode);
                case QueryNodeKind.Constant:
                    return BindConstantNode(node as ConstantNode);
                default:
                    throw Error.NotSupported(SRResources.QueryNodeBindingNotSupported, node.Kind,
                        typeof(AggregationBinder).Name);
            }
        }

        /// <summary>
        /// Gets the CLR <see cref="PropertyInfo"/> that a property segment binds to, mapping the
        /// segment's EDM name to the CLR property name. Returns null for a dynamic (open) property, which
        /// the caller then reads from the dynamic property container.
        /// </summary>
        /// <param name="instanceType">The runtime CLR type of the value being accessed.</param>
        /// <param name="propertyName">The property name from the URI segment (the EDM name).</param>
        /// <param name="model">The Edm model.</param>
        /// <returns>
        /// The <see cref="PropertyInfo"/> to bind for a property declared in the model (resolved by its
        /// CLR name), or null when the segment is an undeclared member of a type that is in the model
        /// (whether open or not), so the caller resolves it from the dynamic container or treats it as
        /// absent rather than binding a CLR member that is not part of the model.
        /// </returns>
        private static PropertyInfo GetDeclaredClrProperty(Type instanceType, string propertyName, IEdmModel model)
        {
            if (model != null && model.GetEdmType(instanceType) is IEdmStructuredType structuredType)
            {
                IEdmProperty edmProperty = structuredType.FindProperty(propertyName);
                if (edmProperty != null)
                {
                    // Declared in the model: bind by the CLR name, which can differ from the EDM name
                    // (e.g. a [DataMember(Name=...)] or builder .Name rename). Fall through to the dynamic
                    // container (null) rather than throwing if the CLR name cannot be resolved.
                    string clrPropertyName = EdmLibHelpers.GetClrPropertyName(edmProperty, model);
                    return clrPropertyName != null ? instanceType.GetProperty(clrPropertyName) : null;
                }

                // The name is not declared on this modeled type. Whether the type is open (a dynamic
                // member) or not (a CLR member deliberately excluded from the model, e.g. via [NotMapped]
                // or Ignore()), it must not bind to the CLR member. Return null so the caller resolves it
                // from the dynamic container (open type) or treats it as absent, consistent with how a
                // genuinely dynamic name on the same type is handled.
                return null;
            }

            // A type that is not in the model (e.g. an intermediate transformation result such as a
            // grouping or flattening wrapper): bind the CLR property directly by the segment name.
            return instanceType.GetProperty(propertyName);
        }

        private Expression CreateOpenPropertyAccessExpression(SingleValueOpenPropertyAccessNode openNode)
        {
            Expression sourceAccessor = BindAccessor(openNode.Source);

            // If the segment names a property declared in the model, bind to it directly (e.g. it exists
            // on the source type after an earlier $apply transformation), mapping the EDM name to the CLR
            // name. Otherwise fall through to the dynamic property container below.
            PropertyInfo declaredClrProperty = GetDeclaredClrProperty(sourceAccessor.Type, openNode.Name, Model);
            if (declaredClrProperty != null)
            {
                return Expression.Property(sourceAccessor, declaredClrProperty);
            }

            // Property doesn't exists go for dynamic properties dictionary
            PropertyInfo prop = GetDynamicPropertyContainer(openNode);
            MemberExpression propertyAccessExpression = Expression.Property(sourceAccessor, prop.Name);
            IndexExpression readDictionaryIndexerExpression = Expression.Property(propertyAccessExpression,
                            DictionaryStringObjectIndexerName, Expression.Constant(openNode.Name));
            MethodCallExpression containsKeyExpression = Expression.Call(propertyAccessExpression,
                propertyAccessExpression.Type.GetMethod("ContainsKey"), Expression.Constant(openNode.Name));
            ConstantExpression nullExpression = Expression.Constant(null);

            if (QuerySettings.HandleNullPropagation == HandleNullPropagationOption.True)
            {
                var dynamicDictIsNotNull = Expression.NotEqual(propertyAccessExpression, Expression.Constant(null));
                var dynamicDictIsNotNullAndContainsKey = Expression.AndAlso(dynamicDictIsNotNull, containsKeyExpression);
                return Expression.Condition(
                    dynamicDictIsNotNullAndContainsKey,
                    readDictionaryIndexerExpression,
                    nullExpression);
            }
            else
            {
                return Expression.Condition(
                    containsKeyExpression,
                    readDictionaryIndexerExpression,
                    nullExpression);
            }
        }
    }
}
