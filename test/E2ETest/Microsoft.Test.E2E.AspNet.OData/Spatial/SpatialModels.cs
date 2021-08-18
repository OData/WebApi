//-----------------------------------------------------------------------------
// <copyright file="SpatialModels.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.ComponentModel.DataAnnotations;
using Microsoft.Spatial;

namespace Microsoft.Test.E2E.AspNet.OData.Spatial
{
    public class SpatialCustomer
    {
        [Key]
        public int CustomerId { get; set; }

        public string Name { get; set; }

        public GeographyPoint Location { get; set; }

        public GeographyLineString Region { get; set; }

        public GeometryPoint HomePoint { get; set; }
    }
}
