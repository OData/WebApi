//-----------------------------------------------------------------------------
// <copyright file="OrderByQueryValidator.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNet.OData.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNet.OData.Query.Validators
{
    /// <summary>
    /// Represents a validator used to validate an <see cref="OrderByQueryOption"/> based on the <see cref="ODataValidationSettings"/>.
    /// </summary>
    public class OrderByQueryValidator
    {
        private readonly DefaultQuerySettings _defaultQuerySettings;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderByQueryValidator" /> class based on
        /// the <see cref="DefaultQuerySettings" />.
        /// </summary>
        /// <param name="defaultQuerySettings">The <see cref="DefaultQuerySettings" />.</param>
        public OrderByQueryValidator(DefaultQuerySettings defaultQuerySettings)
        {
            _defaultQuerySettings = defaultQuerySettings;
        }

        /// <summary>
        /// Validates an <see cref="OrderByQueryOption" />.
        /// </summary>
        /// <param name="orderByOption">The $orderby query.</param>
        /// <param name="validationSettings">The validation settings.</param>
        public virtual void Validate(OrderByQueryOption orderByOption, ODataValidationSettings validationSettings)
        {
            if (orderByOption == null)
            {
                throw Error.ArgumentNull("orderByOption");
            }

            if (validationSettings == null)
            {
                throw Error.ArgumentNull("validationSettings");
            }

            int nodeCount = 0;
            for (OrderByClause clause = orderByOption.OrderByClause; clause != null; clause = clause.ThenBy)
            {
                nodeCount++;
                if (nodeCount > validationSettings.MaxOrderByNodeCount)
                {
                    throw new ODataException(Error.Format(SRResources.OrderByNodeCountExceeded,
                        validationSettings.MaxOrderByNodeCount));
                }
            }

            OrderByModelLimitationsValidator validator = new OrderByModelLimitationsValidator(orderByOption.Context, _defaultQuerySettings.EnableOrderBy);
            bool explicitAllowedProperties = validationSettings.AllowedOrderByProperties.Count > 0;

            foreach (OrderByNode node in orderByOption.OrderByNodes)
            {
                string propertyName = null;
                OrderByPropertyNode propertyNode = node as OrderByPropertyNode;
                if (propertyNode != null)
                {
                    propertyName = propertyNode.Property.Name;
                    bool isValidPath = !validator.TryValidate(propertyNode.OrderByClause, explicitAllowedProperties);
                    if (propertyName != null && isValidPath && explicitAllowedProperties)
                    {
                        // Explicit allowed properties were specified, but this one isn't within the list of allowed 
                        // properties.
                        if (!IsAllowed(validationSettings, propertyName))
                        {
                            throw new ODataException(Error.Format(SRResources.NotAllowedOrderByProperty, propertyName,
                                "AllowedOrderByProperties"));
                        }
                    }
                    else if (propertyName != null)
                    {
                        // The property wasn't limited but it wasn't contained in the set of explicitly allowed 
                        // properties.
                        if (!IsAllowed(validationSettings, propertyName))
                        {
                            throw new ODataException(Error.Format(SRResources.NotAllowedOrderByProperty, propertyName,
                                "AllowedOrderByProperties"));
                        }
                    }
                }
                else
                {
                    propertyName = "$it";
                    if (!IsAllowed(validationSettings, propertyName))
                    {
                        throw new ODataException(Error.Format(SRResources.NotAllowedOrderByProperty, propertyName,
                            "AllowedOrderByProperties"));
                    }
                }
            }
        }

        internal static OrderByQueryValidator GetOrderByQueryValidator(ODataQueryContext context)
        {
            if (context == null)
            {
                return new OrderByQueryValidator(new DefaultQuerySettings());
            }

            return context.RequestContainer == null
                ? new OrderByQueryValidator(context.DefaultQuerySettings)
                : context.RequestContainer.GetRequiredService<OrderByQueryValidator>();
        }

        private static bool IsAllowed(ODataValidationSettings validationSettings, string propertyName)
        {
            return validationSettings.AllowedOrderByProperties.Count == 0 ||
                   validationSettings.AllowedOrderByProperties.Contains(propertyName);
        }
    }
}
