// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Web.Razor.Utils;
using Xunit;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Web.Razor.Test.Utils
{
    public class DisposableActionTest
    {
        [Fact]
        public void ConstructorRequiresNonNullAction()
        {
            Assert.ThrowsArgumentNull(() => new DisposableAction(null), "action");
        }

        [Fact]
        public void ActionIsExecutedOnDispose()
        {
            // Arrange
            bool called = false;
            DisposableAction action = new DisposableAction(() => { called = true; });

            // Act
            action.Dispose();

            // Assert
            Assert.True(called, "The action was not run when the DisposableAction was disposed");
        }
    }
}
