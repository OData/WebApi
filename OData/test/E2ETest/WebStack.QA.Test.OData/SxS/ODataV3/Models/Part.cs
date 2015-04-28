using System;
using System.Collections.Generic;

namespace WebStack.QA.Test.OData.SxS.ODataV3.Models
{
    public class Part
    {
        public int PartId
        {
            get;
            set;
        }

        public DateTime ReleaseDateTime
        {
            get;
            set;
        }

        public virtual ICollection<Product> Products
        {
            get;
            set;
        }
    }
}