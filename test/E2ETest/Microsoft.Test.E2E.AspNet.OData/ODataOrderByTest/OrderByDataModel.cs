//-----------------------------------------------------------------------------
// <copyright file="OrderByDataModel.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Microsoft.Test.E2E.AspNet.OData.ODataOrderByTest
{
    public class Item:OrderedItem
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
    }

    public class Item2 : OrderedItem
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
    }

    public class ItemWithEnum : OrderedItem
    {
        [Key]
        [Column(Order = 3)]
        public SmallNumber A { get; set; }

        [Key]
        [Column(Order = 2)]
        public string B { get; set; }

        [Key]
        [Column(Order = 1)]
        public SmallNumber C { get; set; }
    }

    public class ItemWithoutColumn : OrderedItem
    {
        [Key]
        public int C { get; set; }

        [Key]
        public int B { get; set; }

        [Key]
        public int A { get; set; }
    }

    public enum SmallNumber
    {
        One,
        Two,
        Three,
        Four
    }

    public abstract class OrderedItem
    {
        public int ExpectedOrder { get; set; }
    }
}
