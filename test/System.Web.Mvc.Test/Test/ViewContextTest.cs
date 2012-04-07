// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.IO;
using Moq;
using Xunit;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Web.Mvc.Test
{
    public class ViewContextTest
    {
        [Fact]
        public void GuardClauses()
        {
            // Arrange
            var controllerContext = new Mock<ControllerContext>().Object;
            var view = new Mock<IView>().Object;
            var viewData = new ViewDataDictionary();
            var tempData = new TempDataDictionary();
            var writer = new StringWriter();

            // Act & Assert
            Assert.ThrowsArgumentNull(
                () => new ViewContext(null, view, viewData, tempData, writer),
                "controllerContext"
                );
            Assert.ThrowsArgumentNull(
                () => new ViewContext(controllerContext, null, viewData, tempData, writer),
                "view"
                );
            Assert.ThrowsArgumentNull(
                () => new ViewContext(controllerContext, view, null, tempData, writer),
                "viewData"
                );
            Assert.ThrowsArgumentNull(
                () => new ViewContext(controllerContext, view, viewData, null, writer),
                "tempData"
                );
            Assert.ThrowsArgumentNull(
                () => new ViewContext(controllerContext, view, viewData, tempData, null),
                "writer"
                );
        }

        [Fact]
        public void FormIdGeneratorProperty()
        {
            // Arrange
            var mockHttpContext = new Mock<HttpContextBase>();
            mockHttpContext.Setup(o => o.Items).Returns(new Hashtable());
            var viewContext = new ViewContext
            {
                HttpContext = mockHttpContext.Object
            };

            // Act
            string form0Name = viewContext.FormIdGenerator();
            string form1Name = viewContext.FormIdGenerator();
            string form2Name = viewContext.FormIdGenerator();

            // Assert
            Assert.Equal("form0", form0Name);
            Assert.Equal("form1", form1Name);
            Assert.Equal("form2", form2Name);
        }

        [Fact]
        public void PropertiesAreSet()
        {
            // Arrange
            var mockControllerContext = new Mock<ControllerContext>();
            mockControllerContext.Setup(o => o.HttpContext.Items).Returns(new Hashtable());
            var view = new Mock<IView>().Object;
            var viewData = new ViewDataDictionary();
            var tempData = new TempDataDictionary();
            var writer = new StringWriter();

            // Act
            ViewContext viewContext = new ViewContext(mockControllerContext.Object, view, viewData, tempData, writer);

            // Setting FormContext to null will return the default one later
            viewContext.FormContext = null;

            // Assert
            Assert.Equal(view, viewContext.View);
            Assert.Equal(viewData, viewContext.ViewData);
            Assert.Equal(tempData, viewContext.TempData);
            Assert.Equal(writer, viewContext.Writer);
            Assert.False(viewContext.UnobtrusiveJavaScriptEnabled); // Unobtrusive JavaScript should be off by default
            Assert.NotNull(viewContext.FormContext); // We get the default FormContext
        }

        [Fact]
        public void ViewContextUsesScopeThunkForInstanceClientValidationFlag()
        {
            // Arrange
            var scope = new Dictionary<object, object>();
            var httpContext = new Mock<HttpContextBase>();
            var viewContext = new ViewContext { ScopeThunk = () => scope, HttpContext = httpContext.Object };
            httpContext.Setup(c => c.Items).Returns(new Hashtable());

            // Act & Assert
            Assert.False(viewContext.ClientValidationEnabled);
            viewContext.ClientValidationEnabled = true;
            Assert.True(viewContext.ClientValidationEnabled);
            Assert.Equal(true, scope[ViewContext.ClientValidationKeyName]);
            viewContext.ClientValidationEnabled = false;
            Assert.False(viewContext.ClientValidationEnabled);
            Assert.Equal(false, scope[ViewContext.ClientValidationKeyName]);
        }

        [Fact]
        public void ViewContextUsesScopeThunkForInstanceUnobstrusiveJavaScriptFlag()
        {
            // Arrange
            var scope = new Dictionary<object, object>();
            var httpContext = new Mock<HttpContextBase>();
            var viewContext = new ViewContext { ScopeThunk = () => scope, HttpContext = httpContext.Object };
            httpContext.Setup(c => c.Items).Returns(new Hashtable());

            // Act & Assert
            Assert.False(viewContext.UnobtrusiveJavaScriptEnabled);
            viewContext.UnobtrusiveJavaScriptEnabled = true;
            Assert.True(viewContext.UnobtrusiveJavaScriptEnabled);
            Assert.Equal(true, scope[ViewContext.UnobtrusiveJavaScriptKeyName]);
            viewContext.UnobtrusiveJavaScriptEnabled = false;
            Assert.False(viewContext.UnobtrusiveJavaScriptEnabled);
            Assert.Equal(false, scope[ViewContext.UnobtrusiveJavaScriptKeyName]);
        }

        [Fact]
        public void ViewBagProperty_ReflectsViewData()
        {
            // Arrange
            var mockControllerContext = new Mock<ControllerContext>();
            var view = new Mock<IView>().Object;
            var viewData = new ViewDataDictionary() { { "A", 1 } };

            // Act
            ViewContext viewContext = new ViewContext(mockControllerContext.Object, view, viewData, new TempDataDictionary(), new StringWriter());

            // Assert
            Assert.Equal(1, viewContext.ViewBag.A);
        }

        [Fact]
        public void ViewBagProperty_ReflectsNewViewDataInstance()
        {
            // Arrange
            var mockControllerContext = new Mock<ControllerContext>();
            var view = new Mock<IView>().Object;
            var viewData = new ViewDataDictionary() { { "A", 1 } };

            ViewContext viewContext = new ViewContext(mockControllerContext.Object, view, viewData, new TempDataDictionary(), new StringWriter());

            // Act
            viewContext.ViewData = new ViewDataDictionary() { { "A", "bar" } };

            // Assert
            Assert.Equal("bar", viewContext.ViewBag.A);
        }

        [Fact]
        public void ViewBag_PropagatesChangesToViewData()
        {
            // Arrange
            var mockControllerContext = new Mock<ControllerContext>();
            var view = new Mock<IView>().Object;
            var viewData = new ViewDataDictionary() { { "A", 1 } };

            ViewContext viewContext = new ViewContext(mockControllerContext.Object, view, viewData, new TempDataDictionary(), new StringWriter());

            // Act
            viewContext.ViewBag.A = "foo";
            viewContext.ViewBag.B = 2;

            // Assert
            Assert.Equal("foo", viewContext.ViewData["A"]);
            Assert.Equal(2, viewContext.ViewData["B"]);
        }
    }
}
