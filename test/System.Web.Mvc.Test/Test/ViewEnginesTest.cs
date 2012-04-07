// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using Xunit;

namespace System.Web.Mvc.Test
{
    public class ViewEnginesTest
    {
        [Fact]
        public void EnginesProperty()
        {
            // Act
            ViewEngineCollection collection = ViewEngines.Engines;

            // Assert
            Assert.Equal(2, collection.Count);
            Assert.IsType<WebFormViewEngine>(collection[0]);
            Assert.IsType<RazorViewEngine>(collection[1]);
        }
    }
}
