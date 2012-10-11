// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Web.Http.OData.Builder;
using System.Web.Http.OData.Formatter;
using System.Web.Http.OData.Query;
using System.Web.Http.OData.TestCommon.Models;
using Microsoft.Data.Edm;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.OData
{
    public class HttpConfigurationExtensionTests
    {
        [Fact]
        public void GetEdmModelReturnsNullByDefault()
        {
            HttpConfiguration config = new HttpConfiguration();
            IEdmModel model = config.GetEdmModel();

            Assert.Null(model);
        }

        [Fact]
        public void SetEdmModelThenGetReturnsWhatYouSet()
        {
            // Arrange
            HttpConfiguration config = new HttpConfiguration();
            ODataModelBuilder modelBuilder = new ODataConventionModelBuilder();
            modelBuilder.EntitySet<Customer>(typeof(Customer).Name);
            IEdmModel model = modelBuilder.GetEdmModel();

            // Act
            config.SetEdmModel(model);
            IEdmModel newModel = config.GetEdmModel();

            // Assert
            Assert.Same(model, newModel);
        }

        [Fact]
        public void SetEdmModel_WithADifferentModel_AfterSettingFormatter_Throws()
        {
            // Arrange
            HttpConfiguration config = new HttpConfiguration();
            IEdmModel model = new Mock<IEdmModel>().Object;
            config.SetODataFormatter(new ODataMediaTypeFormatter());

            // Act
            Assert.Throws<NotSupportedException>(
                () => config.SetEdmModel(model),
                "The given 'IEdmModel' does not match the 'IEdmModel' in the formatter on the configuration. Setting 'ODataMediaTypeFormatter' using the method 'SetODataFormatter' also sets the corresponding 'IEdmModel'.");
        }

        [Fact]
        public void SetODataFormatter_AddsFormatterToTheFormatterCollection()
        {
            // Arrange
            HttpConfiguration configuration = new HttpConfiguration();
            ODataMediaTypeFormatter formatter = new Mock<ODataMediaTypeFormatter>().Object;

            // Act
            configuration.SetODataFormatter(formatter);

            // Assert
            Assert.Contains(formatter, configuration.Formatters);
        }

        [Fact]
        public void SetODataFormatter_Throws_FormatterAlreadyInTheCollection()
        {
            // Arrange
            HttpConfiguration configuration = new HttpConfiguration();
            ODataMediaTypeFormatter formatter = new Mock<ODataMediaTypeFormatter>().Object;
            configuration.Formatters.Add(formatter);

            // Act & Assert
            Assert.Throws<NotSupportedException>(
                () => configuration.SetODataFormatter(formatter),
                "You already have a ODataMediaTypeFormatter on your HttpConfiguration. Set your ODataMediaTypeFormatter once using the 'SetODataFormatter' method on 'HttpConfiguration' only.");

        }

        [Fact]
        public void SetODataFormatter_AfterSettingEdmModel_Throws()
        {
            // Arrange
            HttpConfiguration config = new HttpConfiguration();
            IEdmModel model = new Mock<IEdmModel>().Object;
            config.SetEdmModel(model);

            // Act
            Assert.Throws<NotSupportedException>(
                () => config.SetODataFormatter(new ODataMediaTypeFormatter()),
                "The 'IEdmModel' on the configuration does not match the 'IEdmModel' in the given formatter. Setting 'ODataMediaTypeFormatter' using the method 'SetODataFormatter' also sets the corresponding 'IEdmModel'.");
        }

        [Fact]
        public void GetODataFormatter_Returns_SetODataFormatter()
        {
            // Arrange
            HttpConfiguration configuration = new HttpConfiguration();
            ODataMediaTypeFormatter formatter = new Mock<ODataMediaTypeFormatter>().Object;
            configuration.SetODataFormatter(formatter);

            // Act
            ODataMediaTypeFormatter result = configuration.GetODataFormatter();

            // Assert
            Assert.Equal(formatter, result);
        }

        [Fact]
        public void GetODataFormatter_ReturnsNull_IfNotSet()
        {
            // Arrange
            HttpConfiguration configuration = new HttpConfiguration();
            ODataMediaTypeFormatter formatter = new Mock<ODataMediaTypeFormatter>().Object;
            configuration.Formatters.Add(formatter);

            // Act
            ODataMediaTypeFormatter result = configuration.GetODataFormatter();

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void AddQuerySupport_AddsQueryableFilterProvider()
        {
            HttpConfiguration configuration = new HttpConfiguration();

            configuration.EnableQuerySupport();

            Assert.Equal(1, configuration.Services.GetFilterProviders().OfType<QueryableFilterProvider>().Count());
        }

        [Fact]
        public void AddQuerySupport_AddsQueryableFilterProviderWithResultLimit()
        {
            HttpConfiguration configuration = new HttpConfiguration();

            configuration.EnableQuerySupport(100);

            IEnumerable<QueryableFilterProvider> filterProviders = configuration.Services.GetFilterProviders().OfType<QueryableFilterProvider>();
            Assert.Equal(1, filterProviders.Count());
            Assert.Equal(100, filterProviders.First().ResultLimit);
        }

        [Fact]
        public void AddQuerySupport_ActionFilters_TakePrecedence()
        {
            HttpConfiguration config = new HttpConfiguration();
            config.EnableQuerySupport();
            HttpControllerDescriptor controllerDescriptor = new HttpControllerDescriptor(config, "FilterProviderTest", typeof(FilterProviderTestController));
            HttpActionDescriptor actionDescriptor = new ReflectedHttpActionDescriptor(controllerDescriptor, typeof(FilterProviderTestController).GetMethod("GetQueryableWithFilterAttribute"));

            Collection<FilterInfo> filters = actionDescriptor.GetFilterPipeline();

            Assert.Equal(1, filters.Count);
            Assert.Equal(100, ((QueryableAttribute)filters[0].Instance).ResultLimit);
        }
    }
}
