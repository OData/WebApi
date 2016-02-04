using System.ComponentModel.DataAnnotations.Schema;

namespace ODataSample.Web.Models
{
    public class Product
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public double Price { get; set; }
    }
}
