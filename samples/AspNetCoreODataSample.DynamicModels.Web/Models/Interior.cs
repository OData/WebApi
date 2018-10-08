using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AspNetCoreODataSample.DynamicModels.Web.Models
{
    public class Interior : IUserDefinedPropertyBag
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [ForeignKey(nameof(Definition))]
        public int DefinitionID { get; set; }
        public InteriorDefinition Definition { get; set; }


        public string StringProperty1 { get; set; }
        public string StringProperty2 { get; set; }
        public string StringProperty3 { get; set; }
        public int IntProperty1 { get; set; }
        public int IntProperty2 { get; set; }
        public int IntProperty3 { get; set; }
        public double DoubleProperty1 { get; set; }
        public double DoubleProperty2 { get; set; }
        public double DoubleProperty3 { get; set; }
    }
}