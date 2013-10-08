using System.Collections.Generic;

namespace ROOT_PROJECT_NAMESPACE.Areas.HelpPage.ModelDescriptions
{
    public class ComplexTypeModelDescription : ModelDescription
    {
        public ComplexTypeModelDescription()
        {
            Properties = new List<PropertyDescription>();
        }

        public IList<PropertyDescription> Properties { get; private set; }
    }
}