using WebStack.QA.Test.OData.Common;

namespace WebStack.QA.Test.OData.QueryComposition
{
    public enum EnumType_Type
    {
        Task,
        Reminder
    }
    public class EnumType_Todo
    {
        public int ID { get; set; }
        public EnumType_Type Type { get; set; }
    }

    public class EnumTypeTests : ODataTestBase
    {
    }
}
