//-----------------------------------------------------------------------------
// <copyright file="ComputeBinder.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.AspNet.OData.Interfaces;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Microsoft.OData.UriParser.Aggregation;

namespace Microsoft.AspNet.OData.Query.Expressions
{
    internal class ComputeBinder : TransformationBinderBase
    {
        private ComputeTransformationNode _transformation;
        private string _modelID;

        internal ComputeBinder(ODataQuerySettings settings, IWebApiAssembliesResolver assembliesResolver, Type elementType,
            IEdmModel model, ComputeTransformationNode transformation)
            : base(settings, assembliesResolver, elementType, model)
        {
            Contract.Assert(transformation != null);
            
            _transformation = transformation;
            _modelID = ModelContainer.GetModelID(model);

            this.ResultClrType = typeof(ComputeWrapper<>).MakeGenericType(this.ElementType);
        }

        public IQueryable Bind(IQueryable query)
        {
            PreprocessQuery(query);
            // compute(X add Y as Z, A mul B as C) adds new properties to the output
            // Should return following expression
            // .Select($it => new ComputeWrapper<T> {
            //      Instance = $it,
            //      ModelID = 'Guid',
            //      Container => new AggregationPropertyContainer() {
            //          Name = "X", 
            //          Value = $it.X + $it.Y, 
            //          Next = new LastInChain() {
            //              Name = "C",
            //              Value = $it.A * $it.B
            //      }
            // })

            List<MemberAssignment> wrapperTypeMemberAssignments = new List<MemberAssignment>();

            // Set Instance property
            var wrapperProperty = this.ResultClrType.GetProperty("Instance");
            wrapperTypeMemberAssignments.Add(Expression.Bind(wrapperProperty, this.LambdaParameter));
            var properties = new List<NamedPropertyExpression>();
            foreach (var computeExpression in this._transformation.Expressions)
            {
                properties.Add(new NamedPropertyExpression(Expression.Constant(computeExpression.Alias), CreateComputeExpression(computeExpression)));
            }

            // Initialize property 'ModelID' on the wrapper class.
            // source = new Wrapper { ModelID = 'some-guid-id' }
            wrapperProperty = this.ResultClrType.GetProperty("ModelID");
            var wrapperPropertyValueExpression = QuerySettings.EnableConstantParameterization ?
                LinqParameterContainer.Parameterize(typeof(string), _modelID) :
                Expression.Constant(_modelID);
            wrapperTypeMemberAssignments.Add(Expression.Bind(wrapperProperty, wrapperPropertyValueExpression));

            // Set new compute properties
            wrapperProperty = ResultClrType.GetProperty("Container");
            wrapperTypeMemberAssignments.Add(Expression.Bind(wrapperProperty, AggregationPropertyContainer.CreateNextNamedPropertyContainer(properties)));

            var initilizedMember =
                Expression.MemberInit(Expression.New(ResultClrType), wrapperTypeMemberAssignments);
            var selectLambda = Expression.Lambda(initilizedMember, this.LambdaParameter);

            var result = ExpressionHelpers.Select(query, selectLambda, this.ElementType);
            return result;
        }

        private Expression CreateComputeExpression(ComputeExpression expression)
        {
            Expression body = BindAccessor(expression.Expression);
            return WrapConvert(body);
        }
    }
}
