using System.Collections.Generic;

namespace WebStack.QA.Test.OData.Formatter.JsonLight.Metadata.Model
{
    public class EntityWithComplexProperties
    {
        public int Id { get; set; }
        public List<string> StringListProperty { get; set; }
        public ComplexType ComplexProperty { get; set; }
    }
}
