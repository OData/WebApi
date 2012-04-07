// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections;
using System.IO;
using System.Linq;
using System.Web.Hosting;
using System.Web.Routing;
using System.Web.WebPages;
using Moq;
using Xunit;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Web.Mvc.Test
{
    public class VirtualPathProviderViewEngineTest
    {
        [Fact]
        public void FindView_NullControllerContext_Throws()
        {
            // Arrange
            TestableVirtualPathProviderViewEngine engine = new TestableVirtualPathProviderViewEngine();

            // Act & Assert
            Assert.ThrowsArgumentNull(
                () => engine.FindView(null, "view name", null, false),
                "controllerContext"
                );
        }

        [Fact]
        public void FindView_NullViewName_Throws()
        {
            // Arrange
            ControllerContext context = CreateContext();
            TestableVirtualPathProviderViewEngine engine = new TestableVirtualPathProviderViewEngine();

            // Act & Assert
            Assert.ThrowsArgumentNullOrEmpty(
                () => engine.FindView(context, null, null, false),
                "viewName"
                );
        }

        [Fact]
        public void FindView_EmptyViewName_Throws()
        {
            // Arrange
            ControllerContext context = CreateContext();
            TestableVirtualPathProviderViewEngine engine = new TestableVirtualPathProviderViewEngine();

            // Act & Assert
            Assert.ThrowsArgumentNullOrEmpty(
                () => engine.FindView(context, "", null, false),
                "viewName"
                );
        }

        [Fact]
        public void FindView_ControllerNameNotInRequestContext_Throws()
        {
            // Arrange
            TestableVirtualPathProviderViewEngine engine = new TestableVirtualPathProviderViewEngine();
            ControllerContext context = CreateContext();
            context.RouteData.Values.Remove("controller");

            // Act & Assert
            Assert.Throws<InvalidOperationException>(
                () => engine.FindView(context, "viewName", null, false),
                "The RouteData must contain an item named 'controller' with a non-empty string value."
                );
        }

        [Fact]
        public void FindView_EmptyViewLocations_Throws()
        {
            // Arrange
            ControllerContext context = CreateContext();
            TestableVirtualPathProviderViewEngine engine = new TestableVirtualPathProviderViewEngine();
            engine.ClearViewLocations();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(
                () => engine.FindView(context, "viewName", null, false),
                "The property 'ViewLocationFormats' cannot be null or empty."
                );
        }

        [Fact]
        public void FindView_ViewDoesNotExistAndNoMaster_ReturnsSearchedLocationsResult()
        {
            // Arrange
            ControllerContext context = CreateContext();
            TestableVirtualPathProviderViewEngine engine = new TestableVirtualPathProviderViewEngine();
            engine.MockPathProvider
                .Setup(vpp => vpp.FileExists("~/vpath/controllerName/viewName.view"))
                .Returns(false)
                .Verifiable();

            // Act
            ViewEngineResult result = engine.FindView(context, "viewName", null, false);

            // Assert
            Assert.Null(result.View);
            Assert.Single(result.SearchedLocations);
            Assert.True(result.SearchedLocations.Contains("~/vpath/controllerName/viewName.view"));
            engine.MockPathProvider.Verify();
        }

        [Fact]
        public void FindView_VirtualPathViewDoesNotExistAndNoMaster_ReturnsSearchedLocationsResult()
        {
            // Arrange
            ControllerContext context = CreateContext();
            TestableVirtualPathProviderViewEngine engine = new TestableVirtualPathProviderViewEngine();
            engine.MockPathProvider
                .Setup(vpp => vpp.FileExists("~/foo/bar.view"))
                .Returns(false)
                .Verifiable();
            engine.MockCache
                .Setup(c => c.InsertViewLocation(It.IsAny<HttpContextBase>(), It.IsAny<string>(), ""))
                .Verifiable();

            // Act
            ViewEngineResult result = engine.FindView(context, "~/foo/bar.view", null, false);

            // Assert
            Assert.Null(result.View);
            Assert.Single(result.SearchedLocations);
            Assert.True(result.SearchedLocations.Contains("~/foo/bar.view"));
            engine.MockPathProvider.Verify();
            engine.MockCache.Verify();
        }

        [Fact]
        public void FindView_VirtualPathViewNotSupportedAndNoMaster_ReturnsSearchedLocationsResult()
        {
            // Arrange
            ControllerContext context = CreateContext();
            TestableVirtualPathProviderViewEngine engine = new TestableVirtualPathProviderViewEngine();
            engine.MockCache
                .Setup(c => c.InsertViewLocation(It.IsAny<HttpContextBase>(), It.IsAny<string>(), ""))
                .Verifiable();

            // Act
            ViewEngineResult result = engine.FindView(context, "~/foo/bar.unsupported", null, false);

            // Assert
            Assert.Null(result.View);
            Assert.Single(result.SearchedLocations);
            Assert.True(result.SearchedLocations.Contains("~/foo/bar.unsupported"));
            engine.MockPathProvider.Verify(vpp => vpp.FileExists("~/foo/bar.unsupported"), Times.Never());
            engine.MockCache.Verify();
        }

        [Fact]
        public void FindView_AbsolutePathViewDoesNotExistAndNoMaster_ReturnsSearchedLocationsResult()
        {
            // Arrange
            ControllerContext context = CreateContext();
            TestableVirtualPathProviderViewEngine engine = new TestableVirtualPathProviderViewEngine();
            engine.MockPathProvider
                .Setup(vpp => vpp.FileExists("/foo/bar.view"))
                .Returns(false)
                .Verifiable();
            engine.MockCache
                .Setup(c => c.InsertViewLocation(It.IsAny<HttpContextBase>(), It.IsAny<string>(), ""))
                .Verifiable();

            // Act
            ViewEngineResult result = engine.FindView(context, "/foo/bar.view", null, false);

            // Assert
            Assert.Null(result.View);
            Assert.Single(result.SearchedLocations);
            Assert.True(result.SearchedLocations.Contains("/foo/bar.view"));
            engine.MockPathProvider.Verify();
            engine.MockCache.Verify();
        }

        [Fact]
        public void FindView_AbsolutePathViewNotSupportedAndNoMaster_ReturnsSearchedLocationsResult()
        {
            // Arrange
            ControllerContext context = CreateContext();
            TestableVirtualPathProviderViewEngine engine = new TestableVirtualPathProviderViewEngine();
            engine.MockCache
                .Setup(c => c.InsertViewLocation(It.IsAny<HttpContextBase>(), It.IsAny<string>(), ""))
                .Verifiable();

            // Act
            ViewEngineResult result = engine.FindView(context, "/foo/bar.unsupported", null, false);

            // Assert
            Assert.Null(result.View);
            Assert.Single(result.SearchedLocations);
            Assert.True(result.SearchedLocations.Contains("/foo/bar.unsupported"));
            engine.MockPathProvider.Verify(vpp => vpp.FileExists("/foo/bar.unsupported"), Times.Never());
            engine.MockCache.Verify();
        }

        [Fact]
        public void FindView_ViewExistsAndNoMaster_ReturnsView()
        {
            // Arrange
            ControllerContext context = CreateContext();
            TestableVirtualPathProviderViewEngine engine = new TestableVirtualPathProviderViewEngine();
            engine.ClearMasterLocations(); // If master is not provided, master locations can be empty
            engine.MockPathProvider
                .Setup(vpp => vpp.FileExists("~/vpath/controllerName/viewName.view"))
                .Returns(true)
                .Verifiable();
            engine.MockPathProvider
                .Setup(vpp => vpp.FileExists("~/vpath/controllerName/viewName.Mobile.view"))
                .Returns(false)
                .Verifiable();
            engine.MockCache
                .Setup(c => c.InsertViewLocation(It.IsAny<HttpContextBase>(), It.IsAny<string>(), "~/vpath/controllerName/viewName.view"))
                .Verifiable();

            // Act
            ViewEngineResult result = engine.FindView(context, "viewName", null, false);

            // Assert
            Assert.Same(engine.CreateViewResult, result.View);
            Assert.Null(result.SearchedLocations);
            Assert.Same(context, engine.CreateViewControllerContext);
            Assert.Equal("~/vpath/controllerName/viewName.view", engine.CreateViewViewPath);
            Assert.Equal(String.Empty, engine.CreateViewMasterPath);
            engine.MockPathProvider.Verify();
            engine.MockCache.Verify();
        }

        [Fact]
        public void FindView_VirtualPathViewExistsAndNoMaster_ReturnsView()
        {
            // Arrange
            ControllerContext context = CreateContext();
            TestableVirtualPathProviderViewEngine engine = new TestableVirtualPathProviderViewEngine();
            engine.ClearMasterLocations();
            engine.MockPathProvider
                .Setup(vpp => vpp.FileExists("~/foo/bar.view"))
                .Returns(true)
                .Verifiable();
            engine.MockCache
                .Setup(c => c.InsertViewLocation(It.IsAny<HttpContextBase>(), It.IsAny<string>(), "~/foo/bar.view"))
                .Verifiable();

            // Act
            ViewEngineResult result = engine.FindView(context, "~/foo/bar.view", null, false);

            // Assert
            Assert.Same(engine.CreateViewResult, result.View);
            Assert.Null(result.SearchedLocations);
            Assert.Same(context, engine.CreateViewControllerContext);
            Assert.Equal("~/foo/bar.view", engine.CreateViewViewPath);
            Assert.Equal(String.Empty, engine.CreateViewMasterPath);
            engine.MockPathProvider.Verify();
            engine.MockCache.Verify();
        }

        [Fact]
        public void FindView_VirtualPathViewExistsAndNoMaster_Legacy_ReturnsView()
        {
            // Arrange
            ControllerContext context = CreateContext();
            TestableVirtualPathProviderViewEngine engine = new TestableVirtualPathProviderViewEngine()
            {
                FileExtensions = null, // Set FileExtensions to null to simulate View Engines that do not set this property            
            };
            engine.ClearMasterLocations();
            engine.MockPathProvider
                .Setup(vpp => vpp.FileExists("~/foo/bar.unsupported"))
                .Returns(true)
                .Verifiable();
            engine.MockCache
                .Setup(c => c.InsertViewLocation(It.IsAny<HttpContextBase>(), It.IsAny<string>(), "~/foo/bar.unsupported"))
                .Verifiable();

            // Act
            ViewEngineResult result = engine.FindView(context, "~/foo/bar.unsupported", null, false);

            // Assert
            Assert.Same(engine.CreateViewResult, result.View);
            Assert.Null(result.SearchedLocations);
            Assert.Same(context, engine.CreateViewControllerContext);
            Assert.Equal("~/foo/bar.unsupported", engine.CreateViewViewPath);
            Assert.Equal(String.Empty, engine.CreateViewMasterPath);
            engine.MockPathProvider.Verify();
            engine.MockCache.Verify();
        }

        [Fact]
        public void FindView_AbsolutePathViewExistsAndNoMaster_ReturnsView()
        {
            // Arrange
            ControllerContext context = CreateContext();
            TestableVirtualPathProviderViewEngine engine = new TestableVirtualPathProviderViewEngine();
            engine.ClearMasterLocations();
            engine.MockPathProvider
                .Setup(vpp => vpp.FileExists("/foo/bar.view"))
                .Returns(true)
                .Verifiable();
            engine.MockCache
                .Setup(c => c.InsertViewLocation(It.IsAny<HttpContextBase>(), It.IsAny<string>(), "/foo/bar.view"))
                .Verifiable();

            // Act
            ViewEngineResult result = engine.FindView(context, "/foo/bar.view", null, false);

            // Assert
            Assert.Same(engine.CreateViewResult, result.View);
            Assert.Null(result.SearchedLocations);
            Assert.Same(context, engine.CreateViewControllerContext);
            Assert.Equal("/foo/bar.view", engine.CreateViewViewPath);
            Assert.Equal(String.Empty, engine.CreateViewMasterPath);
            engine.MockPathProvider.Verify();
            engine.MockCache.Verify();
        }

        [Fact]
        public void FindView_AbsolutePathViewExistsAndNoMaster_Legacy_ReturnsView()
        {
            // Arrange
            ControllerContext context = CreateContext();
            TestableVirtualPathProviderViewEngine engine = new TestableVirtualPathProviderViewEngine()
            {
                FileExtensions = null, // Set FileExtensions to null to simulate View Engines that do not set this property
            };
            engine.ClearMasterLocations();
            engine.MockPathProvider
                .Setup(vpp => vpp.FileExists("/foo/bar.unsupported"))
                .Returns(true)
                .Verifiable();
            engine.MockCache
                .Setup(c => c.InsertViewLocation(It.IsAny<HttpContextBase>(), It.IsAny<string>(), "/foo/bar.unsupported"))
                .Verifiable();

            // Act
            ViewEngineResult result = engine.FindView(context, "/foo/bar.unsupported", null, false);

            // Assert
            Assert.Same(engine.CreateViewResult, result.View);
            Assert.Null(result.SearchedLocations);
            Assert.Same(context, engine.CreateViewControllerContext);
            Assert.Equal("/foo/bar.unsupported", engine.CreateViewViewPath);
            Assert.Equal(String.Empty, engine.CreateViewMasterPath);
            engine.MockPathProvider.Verify();
            engine.MockCache.Verify();
        }

        [Fact]
        public void FindView_ViewExistsAndMasterNameProvidedButEmptyMasterLocations_Throws()
        {
            // Arrange
            ControllerContext context = CreateContext();
            TestableVirtualPathProviderViewEngine engine = new TestableVirtualPathProviderViewEngine();
            engine.ClearMasterLocations();
            engine.MockPathProvider
                .Setup(vpp => vpp.FileExists("~/vpath/controllerName/viewName.view"))
                .Returns(true)
                .Verifiable();
            engine.MockCache
                .Setup(c => c.InsertViewLocation(It.IsAny<HttpContextBase>(), It.IsAny<string>(), "~/vpath/controllerName/viewName.view"))
                .Verifiable();
            engine.MockPathProvider
                .Setup(vpp => vpp.FileExists("~/vpath/controllerName/viewName.Mobile.view"))
                .Returns(false)
                .Verifiable();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(
                () => engine.FindView(context, "viewName", "masterName", false),
                "The property 'MasterLocationFormats' cannot be null or empty."
                );
            engine.MockPathProvider.Verify();
            engine.MockCache.Verify();
        }

        [Fact]
        public void FindView_ViewDoesNotExistAndMasterDoesNotExist_ReturnsSearchedLocationsResult()
        {
            // Arrange
            ControllerContext context = CreateContext();
            TestableVirtualPathProviderViewEngine engine = new TestableVirtualPathProviderViewEngine();
            engine.MockPathProvider
                .Setup(vpp => vpp.FileExists("~/vpath/controllerName/viewName.view"))
                .Returns(false)
                .Verifiable();
            engine.MockPathProvider
                .Setup(vpp => vpp.FileExists("~/vpath/controllerName/masterName.master"))
                .Returns(false)
                .Verifiable();

            // Act
            ViewEngineResult result = engine.FindView(context, "viewName", "masterName", false);

            // Assert
            Assert.Null(result.View);
            Assert.Equal(2, result.SearchedLocations.Count()); // Both view and master locations
            Assert.True(result.SearchedLocations.Contains("~/vpath/controllerName/viewName.view"));
            Assert.True(result.SearchedLocations.Contains("~/vpath/controllerName/masterName.master"));
            engine.MockPathProvider.Verify();
        }

        [Fact]
        public void FindView_ViewExistsButMasterDoesNotExist_ReturnsSearchedLocationsResult()
        {
            // Arrange
            ControllerContext context = CreateContext();
            TestableVirtualPathProviderViewEngine engine = new TestableVirtualPathProviderViewEngine();
            engine.MockPathProvider
                .Setup(vpp => vpp.FileExists("~/vpath/controllerName/viewName.view"))
                .Returns(true)
                .Verifiable();
            engine.MockPathProvider
                .Setup(vpp => vpp.FileExists("~/vpath/controllerName/viewName.Mobile.view"))
                .Returns(false)
                .Verifiable();
            engine.MockCache
                .Setup(c => c.InsertViewLocation(It.IsAny<HttpContextBase>(), It.IsAny<string>(), "~/vpath/controllerName/viewName.view"))
                .Verifiable();
            engine.MockPathProvider
                .Setup(vpp => vpp.FileExists("~/vpath/controllerName/masterName.master"))
                .Returns(false)
                .Verifiable();

            // Act
            ViewEngineResult result = engine.FindView(context, "viewName", "masterName", false);

            // Assert
            Assert.Null(result.View);
            Assert.Single(result.SearchedLocations); // View was found, not included in 'searched locations'
            Assert.True(result.SearchedLocations.Contains("~/vpath/controllerName/masterName.master"));
            engine.MockPathProvider.Verify();
            engine.MockCache.Verify();
        }

        [Fact]
        public void FindView_MasterInAreaDoesNotExist_ReturnsSearchedLocationsResult()
        {
            // Arrange
            ControllerContext context = CreateContext();
            context.RouteData.DataTokens["area"] = "areaName";

            TestableVirtualPathProviderViewEngine engine = new TestableVirtualPathProviderViewEngine();
            engine.MockPathProvider
                .Setup(vpp => vpp.FileExists("~/vpath/areaName/controllerName/viewName.view"))
                .Returns(true)
                .Verifiable();
            engine.MockCache
                .Setup(c => c.InsertViewLocation(It.IsAny<HttpContextBase>(), It.IsAny<string>(), "~/vpath/areaName/controllerName/viewName.view"))
                .Verifiable();
            engine.MockPathProvider
                .Setup(vpp => vpp.FileExists("~/vpath/areaName/controllerName/viewName.Mobile.view"))
                .Returns(false)
                .Verifiable();
            engine.MockPathProvider
                .Setup(vpp => vpp.FileExists("~/vpath/areaName/controllerName/masterName.master"))
                .Returns(false)
                .Verifiable();
            engine.MockPathProvider
                .Setup(vpp => vpp.FileExists("~/vpath/controllerName/masterName.master"))
                .Returns(false)
                .Verifiable();

            // Act
            ViewEngineResult result = engine.FindView(context, "viewName", "masterName", false);

            // Assert
            Assert.Null(result.View);
            Assert.Equal(2, result.SearchedLocations.Count()); // View was found, not included in 'searched locations'
            Assert.True(result.SearchedLocations.Contains("~/vpath/areaName/controllerName/masterName.master"));
            Assert.True(result.SearchedLocations.Contains("~/vpath/controllerName/masterName.master"));
            engine.MockPathProvider.Verify();
            engine.MockCache.Verify();
        }

        [Fact]
        public void FindView_ViewExistsAndMasterExists_ReturnsView()
        {
            // Arrange
            ControllerContext context = CreateContext();
            TestableVirtualPathProviderViewEngine engine = new TestableVirtualPathProviderViewEngine();
            engine.MockPathProvider
                .Setup(vpp => vpp.FileExists("~/vpath/controllerName/viewName.view"))
                .Returns(true)
                .Verifiable();
            engine.MockCache
                .Setup(c => c.InsertViewLocation(It.IsAny<HttpContextBase>(), It.IsAny<string>(), "~/vpath/controllerName/viewName.view"))
                .Verifiable();
            engine.MockPathProvider
                .Setup(vpp => vpp.FileExists("~/vpath/controllerName/viewName.Mobile.view"))
                .Returns(false)
                .Verifiable();
            engine.MockPathProvider
                .Setup(vpp => vpp.FileExists("~/vpath/controllerName/masterName.master"))
                .Returns(true)
                .Verifiable();
            engine.MockCache
                .Setup(c => c.InsertViewLocation(It.IsAny<HttpContextBase>(), It.IsAny<string>(), "~/vpath/controllerName/masterName.master"))
                .Verifiable();
            engine.MockPathProvider
                .Setup(vpp => vpp.FileExists("~/vpath/controllerName/masterName.Mobile.master"))
                .Returns(false)
                .Verifiable();

            // Act
            ViewEngineResult result = engine.FindView(context, "viewName", "masterName", false);

            // Assert
            Assert.Same(engine.CreateViewResult, result.View);
            Assert.Null(result.SearchedLocations);
            Assert.Same(context, engine.CreateViewControllerContext);
            Assert.Equal("~/vpath/controllerName/viewName.view", engine.CreateViewViewPath);
            Assert.Equal("~/vpath/controllerName/masterName.master", engine.CreateViewMasterPath);
            engine.MockPathProvider.Verify();
            engine.MockCache.Verify();
        }

        [Fact]
        public void FindView_ViewInAreaExistsAndMasterExists_ReturnsView()
        {
            // Arrange
            ControllerContext context = CreateContext();
            context.RouteData.DataTokens["area"] = "areaName";

            TestableVirtualPathProviderViewEngine engine = new TestableVirtualPathProviderViewEngine();
            engine.MockPathProvider
                .Setup(vpp => vpp.FileExists("~/vpath/areaName/controllerName/viewName.view"))
                .Returns(true)
                .Verifiable();
            engine.MockCache
                .Setup(c => c.InsertViewLocation(It.IsAny<HttpContextBase>(), It.IsAny<string>(), "~/vpath/areaName/controllerName/viewName.view"))
                .Verifiable();
            engine.MockPathProvider
                .Setup(vpp => vpp.FileExists("~/vpath/areaName/controllerName/viewName.Mobile.view"))
                .Returns(false)
                .Verifiable();
            engine.MockPathProvider
                .Setup(vpp => vpp.FileExists("~/vpath/areaName/controllerName/masterName.master"))
                .Returns(false)
                .Verifiable();
            engine.MockPathProvider
                .Setup(vpp => vpp.FileExists("~/vpath/controllerName/masterName.master"))
                .Returns(true)
                .Verifiable();
            engine.MockCache
                .Setup(c => c.InsertViewLocation(It.IsAny<HttpContextBase>(), It.IsAny<string>(), "~/vpath/controllerName/masterName.master"))
                .Verifiable();
            engine.MockPathProvider
                .Setup(vpp => vpp.FileExists("~/vpath/controllerName/masterName.Mobile.master"))
                .Returns(false)
                .Verifiable();

            // Act
            ViewEngineResult result = engine.FindView(context, "viewName", "masterName", false);

            // Assert
            Assert.Same(engine.CreateViewResult, result.View);
            Assert.Null(result.SearchedLocations);
            Assert.Same(context, engine.CreateViewControllerContext);
            Assert.Equal("~/vpath/areaName/controllerName/viewName.view", engine.CreateViewViewPath);
            Assert.Equal("~/vpath/controllerName/masterName.master", engine.CreateViewMasterPath);
            engine.MockPathProvider.Verify();
            engine.MockCache.Verify();
        }

        [Fact]
        public void FindView_ViewInAreaExistsAndMasterExists_ReturnsView_Mobile()
        {
            // Arrange
            ControllerContext context = CreateContext(isMobileDevice: true);
            context.RouteData.DataTokens["area"] = "areaName";

            TestableVirtualPathProviderViewEngine engine = new TestableVirtualPathProviderViewEngine();
            engine.MockPathProvider
                .Setup(vpp => vpp.FileExists("~/vpath/areaName/controllerName/viewName.view"))
                .Returns(true)
                .Verifiable();
            engine.MockCache
                .Setup(c => c.InsertViewLocation(It.IsAny<HttpContextBase>(), It.IsAny<string>(), "~/vpath/areaName/controllerName/viewName.view"))
                .Verifiable();

            engine.MockPathProvider
                .Setup(vpp => vpp.FileExists("~/vpath/areaName/controllerName/viewName.Mobile.view"))
                .Returns(true)
                .Verifiable();
            engine.MockCache
                .Setup(c => c.InsertViewLocation(It.IsAny<HttpContextBase>(), It.IsAny<string>(), "~/vpath/areaName/controllerName/viewName.Mobile.view"))
                .Verifiable();

            engine.MockPathProvider
                .Setup(vpp => vpp.FileExists("~/vpath/areaName/controllerName/masterName.master"))
                .Returns(false)
                .Verifiable();
            engine.MockPathProvider
                .Setup(vpp => vpp.FileExists("~/vpath/areaName/controllerName/masterName.Mobile.master"))
                .Returns(false)
                .Verifiable();

            engine.MockPathProvider
                .Setup(vpp => vpp.FileExists("~/vpath/controllerName/masterName.master"))
                .Returns(true)
                .Verifiable();
            engine.MockCache
                .Setup(c => c.InsertViewLocation(It.IsAny<HttpContextBase>(), It.IsAny<string>(), "~/vpath/controllerName/masterName.master"))
                .Verifiable();

            engine.MockPathProvider
                .Setup(vpp => vpp.FileExists("~/vpath/controllerName/masterName.Mobile.master"))
                .Returns(true)
                .Verifiable();
            engine.MockCache
                .Setup(c => c.InsertViewLocation(It.IsAny<HttpContextBase>(), It.IsAny<string>(), "~/vpath/controllerName/masterName.Mobile.master"))
                .Verifiable();

            // Act
            ViewEngineResult result = engine.FindView(context, "viewName", "masterName", false);

            // Assert
            Assert.Same(engine.CreateViewResult, result.View);
            Assert.Null(result.SearchedLocations);
            Assert.Same(context, engine.CreateViewControllerContext);
            Assert.Equal("~/vpath/areaName/controllerName/viewName.Mobile.view", engine.CreateViewViewPath);
            Assert.Equal("~/vpath/controllerName/masterName.Mobile.master", engine.CreateViewMasterPath);
            engine.MockPathProvider.Verify();
            engine.MockCache.Verify();
        }

        [Fact]
        public void FindPartialView_NullControllerContext_Throws()
        {
            // Arrange
            TestableVirtualPathProviderViewEngine engine = new TestableVirtualPathProviderViewEngine();

            // Act & Assert
            Assert.ThrowsArgumentNull(
                () => engine.FindPartialView(null, "view name", false),
                "controllerContext"
                );
        }

        [Fact]
        public void FindPartialView_NullPartialViewName_Throws()
        {
            // Arrange
            ControllerContext context = CreateContext();
            TestableVirtualPathProviderViewEngine engine = new TestableVirtualPathProviderViewEngine();

            // Act & Assert
            Assert.ThrowsArgumentNullOrEmpty(
                () => engine.FindPartialView(context, null, false),
                "partialViewName"
                );
        }

        [Fact]
        public void FindPartialView_EmptyPartialViewName_Throws()
        {
            // Arrange
            ControllerContext context = CreateContext();
            TestableVirtualPathProviderViewEngine engine = new TestableVirtualPathProviderViewEngine();

            // Act & Assert
            Assert.ThrowsArgumentNullOrEmpty(
                () => engine.FindPartialView(context, "", false),
                "partialViewName"
                );
        }

        [Fact]
        public void FindPartialView_ControllerNameNotInRequestContext_Throws()
        {
            // Arrange
            TestableVirtualPathProviderViewEngine engine = new TestableVirtualPathProviderViewEngine();
            ControllerContext context = CreateContext();
            context.RouteData.Values.Remove("controller");

            // Act & Assert
            Assert.Throws<InvalidOperationException>(
                () => engine.FindPartialView(context, "partialName", false),
                "The RouteData must contain an item named 'controller' with a non-empty string value."
                );
        }

        [Fact]
        public void FindPartialView_EmptyPartialViewLocations_Throws()
        {
            // Arrange
            ControllerContext context = CreateContext();
            TestableVirtualPathProviderViewEngine engine = new TestableVirtualPathProviderViewEngine();
            engine.ClearPartialViewLocations();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(
                () => engine.FindPartialView(context, "partialName", false),
                "The property 'PartialViewLocationFormats' cannot be null or empty."
                );
        }

        [Fact]
        public void FindPartialView_ViewDoesNotExist_ReturnsSearchLocationsResult()
        {
            // Arrange
            ControllerContext context = CreateContext();
            TestableVirtualPathProviderViewEngine engine = new TestableVirtualPathProviderViewEngine();
            engine.MockPathProvider
                .Setup(vpp => vpp.FileExists("~/vpath/controllerName/partialName.partial"))
                .Returns(false)
                .Verifiable();

            // Act
            ViewEngineResult result = engine.FindPartialView(context, "partialName", false);

            // Assert
            Assert.Null(result.View);
            Assert.Single(result.SearchedLocations);
            Assert.True(result.SearchedLocations.Contains("~/vpath/controllerName/partialName.partial"));
            engine.MockPathProvider.Verify();
        }

        [Fact]
        public void FindPartialView_VirtualPathViewExists_Legacy_ReturnsView()
        {
            // Arrange
            ControllerContext context = CreateContext();
            TestableVirtualPathProviderViewEngine engine = new TestableVirtualPathProviderViewEngine()
            {
                FileExtensions = null, // Set FileExtensions to null to simulate View Engines that do not set this property
            };
            engine.MockPathProvider
                .Setup(vpp => vpp.FileExists("~/foo/bar.unsupported"))
                .Returns(true)
                .Verifiable();
            engine.MockCache
                .Setup(c => c.InsertViewLocation(It.IsAny<HttpContextBase>(), It.IsAny<string>(), "~/foo/bar.unsupported"))
                .Verifiable();

            // Act
            ViewEngineResult result = engine.FindPartialView(context, "~/foo/bar.unsupported", false);

            // Assert
            Assert.Same(engine.CreatePartialViewResult, result.View);
            Assert.Null(result.SearchedLocations);
            Assert.Same(context, engine.CreatePartialViewControllerContext);
            Assert.Equal("~/foo/bar.unsupported", engine.CreatePartialViewPartialPath);
            engine.MockPathProvider.Verify();
            engine.MockCache.Verify();
        }

        [Fact]
        public void FindPartialView_VirtualPathViewDoesNotExist_ReturnsSearchedLocationsResult()
        {
            // Arrange
            ControllerContext context = CreateContext();
            TestableVirtualPathProviderViewEngine engine = new TestableVirtualPathProviderViewEngine();
            engine.MockPathProvider
                .Setup(vpp => vpp.FileExists("~/foo/bar.partial"))
                .Returns(false)
                .Verifiable();
            engine.MockCache
                .Setup(c => c.InsertViewLocation(It.IsAny<HttpContextBase>(), It.IsAny<string>(), ""))
                .Verifiable();

            // Act
            ViewEngineResult result = engine.FindPartialView(context, "~/foo/bar.partial", false);

            // Assert
            Assert.Null(result.View);
            Assert.Single(result.SearchedLocations);
            Assert.True(result.SearchedLocations.Contains("~/foo/bar.partial"));
            engine.MockPathProvider.Verify();
            engine.MockCache.Verify();
        }

        [Fact]
        public void FindPartialView_VirtualPathViewNotSupported_ReturnsSearchedLocationsResult()
        {
            // Arrange
            ControllerContext context = CreateContext();
            TestableVirtualPathProviderViewEngine engine = new TestableVirtualPathProviderViewEngine();
            engine.MockCache
                .Setup(c => c.InsertViewLocation(It.IsAny<HttpContextBase>(), It.IsAny<string>(), ""))
                .Verifiable();

            // Act
            ViewEngineResult result = engine.FindPartialView(context, "~/foo/bar.unsupported", false);

            // Assert
            Assert.Null(result.View);
            Assert.Single(result.SearchedLocations);
            Assert.True(result.SearchedLocations.Contains("~/foo/bar.unsupported"));
            engine.MockPathProvider.Verify(vpp => vpp.FileExists("~/foo/bar.unsupported"), Times.Never());
            engine.MockCache.Verify();
        }

        [Fact]
        public void FindPartialView_AbsolutePathViewDoesNotExist_ReturnsSearchedLocationsResult()
        {
            // Arrange
            ControllerContext context = CreateContext();
            TestableVirtualPathProviderViewEngine engine = new TestableVirtualPathProviderViewEngine();
            engine.MockPathProvider
                .Setup(vpp => vpp.FileExists("/foo/bar.partial"))
                .Returns(false)
                .Verifiable();
            engine.MockCache
                .Setup(c => c.InsertViewLocation(It.IsAny<HttpContextBase>(), It.IsAny<string>(), ""))
                .Verifiable();

            // Act
            ViewEngineResult result = engine.FindPartialView(context, "/foo/bar.partial", false);

            // Assert
            Assert.Null(result.View);
            Assert.Single(result.SearchedLocations);
            Assert.True(result.SearchedLocations.Contains("/foo/bar.partial"));
            engine.MockPathProvider.Verify();
            engine.MockCache.Verify();
        }

        [Fact]
        public void FindPartialView_AbsolutePathViewNotSupported_ReturnsSearchedLocationsResult()
        {
            // Arrange
            ControllerContext context = CreateContext();
            TestableVirtualPathProviderViewEngine engine = new TestableVirtualPathProviderViewEngine();
            engine.MockCache
                .Setup(c => c.InsertViewLocation(It.IsAny<HttpContextBase>(), It.IsAny<string>(), ""))
                .Verifiable();

            // Act
            ViewEngineResult result = engine.FindPartialView(context, "/foo/bar.unsupported", false);

            // Assert
            Assert.Null(result.View);
            Assert.Single(result.SearchedLocations);
            Assert.True(result.SearchedLocations.Contains("/foo/bar.unsupported"));
            engine.MockPathProvider.Verify<bool>(vpp => vpp.FileExists("/foo/bar.unsupported"), Times.Never());
            engine.MockCache.Verify();
        }

        [Fact]
        public void FindPartialView_AbsolutePathViewExists_Legacy_ReturnsView()
        {
            // Arrange
            ControllerContext context = CreateContext();
            TestableVirtualPathProviderViewEngine engine = new TestableVirtualPathProviderViewEngine()
            {
                FileExtensions = null, // Set FileExtensions to null to simulate View Engines that do not set this property
            };
            engine.MockPathProvider
                .Setup(vpp => vpp.FileExists("/foo/bar.unsupported"))
                .Returns(true)
                .Verifiable();
            engine.MockCache
                .Setup(c => c.InsertViewLocation(It.IsAny<HttpContextBase>(), It.IsAny<string>(), "/foo/bar.unsupported"))
                .Verifiable();

            // Act
            ViewEngineResult result = engine.FindPartialView(context, "/foo/bar.unsupported", false);

            // Assert
            Assert.Same(engine.CreatePartialViewResult, result.View);
            Assert.Null(result.SearchedLocations);
            Assert.Same(context, engine.CreatePartialViewControllerContext);
            Assert.Equal("/foo/bar.unsupported", engine.CreatePartialViewPartialPath);
            engine.MockPathProvider.Verify();
            engine.MockCache.Verify();
        }

        [Fact]
        public void FindPartialView_ViewExists_ReturnsView()
        {
            // Arrange
            ControllerContext context = CreateContext();
            TestableVirtualPathProviderViewEngine engine = new TestableVirtualPathProviderViewEngine();
            engine.MockPathProvider
                .Setup(vpp => vpp.FileExists("~/vpath/controllerName/partialName.partial"))
                .Returns(true)
                .Verifiable();
            engine.MockCache
                .Setup(c => c.InsertViewLocation(It.IsAny<HttpContextBase>(), It.IsAny<string>(), "~/vpath/controllerName/partialName.partial"))
                .Verifiable();
            engine.MockPathProvider
                .Setup(vpp => vpp.FileExists("~/vpath/controllerName/partialName.Mobile.partial"))
                .Returns(false)
                .Verifiable();

            // Act
            ViewEngineResult result = engine.FindPartialView(context, "partialName", false);

            // Assert
            Assert.Same(engine.CreatePartialViewResult, result.View);
            Assert.Null(result.SearchedLocations);
            Assert.Same(context, engine.CreatePartialViewControllerContext);
            Assert.Equal("~/vpath/controllerName/partialName.partial", engine.CreatePartialViewPartialPath);
            engine.MockPathProvider.Verify();
            engine.MockCache.Verify();
        }

        [Fact]
        public void FindPartialView_VirtualPathViewExists_ReturnsView()
        {
            // Arrange
            ControllerContext context = CreateContext();
            TestableVirtualPathProviderViewEngine engine = new TestableVirtualPathProviderViewEngine();
            engine.MockPathProvider
                .Setup(vpp => vpp.FileExists("~/foo/bar.partial"))
                .Returns(true)
                .Verifiable();
            engine.MockCache
                .Setup(c => c.InsertViewLocation(It.IsAny<HttpContextBase>(), It.IsAny<string>(), "~/foo/bar.partial"))
                .Verifiable();

            // Act
            ViewEngineResult result = engine.FindPartialView(context, "~/foo/bar.partial", false);

            // Assert
            Assert.Same(engine.CreatePartialViewResult, result.View);
            Assert.Null(result.SearchedLocations);
            Assert.Same(context, engine.CreatePartialViewControllerContext);
            Assert.Equal("~/foo/bar.partial", engine.CreatePartialViewPartialPath);
            engine.MockPathProvider.Verify();
            engine.MockCache.Verify();
        }

        [Fact]
        public void FindPartialView_AbsolutePathViewExists_ReturnsView()
        {
            // Arrange
            ControllerContext context = CreateContext();
            TestableVirtualPathProviderViewEngine engine = new TestableVirtualPathProviderViewEngine();
            engine.MockPathProvider
                .Setup(vpp => vpp.FileExists("/foo/bar.partial"))
                .Returns(true)
                .Verifiable();
            engine.MockCache
                .Setup(c => c.InsertViewLocation(It.IsAny<HttpContextBase>(), It.IsAny<string>(), "/foo/bar.partial"))
                .Verifiable();

            // Act
            ViewEngineResult result = engine.FindPartialView(context, "/foo/bar.partial", false);

            // Assert
            Assert.Same(engine.CreatePartialViewResult, result.View);
            Assert.Null(result.SearchedLocations);
            Assert.Same(context, engine.CreatePartialViewControllerContext);
            Assert.Equal("/foo/bar.partial", engine.CreatePartialViewPartialPath);
            engine.MockPathProvider.Verify();
            engine.MockCache.Verify();
        }

        [Fact]
        public void FileExtensions()
        {
            // Arrange + Assert
            Assert.Null(new Mock<VirtualPathProviderViewEngine>().Object.FileExtensions);
        }

        [Fact]
        public void GetExtensionThunk()
        {
            // Arrange and Assert
            Assert.Equal(VirtualPathUtility.GetExtension, new Mock<VirtualPathProviderViewEngine>().Object.GetExtensionThunk);
        }

        [Fact]
        public void DisplayModeSetOncePerRequest()
        {
            // Arrange
            RouteData routeData = new RouteData();
            routeData.Values["controller"] = "controllerName";
            routeData.Values["action"] = "actionName";

            Mock<ControllerContext> mockControllerContext = new Mock<ControllerContext>();
            mockControllerContext.Setup(c => c.HttpContext.Request.Browser.IsMobileDevice).Returns(true);
            mockControllerContext.Setup(c => c.HttpContext.Request.Cookies).Returns(new HttpCookieCollection());
            mockControllerContext.Setup(c => c.HttpContext.Response.Cookies).Returns(new HttpCookieCollection());
            mockControllerContext.Setup(c => c.HttpContext.Items).Returns(new Hashtable());
            mockControllerContext.Setup(c => c.RouteData).Returns(routeData);

            ControllerContext context = mockControllerContext.Object;
            TestableVirtualPathProviderViewEngine engine = new TestableVirtualPathProviderViewEngine();

            engine.MockPathProvider
                .Setup(vpp => vpp.FileExists("~/vpath/controllerName/viewName.view"))
                .Returns(true)
                .Verifiable();
            engine.MockCache
                .Setup(c => c.InsertViewLocation(It.IsAny<HttpContextBase>(), It.IsAny<string>(), "~/vpath/controllerName/viewName.view"))
                .Verifiable();
            engine.MockPathProvider
                .Setup(vpp => vpp.FileExists("~/vpath/controllerName/viewName.Mobile.view"))
                .Returns(false)
                .Verifiable();

            engine.MockPathProvider
                .Setup(vpp => vpp.FileExists("~/vpath/controllerName/partialName.partial"))
                .Returns(false)
                .Verifiable();
            engine.MockPathProvider
                .Setup(vpp => vpp.FileExists("~/vpath/controllerName/partialName.Mobile.partial"))
                .Returns(true)
                .Verifiable();
            engine.MockCache
                .Setup(c => c.InsertViewLocation(It.IsAny<HttpContextBase>(), It.IsAny<string>(), "~/vpath/controllerName/partialName.Mobile.partial"))
                .Callback<HttpContextBase, string, string>((httpContext, key, virtualPath) =>
                {
                    engine.MockCache
                        .Setup(c => c.GetViewLocation(It.IsAny<HttpContextBase>(), key))
                        .Returns("~/vpath/controllerName/partialName.Mobile.partial")
                        .Verifiable();
                })
                .Verifiable();

            // Act
            ViewEngineResult viewResult = engine.FindView(context, "viewName", masterName: null, useCache: false);

            // Mobile display mode will be used to locate the view with and without the cache.
            // In neither case should this set the DisplayModeId to Mobile because it has already been set.
            ViewEngineResult partialResult = engine.FindPartialView(context, "partialName", useCache: false);
            ViewEngineResult cachedPartialResult = engine.FindPartialView(context, "partialName", useCache: true);

            // Assert
            engine.MockPathProvider.Verify();
            engine.MockCache.Verify();
            Assert.Same(engine.CreateViewResult, viewResult.View);
            Assert.Same(engine.CreatePartialViewResult, partialResult.View);
            Assert.Same(engine.CreatePartialViewResult, cachedPartialResult.View);

            Assert.Equal(DisplayModeProvider.DefaultDisplayModeId, context.DisplayMode.DisplayModeId);
        }

        // The core caching scenarios are covered in the FindView/FindPartialView tests. These
        // extra tests deal with the cache itself, rather than specifics around finding views.

        private const string MASTER_VIRTUAL = "~/vpath/controllerName/name.master";
        private const string PARTIAL_VIRTUAL = "~/vpath/controllerName/name.partial";
        private const string VIEW_VIRTUAL = "~/vpath/controllerName/name.view";
        private const string MOBILE_VIEW_VIRTUAL = "~/vpath/controllerName/name.Mobile.view";

        [Fact]
        public void UsesDifferentKeysForViewMasterAndPartial()
        {
            string keyMaster = null;
            string keyPartial = null;
            string keyView = null;

            // Arrange
            ControllerContext context = CreateContext();
            TestableVirtualPathProviderViewEngine engine = new TestableVirtualPathProviderViewEngine();
            engine.MockPathProvider
                .Setup(vpp => vpp.FileExists(VIEW_VIRTUAL))
                .Returns(true)
                .Verifiable();
            engine.MockPathProvider
                .Setup(vpp => vpp.FileExists(MOBILE_VIEW_VIRTUAL))
                .Returns(false)
                .Verifiable();
            engine.MockPathProvider
                .Setup(vpp => vpp.FileExists(MASTER_VIRTUAL))
                .Returns(true)
                .Verifiable();
            engine.MockPathProvider
                .Setup(vpp => vpp.FileExists("~/vpath/controllerName/name.Mobile.master"))
                .Returns(false)
                .Verifiable();
            engine.MockPathProvider
                .Setup(vpp => vpp.FileExists(PARTIAL_VIRTUAL))
                .Returns(true)
                .Verifiable();
            engine.MockPathProvider
                .Setup(vpp => vpp.FileExists("~/vpath/controllerName/name.Mobile.partial"))
                .Returns(false)
                .Verifiable();
            engine.MockCache
                .Setup(c => c.InsertViewLocation(It.IsAny<HttpContextBase>(), It.IsAny<string>(), VIEW_VIRTUAL))
                .Callback<HttpContextBase, string, string>((httpContext, key, path) => keyView = key)
                .Verifiable();
            engine.MockCache
                .Setup(c => c.InsertViewLocation(It.IsAny<HttpContextBase>(), It.IsAny<string>(), MASTER_VIRTUAL))
                .Callback<HttpContextBase, string, string>((httpContext, key, path) => keyMaster = key)
                .Verifiable();
            engine.MockCache
                .Setup(c => c.InsertViewLocation(It.IsAny<HttpContextBase>(), It.IsAny<string>(), PARTIAL_VIRTUAL))
                .Callback<HttpContextBase, string, string>((httpContext, key, path) => keyPartial = key)
                .Verifiable();

            // Act
            engine.FindView(context, "name", "name", false);
            engine.FindPartialView(context, "name", false);

            // Assert
            Assert.NotNull(keyMaster);
            Assert.NotNull(keyPartial);
            Assert.NotNull(keyView);
            Assert.NotEqual(keyMaster, keyPartial);
            Assert.NotEqual(keyMaster, keyView);
            Assert.NotEqual(keyPartial, keyView);
            engine.MockPathProvider.Verify();
            engine.MockCache.Verify();
            engine.MockPathProvider
                .Verify(vpp => vpp.FileExists(VIEW_VIRTUAL), Times.AtMostOnce());
            engine.MockPathProvider
                .Verify(vpp => vpp.FileExists(MASTER_VIRTUAL), Times.AtMostOnce());
            engine.MockPathProvider
                .Verify(vpp => vpp.FileExists(PARTIAL_VIRTUAL), Times.AtMostOnce());
            engine.MockCache
                .Verify(c => c.InsertViewLocation(It.IsAny<HttpContextBase>(), It.IsAny<string>(), VIEW_VIRTUAL), Times.AtMostOnce());
            engine.MockCache
                .Verify(c => c.InsertViewLocation(It.IsAny<HttpContextBase>(), It.IsAny<string>(), MASTER_VIRTUAL), Times.AtMostOnce());
            engine.MockCache
                .Verify(c => c.InsertViewLocation(It.IsAny<HttpContextBase>(), It.IsAny<string>(), PARTIAL_VIRTUAL), Times.AtMostOnce());
        }

        // This tests the protocol involved with two calls to FindView for the same view name
        // where the request succeeds. The calls happen in this order:
        //
        //    FindView("view")
        //      Cache.GetViewLocation(key for "view") -> returns null (not found)
        //      VirtualPathProvider.FileExists(virtual path for "view") -> returns true
        //      Cache.InsertViewLocation(key for "view", virtual path for "view")
        //    FindView("view")
        //      Cache.GetViewLocation(key for "view") -> returns virtual path for "view"
        //
        // The mocking code is written as it is because we don't want to make any assumptions
        // about the format of the cache key. So we intercept the first call to Cache.GetViewLocation and
        // take the key they gave us to set up the rest of the mock expectations.
        // The ViewCollection class will typically place to successive calls to FindView and FindPartialView and
        // set the useCache parameter to true/false respectively. To simulate this, both calls to FindView are executed
        // with useCache set to true. This mimics the behavior of always going to the cache first and after finding a
        // view, ensuring that subsequent calls from the cache are successful.

        [Fact]
        public void ValueInCacheBypassesVirtualPathProvider()
        {
            // Arrange
            string cacheKey = null;
            ControllerContext context = CreateContext();
            ControllerContext mobileContext = CreateContext(isMobileDevice: true);

            TestableVirtualPathProviderViewEngine engine = new TestableVirtualPathProviderViewEngine();

            engine.MockPathProvider // It wasn't found, so they call vpp.FileExists
                .Setup(vpp => vpp.FileExists(VIEW_VIRTUAL))
                .Returns(true)
                .Verifiable();
            engine.MockPathProvider
                .Setup(vpp => vpp.FileExists(MOBILE_VIEW_VIRTUAL))
                .Returns(false)
                .Verifiable();
            engine.MockCache // Then they set the value into the cache
                .Setup(c => c.InsertViewLocation(It.IsAny<HttpContextBase>(), It.IsAny<string>(), VIEW_VIRTUAL))
                .Callback<HttpContextBase, string, string>((httpContext, key, virtualPath) =>
                {
                    cacheKey = key;
                    engine.MockCache // Second time through, we give them a cache hit
                        .Setup(c => c.GetViewLocation(It.IsAny<HttpContextBase>(), key))
                        .Returns(VIEW_VIRTUAL)
                        .Verifiable();
                })
                .Verifiable();

            // Act
            engine.FindView(context, "name", null, false); // Call it once with false to seed the cache
            engine.FindView(context, "name", null, true); // Call it once with true to check the cache

            // Assert
            engine.MockPathProvider.Verify();
            engine.MockCache.Verify();

            engine.MockPathProvider.Verify(vpp => vpp.FileExists(VIEW_VIRTUAL), Times.AtMostOnce());
            engine.MockCache.Verify(c => c.InsertViewLocation(It.IsAny<HttpContextBase>(), It.IsAny<string>(), VIEW_VIRTUAL), Times.AtMostOnce());
            engine.MockCache.Verify(c => c.GetViewLocation(It.IsAny<HttpContextBase>(), cacheKey), Times.AtMostOnce());

            // We seed the cache with all possible display modes but since the mobile view does not exist we don't insert it into the cache.
            engine.MockPathProvider.Verify(vpp => vpp.FileExists(MOBILE_VIEW_VIRTUAL), Times.Exactly(1));
            engine.MockCache.Verify(c => c.InsertViewLocation(It.IsAny<HttpContextBase>(), It.IsAny<string>(), MOBILE_VIEW_VIRTUAL), Times.Never());
            engine.MockCache.Verify(c => c.GetViewLocation(It.IsAny<HttpContextBase>(), VirtualPathProviderViewEngine.AppendDisplayModeToCacheKey(cacheKey, DisplayModeProvider.MobileDisplayModeId)), Times.Never());
        }

        [Fact]
        public void ValueInCacheBypassesVirtualPathProviderForAllAvailableDisplayModesForContext()
        {
            // Arrange
            string cacheKey = null;
            string mobileCacheKey = null;

            ControllerContext mobileContext = CreateContext(isMobileDevice: true);
            ControllerContext desktopContext = CreateContext(isMobileDevice: false);

            TestableVirtualPathProviderViewEngine engine = new TestableVirtualPathProviderViewEngine();

            engine.MockPathProvider
                .Setup(vpp => vpp.FileExists(VIEW_VIRTUAL))
                .Returns(true)
                .Verifiable();
            engine.MockPathProvider
                .Setup(vpp => vpp.FileExists(MOBILE_VIEW_VIRTUAL))
                .Returns(true)
                .Verifiable();

            engine.MockCache
                .Setup(c => c.InsertViewLocation(It.IsAny<HttpContextBase>(), It.IsAny<string>(), VIEW_VIRTUAL))
                .Callback<HttpContextBase, string, string>((httpContext, key, virtualPath) =>
                {
                    cacheKey = key;
                    engine.MockCache
                        .Setup(c => c.GetViewLocation(It.IsAny<HttpContextBase>(), key))
                        .Returns(MOBILE_VIEW_VIRTUAL)
                        .Verifiable();
                })
                .Verifiable();
            engine.MockCache
                .Setup(c => c.InsertViewLocation(It.IsAny<HttpContextBase>(), It.IsAny<string>(), MOBILE_VIEW_VIRTUAL))
                .Callback<HttpContextBase, string, string>((httpContext, key, virtualPath) =>
                {
                    mobileCacheKey = key;
                    engine.MockCache
                        .Setup(c => c.GetViewLocation(It.IsAny<HttpContextBase>(), key))
                        .Returns(MOBILE_VIEW_VIRTUAL)
                        .Verifiable();
                })
                .Verifiable();

            // Act
            engine.FindView(mobileContext, "name", null, false);
            engine.FindView(mobileContext, "name", null, true);

            // Assert
            engine.MockPathProvider.Verify();

            // DefaultDisplayMode with Mobile substitution is cached and hit on the second call to FindView
            engine.MockPathProvider.Verify(vpp => vpp.FileExists(MOBILE_VIEW_VIRTUAL), Times.AtMostOnce());
            engine.MockCache.Verify(c => c.InsertViewLocation(It.IsAny<HttpContextBase>(), It.IsAny<string>(), MOBILE_VIEW_VIRTUAL), Times.AtMostOnce());
            engine.MockCache.Verify(c => c.GetViewLocation(It.IsAny<HttpContextBase>(), VirtualPathProviderViewEngine.AppendDisplayModeToCacheKey(cacheKey, DisplayModeProvider.MobileDisplayModeId)), Times.AtMostOnce());

            engine.MockPathProvider.Verify(vpp => vpp.FileExists(VIEW_VIRTUAL), Times.AtMostOnce());
            engine.MockCache.Verify(c => c.InsertViewLocation(It.IsAny<HttpContextBase>(), It.IsAny<string>(), VIEW_VIRTUAL), Times.Exactly(1));

            Assert.NotEqual(cacheKey, mobileCacheKey);

            // Act
            engine.FindView(desktopContext, "name", null, true);

            // Assert
            engine.MockCache.Verify();

            // The first call to FindView without a mobile browser results in a cache hit
            engine.MockPathProvider.Verify(vpp => vpp.FileExists(VIEW_VIRTUAL), Times.AtMostOnce());
            engine.MockCache.Verify(c => c.InsertViewLocation(It.IsAny<HttpContextBase>(), It.IsAny<string>(), VIEW_VIRTUAL), Times.Exactly(1));
            engine.MockCache.Verify(c => c.GetViewLocation(It.IsAny<HttpContextBase>(), cacheKey), Times.Exactly(1));
        }

        [Fact]
        public void NoValueInCacheButFileExistsReturnsNullIfUsingCache()
        {
            // Arrange
            ControllerContext mobileContext = CreateContext(isMobileDevice: true);
            TestableVirtualPathProviderViewEngine engine = new TestableVirtualPathProviderViewEngine();

            engine.MockPathProvider
                .Setup(vpp => vpp.FileExists(MOBILE_VIEW_VIRTUAL))
                .Returns(true);
            engine.MockPathProvider
                .Setup(vpp => vpp.FileExists(VIEW_VIRTUAL))
                .Returns(true);

            engine.MockCache
                .Setup(c => c.GetViewLocation(It.IsAny<HttpContextBase>(), It.IsAny<string>()))
                .Returns((string)null)
                .Verifiable();
            engine.MockCache
                .Setup(c => c.InsertViewLocation(It.IsAny<HttpContextBase>(), It.IsAny<string>(), It.IsAny<string>()));

            // Act
            IView viewNotInCacheResult = engine.FindView(mobileContext, "name", masterName: null, useCache: true).View;

            // Assert
            Assert.Null(viewNotInCacheResult);

            // On a cache miss we should never check the file system. FindView will be called on a second pass
            // without using the cache.
            engine.MockPathProvider.Verify(vpp => vpp.FileExists(MOBILE_VIEW_VIRTUAL), Times.Never());
            engine.MockPathProvider.Verify(vpp => vpp.FileExists(VIEW_VIRTUAL), Times.Never());

            // Act & Assert

            //At this point the view on disk should be found and cached.
            Assert.NotNull(engine.FindView(mobileContext, "name", masterName: null, useCache: false).View);
        }

        [Fact]
        public void ReleaseViewCallsDispose()
        {
            // Arrange
            TestableVirtualPathProviderViewEngine engine = new TestableVirtualPathProviderViewEngine();
            ControllerContext context = CreateContext();
            IView view = engine.CreateViewResult;

            // Act
            engine.ReleaseView(context, view);

            // Assert
            Assert.True(((TestView)view).Disposed);
        }

        private static ControllerContext CreateContext(bool isMobileDevice = false)
        {
            RouteData routeData = new RouteData();
            routeData.Values["controller"] = "controllerName";
            routeData.Values["action"] = "actionName";

            Mock<ControllerContext> mockControllerContext = new Mock<ControllerContext>();
            mockControllerContext.Setup(c => c.HttpContext.Request.Browser.IsMobileDevice).Returns(isMobileDevice);
            mockControllerContext.Setup(c => c.HttpContext.Items).Returns(new Hashtable());
            mockControllerContext.Setup(c => c.RouteData).Returns(routeData);

            mockControllerContext.Setup(c => c.HttpContext.Response.Cookies).Returns(new HttpCookieCollection());
            mockControllerContext.Setup(c => c.HttpContext.Request.Cookies).Returns(new HttpCookieCollection());

            return mockControllerContext.Object;
        }

        private class TestView : IView, IDisposable
        {
            public bool Disposed { get; set; }

            void IDisposable.Dispose()
            {
                Disposed = true;
            }

            void IView.Render(ViewContext viewContext, TextWriter writer)
            {
            }
        }

        private class TestableVirtualPathProviderViewEngine : VirtualPathProviderViewEngine
        {
            public IView CreatePartialViewResult = new Mock<IView>().Object;
            public string CreatePartialViewPartialPath;
            public ControllerContext CreatePartialViewControllerContext;

            //public IView CreateViewResult = new Mock<IView>().Object;
            public IView CreateViewResult = new TestView();
            public string CreateViewMasterPath;
            public ControllerContext CreateViewControllerContext;
            public string CreateViewViewPath;

            public Mock<IViewLocationCache> MockCache = new Mock<IViewLocationCache>(MockBehavior.Strict);
            public Mock<VirtualPathProvider> MockPathProvider = new Mock<VirtualPathProvider>(MockBehavior.Strict);

            public TestableVirtualPathProviderViewEngine()
            {
                MasterLocationFormats = new[] { "~/vpath/{1}/{0}.master" };
                ViewLocationFormats = new[] { "~/vpath/{1}/{0}.view" };
                PartialViewLocationFormats = new[] { "~/vpath/{1}/{0}.partial" };
                AreaMasterLocationFormats = new[] { "~/vpath/{2}/{1}/{0}.master" };
                AreaViewLocationFormats = new[] { "~/vpath/{2}/{1}/{0}.view" };
                AreaPartialViewLocationFormats = new[] { "~/vpath/{2}/{1}/{0}.partial" };
                FileExtensions = new[] { "view", "partial", "master" };

                ViewLocationCache = MockCache.Object;
                VirtualPathProvider = MockPathProvider.Object;

                MockCache
                    .Setup(c => c.GetViewLocation(It.IsAny<HttpContextBase>(), It.IsAny<string>()))
                    .Returns((string)null);

                GetExtensionThunk = GetExtension;
            }

            public void ClearViewLocations()
            {
                ViewLocationFormats = new string[0];
            }

            public void ClearMasterLocations()
            {
                MasterLocationFormats = new string[0];
            }

            public void ClearPartialViewLocations()
            {
                PartialViewLocationFormats = new string[0];
            }

            protected override IView CreatePartialView(ControllerContext controllerContext, string partialPath)
            {
                CreatePartialViewControllerContext = controllerContext;
                CreatePartialViewPartialPath = partialPath;

                return CreatePartialViewResult;
            }

            protected override IView CreateView(ControllerContext controllerContext, string viewPath, string masterPath)
            {
                CreateViewControllerContext = controllerContext;
                CreateViewViewPath = viewPath;
                CreateViewMasterPath = masterPath;

                return CreateViewResult;
            }

            private static string GetExtension(string virtualPath)
            {
                var extension = virtualPath.Substring(virtualPath.LastIndexOf('.'));
                return extension;
            }
        }
    }
}
