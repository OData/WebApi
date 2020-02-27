// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNet.OData.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.OData.Edm;

namespace Microsoft.Test.E2E.AspNet.OData.Endpoint
{
    public class EndpointModelGenerator
    {
        // Builds the EDM model for the OData service.
        public static IEdmModel GetConventionalEdmModel()
        {
            var modelBuilder = new ODataConventionModelBuilder();
            modelBuilder.EntitySet<EpCustomer>("EpCustomers");
            modelBuilder.EntitySet<EpOrder>("EpOrders");
            return modelBuilder.GetEdmModel();
        }
    }

    public class EpCustomer
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public virtual EpAddress HomeAddress { get; set; }

        public virtual IList<EpAddress> FavoriteAddresses { get; set; }

        public virtual EpOrder Order { get; set; }

        public virtual IList<EpOrder> Orders { get; set; }
    }

    public class EpOrder
    {
        public int Id { get; set; }

        public string Title { get; set; }
    }

    [Owned, ComplexType]
    public class EpAddress
    {
        public string City { get; set; }

        public string Street { get; set; }
    }
}
