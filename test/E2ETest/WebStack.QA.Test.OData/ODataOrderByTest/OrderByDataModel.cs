using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebStack.QA.Test.OData.ODataOrderByTest
{
    using System;

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
        public string ColumnA { get; set; }

        [Key]
        [Column(Order = 1)]
        public string ColumnC { get; set; }

        [Key]
        [Column(Order = 2)]
        public int ColumnB { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }
    }
}

