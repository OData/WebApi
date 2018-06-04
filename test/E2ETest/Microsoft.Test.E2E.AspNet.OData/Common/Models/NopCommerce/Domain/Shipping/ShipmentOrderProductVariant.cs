// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

namespace Nop.Core.Domain.Shipping
{
    /// <summary>
    /// Represents a shipment order product variant
    /// </summary>
    public partial class ShipmentOrderProductVariant : BaseEntity
    {
        /// <summary>
        /// Gets or sets the shipment identifier
        /// </summary>
        public virtual int ShipmentId { get; set; }

        /// <summary>
        /// Gets or sets the order product variant identifier
        /// </summary>
        public virtual int OrderProductVariantId { get; set; }

        /// <summary>
        /// Gets or sets the quantity
        /// </summary>
        public virtual int Quantity { get; set; }

        /// <summary>
        /// Gets the shipment
        /// </summary>
        public virtual Shipment Shipment { get; set; }
    }
}