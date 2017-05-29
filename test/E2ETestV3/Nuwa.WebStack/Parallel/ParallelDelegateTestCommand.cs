using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Sdk;

namespace Nuwa.WebStack.Parallel
{
    public class ParallelDelegateTestCommand : TestCommand
    {
        private ITestCommand _inner;
        private int _count;

        public ParallelDelegateTestCommand(ITestCommand inner, IMethodInfo method, int count) 
            : base(method, inner.DisplayName, inner.Timeout)
        {
            _inner = inner;
            _count = count;
        }

        public override MethodResult Execute(object testClass)
        {
            MethodResult result = null;
            System.Threading.Tasks.Parallel.For(0, _count, i =>
                {
                    var temp = _inner.Execute(testClass);
                    if (result == null || temp is FailedResult)
                    {
                        result = temp;
                    }
                });

            return result;
        }
    }
}
