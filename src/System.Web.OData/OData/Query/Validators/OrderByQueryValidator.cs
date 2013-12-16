// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Web.Http.OData.Formatter;
using System.Web.Http.OData.Properties;
using Microsoft.OData.Core;
using Microsoft.OData.Core.UriParser.Semantic;

namespace System.Web.Http.OData.Query.Validators
{
    /// <summary>
    /// Represents a validator used to validate an <see cref="OrderByQueryOption"/> based on the <see cref="ODataValidationSettings"/>.
    /// </summary>
    public class OrderByQueryValidator
    {
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
                    throw new ODataException(Error.Format(SRResources.OrderByNodeCountExceeded, validationSettings.MaxOrderByNodeCount));
                }
            }

            // need to validate only if orderby options presented
            if (orderByOption.OrderByNodes.Count > 0)
            {
                foreach (OrderByNode node in orderByOption.OrderByNodes)
                {
                    string propertyName = null;
                    OrderByPropertyNode propertyNode = node as OrderByPropertyNode;
                    if (propertyNode != null)
                    {
                        propertyName = propertyNode.Property.Name;

                        // First validate whether it's allowed or not
                        if (propertyName != null && 
                            validationSettings.AllowedOrderByProperties.Count > 0 && 
                            !validationSettings.AllowedOrderByProperties.Contains(propertyName))
                        {
                            throw new ODataException(Error.Format(SRResources.NotAllowedOrderByProperty, propertyName,
                                "AllowedOrderByProperties"));
                        }

                        // Second validate whether it's limited or not
                        if (EdmLibHelpers.IsUnsortable(propertyNode.Property, orderByOption.Context.Model))
                        {
                            throw new ODataException(Error.Format(SRResources.UnsortablePropertyUsedInOrderBy, propertyName));
                        }
                    }
                    else if (node as OrderByItNode != null)
                    {
                        propertyName = "$it";
                        if (validationSettings.AllowedOrderByProperties.Count > 0 &&
                            !validationSettings.AllowedOrderByProperties.Contains(propertyName))
                        {
                            throw new ODataException(Error.Format(SRResources.NotAllowedOrderByProperty, propertyName,
                                "AllowedOrderByProperties"));
                        }
                    }
                }
            }
        }
    }
}
