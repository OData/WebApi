// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations;
using Microsoft.Spatial;

namespace WebStack.QA.Test.OData.Spatial
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
