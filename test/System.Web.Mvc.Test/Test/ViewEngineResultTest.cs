// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using Moq;
using Xunit;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Web.Mvc.Test
{
    public class ViewEngineResultTest
    {
        [Fact]
        public void ConstructorThrowsIfSearchedLocationsIsNull()
        {
            // Arrange
            string[] searchedLocations = null;

            // Act & Assert
            Assert.ThrowsArgumentNull(
                delegate { new ViewEngineResult(searchedLocations); }, "searchedLocations");
        }

        [Fact]
        public void ConstructorThrowsIfViewIsNull()
        {
            // Arrange
            IView view = null;

            // Act & Assert
            Assert.ThrowsArgumentNull(
                delegate { new ViewEngineResult(view, null); }, "view");
        }

        [Fact]
        public void ConstructorThrowsIfViewEngineIsNull()
        {
            // Arrange
            IView view = new Mock<IView>().Object;

            // Act & Assert
            Assert.ThrowsArgumentNull(
                delegate { new ViewEngineResult(view, null); }, "viewEngine");
        }

        [Fact]
        public void SearchedLocationsProperty()
        {
            // Arrange
            string[] searchedLocations = new string[0];
            ViewEngineResult result = new ViewEngineResult(searchedLocations);

            // Act & Assert
            Assert.Same(searchedLocations, result.SearchedLocations);
            Assert.Null(result.View);
        }

        [Fact]
        public void ViewProperty()
        {
            // Arrange
            IView view = new Mock<IView>().Object;
            IViewEngine viewEngine = new Mock<IViewEngine>().Object;
            ViewEngineResult result = new ViewEngineResult(view, viewEngine);

            // Act & Assert
            Assert.Same(view, result.View);
            Assert.Null(result.SearchedLocations);
        }

        [Fact]
        public void ViewEngineProperty()
        {
            // Arrange
            IView view = new Mock<IView>().Object;
            IViewEngine viewEngine = new Mock<IViewEngine>().Object;
            ViewEngineResult result = new ViewEngineResult(view, viewEngine);

            // Act & Assert
            Assert.Same(viewEngine, result.ViewEngine);
            Assert.Null(result.SearchedLocations);
        }
    }
}
