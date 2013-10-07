// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Web.Http.OData.Properties;
using Microsoft.Data.OData;
using Microsoft.Data.OData.Query.SemanticAst;

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

            if (validationSettings.AllowedOrderByProperties.Count > 0)
            {
                IEnumerable<OrderByNode> orderByNodes = orderByOption.OrderByNodes;

                foreach (OrderByNode node in orderByNodes)
                {
                    string propertyName = null;
                    OrderByPropertyNode property = node as OrderByPropertyNode;
                    if (property != null)
                    {
                        propertyName = property.Property.Name;
                    }
                    else if ((node as OrderByItNode) != null && !validationSettings.AllowedOrderByProperties.Contains("$it"))
                    {
                        propertyName = "$it";
                    }

                    if (propertyName != null && !validationSettings.AllowedOrderByProperties.Contains(propertyName))
                    {
                        throw new ODataException(Error.Format(SRResources.NotAllowedOrderByProperty, propertyName, "AllowedOrderByProperties"));
                    }
                }
            }
        }
    }
}
