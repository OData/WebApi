// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using Xunit;

namespace System.Web.Mvc.Async.Test
{
    public class SingleEntryGateTest
    {
        [Fact]
        public void TryEnterShouldBeTrueForFirstCallAndFalseForSubsequentCalls()
        {
            // Arrange
            SingleEntryGate gate = new SingleEntryGate();

            // Act
            bool firstCall = gate.TryEnter();
            bool secondCall = gate.TryEnter();
            bool thirdCall = gate.TryEnter();

            // Assert
            Assert.True(firstCall);
            Assert.False(secondCall);
            Assert.False(thirdCall);
        }
    }
}
