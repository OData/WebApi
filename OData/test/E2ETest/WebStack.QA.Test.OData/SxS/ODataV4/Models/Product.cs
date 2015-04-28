using System;

namespace WebStack.QA.Test.OData.SxS.ODataV4.Models
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