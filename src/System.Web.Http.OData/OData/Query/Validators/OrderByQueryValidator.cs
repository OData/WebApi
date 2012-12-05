// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Web.Http.OData.Properties;
using Microsoft.Data.OData;

namespace System.Web.Http.OData.Query.Validators
{
    public class OrderByQueryValidator
    {
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
