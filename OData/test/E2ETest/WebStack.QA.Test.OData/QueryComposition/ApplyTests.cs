using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using WebStack.QA.Test.OData.Common;
using Xunit;

namespace WebStack.QA.Test.OData.QueryComposition
{
    public class ApplyTests : ODataTestBase
    {
        [Fact]
        public void CanApply()
        {
            var response = this.Client.GetAsync(this.BaseAddress + "/api/ApplyTests/Get?$apply=groupby((Name))").Result;
            var result = response.Content.ReadAsAsync<IEnumerable<dynamic>>().Result;

        }
    }
}
