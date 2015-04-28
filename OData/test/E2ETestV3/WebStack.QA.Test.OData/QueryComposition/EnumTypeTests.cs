using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using Nuwa;
using WebStack.QA.Common.WebHost;
using WebStack.QA.Test.OData.Common;
using Xunit;
using Xunit.Extensions;

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
