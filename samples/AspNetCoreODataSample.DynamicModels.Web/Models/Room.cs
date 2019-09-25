using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AspNetCoreODataSample.DynamicModels.Web.Models
{
    public class Room
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        [ForeignKey(nameof(House))]
        public int HouseID { get; set; }
        public House House { get; set; }
        public string Name { get; set; }

        public List<Interior> Interior { get; set; }
    }
}
