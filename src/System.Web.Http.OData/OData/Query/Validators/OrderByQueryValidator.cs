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
                ICollection<OrderByPropertyNode> propertyNodes = orderByOption.PropertyNodes;

                foreach (OrderByPropertyNode property in propertyNodes)
                {
                    if (!validationSettings.AllowedOrderByProperties.Contains(property.Property.Name))
                    {
                        throw new ODataException(Error.Format(SRResources.NotAllowedOrderByProperty, property.Property.Name, "AllowedOrderByProperties"));
                    }
                }
            }
        }
    }
}
