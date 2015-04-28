using System;

namespace WebStack.QA.Test.OData.SxS2.ODataV4.Models
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

        public DateTimeOffset ManufactureDateTime
        {
            get;
            set;
        }
    }
}