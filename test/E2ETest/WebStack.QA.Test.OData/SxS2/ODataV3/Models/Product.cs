using System;

namespace WebStack.QA.Test.OData.SxS2.ODataV3.Models
{
    public class Product
    {
        public int Id
        {
            get;
            set;
        }

        public string Title
        {
            get; set;
        }

        public DateTime ManufactureDateTime
        {
            get;
            set;
        }
    }
}