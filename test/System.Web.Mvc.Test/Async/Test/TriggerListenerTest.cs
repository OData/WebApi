// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using Xunit;

namespace System.Web.Mvc.Async.Test
{
    public class TriggerListenerTest
    {
        [Fact]
        public void PerformTest()
        {
            // Arrange
            int count = 0;
            TriggerListener listener = new TriggerListener();
            Trigger trigger = listener.CreateTrigger();

            // Act & assert (hasn't fired yet)
            listener.SetContinuation(() => { count++; });
            listener.Activate();
            Assert.Equal(0, count);

            // Act & assert (fire it, get the callback)
            trigger.Fire();
            Assert.Equal(1, count);

            // Act & assert (fire again, but no callback since it already fired)
            Trigger trigger2 = listener.CreateTrigger();
            trigger2.Fire();
            Assert.Equal(1, count);
        }
    }
}
