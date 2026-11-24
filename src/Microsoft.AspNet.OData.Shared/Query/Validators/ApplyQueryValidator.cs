//-----------------------------------------------------------------------------
// <copyright file="ApplyQueryValidator.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNet.OData.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.UriParser;
using Microsoft.OData.UriParser.Aggregation;

namespace Microsoft.AspNet.OData.Query.Validators
{
    /// <summary>
    /// Represents a validator used to validate an <see cref="ApplyQueryOption"/> based on the
    /// <see cref="ODataValidationSettings"/>.
    /// </summary>
    public class ApplyQueryValidator
    {
        /// <summary>
        /// Validates an <see cref="ApplyQueryOption"/>.
        /// </summary>
        /// <param name="applyQueryOption">The $apply query.</param>
        /// <param name="validationSettings">The validation settings.</param>
        public virtual void Validate(ApplyQueryOption applyQueryOption, ODataValidationSettings validationSettings)
        {
            if (applyQueryOption == null)
            {
                throw Error.ArgumentNull("applyQueryOption");
            }

            if (validationSettings == null)
            {
                throw Error.ArgumentNull("validationSettings");
            }

            ApplyClause applyClause = applyQueryOption.ApplyClause;
            if (applyClause == null)
            {
                return;
            }

            foreach (TransformationNode transformation in applyClause.Transformations)
            {
                ValidateTransformation(transformation, applyQueryOption.Context, validationSettings);
            }
        }

        internal static ApplyQueryValidator GetApplyQueryValidator(ODataQueryContext context)
        {
            return context == null || context.RequestContainer == null
                ? new ApplyQueryValidator()
                : context.RequestContainer.GetRequiredService<ApplyQueryValidator>();
        }

        private static void ValidateTransformation(TransformationNode transformation, ODataQueryContext context,
            ODataValidationSettings validationSettings)
        {
            if (transformation == null)
            {
                return;
            }

            switch (transformation.Kind)
            {
                case TransformationNodeKind.Filter:
                    FilterTransformationNode filter = (FilterTransformationNode)transformation;
                    // Preserve $filter's model settings, custom validator and complexity limits.
                    new FilterQueryOption(context, filter.FilterClause).Validate(validationSettings);
                    break;

                case TransformationNodeKind.GroupBy:
                    GroupByTransformationNode groupBy = (GroupByTransformationNode)transformation;
                    if (groupBy.GroupingProperties != null)
                    {
                        foreach (GroupByPropertyNode property in groupBy.GroupingProperties)
                        {
                            ValidateGroupByProperty(property, context, validationSettings);
                        }
                    }

                    ValidateTransformation(groupBy.ChildTransformations, context, validationSettings);
                    break;

                case TransformationNodeKind.Aggregate:
                    AggregateTransformationNode aggregate = (AggregateTransformationNode)transformation;
                    if (aggregate.AggregateExpressions != null)
                    {
                        foreach (AggregateExpressionBase expression in aggregate.AggregateExpressions)
                        {
                            ValidateAggregateExpression(expression, context, validationSettings);
                        }
                    }

                    break;

                case TransformationNodeKind.Compute:
                    ComputeTransformationNode compute = (ComputeTransformationNode)transformation;
                    if (compute.Expressions != null)
                    {
                        foreach (ComputeExpression expression in compute.Expressions)
                        {
                            QueryNodeRestrictionValidator.Validate(expression.Expression, context, validationSettings);
                        }
                    }

                    break;

                // Other transformations are not bound or executed by ApplyQueryOption.ApplyTo.
            }
        }

        private static void ValidateGroupByProperty(GroupByPropertyNode property, ODataQueryContext context,
            ODataValidationSettings validationSettings)
        {
            if (property == null)
            {
                return;
            }

            QueryNodeRestrictionValidator.Validate(property.Expression, context, validationSettings);
            if (property.ChildTransformations != null)
            {
                foreach (GroupByPropertyNode child in property.ChildTransformations)
                {
                    ValidateGroupByProperty(child, context, validationSettings);
                }
            }
        }

        private static void ValidateAggregateExpression(AggregateExpressionBase expression, ODataQueryContext context,
            ODataValidationSettings validationSettings)
        {
            AggregateExpression singleExpression = expression as AggregateExpression;
            if (singleExpression != null)
            {
                QueryNodeRestrictionValidator.Validate(singleExpression.Expression, context, validationSettings);
                return;
            }

            EntitySetAggregateExpression entitySetExpression = expression as EntitySetAggregateExpression;
            if (entitySetExpression != null)
            {
                QueryNodeRestrictionValidator.Validate(entitySetExpression.Expression, context, validationSettings);
                if (entitySetExpression.Children != null)
                {
                    foreach (AggregateExpressionBase child in entitySetExpression.Children)
                    {
                        ValidateAggregateExpression(child, context, validationSettings);
                    }
                }
            }
        }
    }
}
