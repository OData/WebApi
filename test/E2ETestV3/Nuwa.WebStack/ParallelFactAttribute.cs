using Nuwa.WebStack.Parallel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Sdk;

namespace Nuwa
{
    public class ParallelFactAttribute : FactAttribute
    {
        public ParallelFactAttribute()
        {
            Count = 5;
        }

        public int Count { get; set; }

        protected override IEnumerable<ITestCommand> EnumerateTestCommands(IMethodInfo method)
        {
            return base.EnumerateTestCommands(method).Select(command => 
                new ParallelDelegateTestCommand(command, method, Count));

        }
    }
}
