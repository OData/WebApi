using System.Collections.Generic;

namespace ROOT_PROJECT_NAMESPACE.Areas.HelpPage.ModelDescriptions
{
    public class EnumTypeModelDescription : ModelDescription
    {
        public EnumTypeModelDescription()
        {
            Values = new List<EnumValueDescription>();
        }

        public IList<EnumValueDescription> Values { get; private set; }
    }
}