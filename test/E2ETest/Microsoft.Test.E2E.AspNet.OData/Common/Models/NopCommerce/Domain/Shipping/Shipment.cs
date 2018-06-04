// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Nop.Core.Domain.Orders;

namespace Nop.Core.Domain.Shipping
{
    /// <summary>
    /// Represents a shipment
    /// </summary>
    public partial class Shipment : BaseEntity
    {
        private ICollection<ShipmentOrderProductVariant> _shipmentOrderProductVariants;

        /// <summary>
        /// Gets or sets the order identifier
        /// </summary>
        public virtual int OrderId { get; set; }

        /// <summary>
        /// Gets or sets the tracking number of this shipment
        /// </summary>
        public virtual string TrackingNumber { get; set; }

        /// <summary>
        /// Gets or sets the total weight of this shipment
        /// It's nullable for compatibility with the previous version of nopCommerce where was no such property
        /// </summary>
        public virtual decimal? TotalWeight { get; set; }

        /// <summary>
        /// Gets or sets the shipped date and time
        /// </summary>
        public virtual DateTimeOffset? ShippedDateUtc { get; set; }

        /// <summary>
        /// Gets or sets the delivery date and time
        /// </summary>
        public virtual DateTimeOffset? DeliveryDateUtc { get; set; }

        /// <summary>
        /// Gets or sets the entity creation date
        /// </summary>
        public virtual DateTimeOffset CreatedOnUtc { get; set; }

        /// <summary>
        /// Gets the order
        /// </summary>
        public virtual Order Order { get; set; }

        /// <summary>
        /// Gets or sets the shipment order product variants
        /// </summary>
        public virtual ICollection<ShipmentOrderProductVariant> ShipmentOrderProductVariants
        {
            get { return _shipmentOrderProductVariants ?? (_shipmentOrderProductVariants = new List<ShipmentOrderProductVariant>()); }
            protected set { _shipmentOrderProductVariants = value; }
        }
    }
}