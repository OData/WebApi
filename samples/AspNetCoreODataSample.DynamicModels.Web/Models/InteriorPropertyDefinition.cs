using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AspNetCoreODataSample.DynamicModels.Web.Models
{
    public class InteriorPropertyDefinition
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [ForeignKey(nameof(Definition))]
        public int DefinitionID { get; set; }
        public InteriorDefinition Definition { get; set; }

        public string Name { get; set; }
        public InteriorPropertyType PropertyType { get; set; }
        public string PropertyName { get; set; }
    }
}