// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http.Formatting;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Web.Http.OData.Builder;
using System.Web.Http.OData.Formatter;
using System.Web.Http.OData.Query;
using System.Web.Http.OData.Routing;
using System.Web.Http.OData.TestCommon.Models;
using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Library;
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
        public void GetODataPathParserReturnsDefaultPathParserByDefault()
        {
            HttpConfiguration config = new HttpConfiguration();
            ODataModelBuilder modelBuilder = new ODataConventionModelBuilder();
            modelBuilder.EntitySet<Customer>(typeof(Customer).Name);
            IEdmModel model = modelBuilder.GetEdmModel();
            config.SetEdmModel(model);

            IODataPathParser parser = config.GetODataPathParser();

            DefaultODataPathParser defaultParser = Assert.IsType<DefaultODataPathParser>(parser);
            Assert.Same(model, defaultParser.Model);
        }

        [Fact]
        public void SetODataPathParserThenGetReturnsWhatYouSet()
        {
            HttpConfiguration config = new HttpConfiguration();
            IODataPathParser parser = new Mock<IODataPathParser>().Object;

            // Act
            config.SetODataPathParser(parser);

            // Assert
            Assert.Same(parser, config.GetODataPathParser());
        }

        [Fact]
        public void SetODataFormatter_AddsFormatterToTheFormatterCollection()
        {
            // Arrange
            HttpConfiguration configuration = new HttpConfiguration();
            ODataMediaTypeFormatter formatter = new ODataMediaTypeFormatter(EdmCoreModel.Instance);

            // Act
            configuration.Formatters.Insert(0, formatter);

            // Assert
            Assert.Contains(formatter, configuration.Formatters);
        }

        [Fact]
        public void GetODataFormatter_ReturnsFormatter_IfSet()
        {
            // Arrange
            HttpConfiguration configuration = new HttpConfiguration();
            ODataMediaTypeFormatter formatter = new ODataMediaTypeFormatter(EdmCoreModel.Instance);
            configuration.Formatters.Add(formatter);

            // Act
            IEdmModel model;
            MediaTypeFormatter result = configuration.GetODataFormatter(out model);

            // Assert
            Assert.Same(formatter, result);
        }

        [Fact]
        public void AddQuerySupport_AddsQueryableFilterProvider()
        {
            HttpConfiguration configuration = new HttpConfiguration();

            configuration.EnableQuerySupport();

            var queryFilterProviders = configuration.Services.GetFilterProviders().OfType<QueryFilterProvider>();
            Assert.Equal(1, queryFilterProviders.Count());
            var queryAttribute = Assert.IsType<QueryableAttribute>(queryFilterProviders.First().QueryFilter);
        }

        [Fact]
        public void AddQuerySupport_AddsFilterProviderForQueryFilter()
        {
            HttpConfiguration configuration = new HttpConfiguration();
            Mock<IActionFilter> myQueryFilter = new Mock<IActionFilter>();

            configuration.EnableQuerySupport(myQueryFilter.Object);

            var queryFilterProviders = configuration.Services.GetFilterProviders().OfType<QueryFilterProvider>();
            Assert.Equal(1, queryFilterProviders.Count());
            Assert.Same(myQueryFilter.Object, queryFilterProviders.First().QueryFilter);
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
