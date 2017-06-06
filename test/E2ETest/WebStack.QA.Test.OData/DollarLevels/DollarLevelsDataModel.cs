using System.Collections.Generic;

namespace WebStack.QA.Test.OData.DollarLevels
{
    public class DLManager
    {
        public int ID { get; set; }

        public string Name { get; set; }

        public DLManager Manager { get; set; }

        public IList<DLManager> DirectReports { get; set; }
    }

    public class DLEmployee
    {
        public int ID { get; set; }

        public DLEmployee Friend { get; set; }
    }
}
