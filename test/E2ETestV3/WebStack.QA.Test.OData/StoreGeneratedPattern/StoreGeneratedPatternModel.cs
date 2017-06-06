using System.ComponentModel.DataAnnotations.Schema;

namespace WebStack.QA.Test.OData.StoreGeneratedPattern
{
    public class StoreGeneratedPatternCustomer
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string Name { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public string ComputedProperty { get; set; }
    }
}
