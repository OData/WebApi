using System.ComponentModel.DataAnnotations.Schema;

namespace WebStack.QA.Test.OData.Common.Models.ProductFamilies
{
    [ComplexType]
    public class Address
    {
        public string Street { get; set; }

        public string City { get; set; }

        public string State { get; set; }

        public string ZipCode { get; set; }

        public string CountryOrRegion { get; set; }
    }
}
