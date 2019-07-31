// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Microsoft.Test.E2E.AspNet.OData.ODataOrderByTest
{
    public class Item
    {
        [Key]
        [Column(Order = 2)]
        public int A { get; set; }

        [Key]
        [Column(Order = 1)]
        public int C { get; set; }

        [Key]
        [Column(Order = 3)]
        public int B { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }
    }

    public class Item2
    {
        [Key]
        [Column(Order = 3)]
        public string A { get; set; }

        [Key]
        [Column(Order = 1)]
        public string C { get; set; }  

        [Key]
        [Column(Order = 2)]
        public int B { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }
    }
}

