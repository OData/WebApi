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
using Microsoft.Data.OData;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.OData
{
    public class ODataHttpConfigurationExtensionTest
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
        public void GetODataPathHandlerReturnsDefaultPathHandlerByDefault()
        {
            HttpConfiguration config = new HttpConfiguration();
            ODataModelBuilder modelBuilder = new ODataConventionModelBuilder();
            modelBuilder.EntitySet<Customer>(typeof(Customer).Name);
            IEdmModel model = modelBuilder.GetEdmModel();
            config.SetEdmModel(model);

            var pathHandler = config.GetODataPathHandler();

            Assert.NotNull(pathHandler);
            Assert.IsType<DefaultODataPathHandler>(pathHandler);
        }

        [Fact]
        public void SetODataPathHandlerThenGetReturnsWhatYouSet()
        {
            HttpConfiguration config = new HttpConfiguration();
            IODataPathHandler parser = new Mock<IODataPathHandler>().Object;

            // Act
            config.SetODataPathHandler(parser);

            // Assert
            Assert.Same(parser, config.GetODataPathHandler());
        }

        [Fact]
        public void SetODataFormatter_AddsFormatterToTheFormatterCollection()
        {
            // Arrange
            HttpConfiguration configuration = new HttpConfiguration();
            ODataMediaTypeFormatter formatter = CreateODataFormatter();

            // Act
            configuration.Formatters.Insert(0, formatter);

            // Assert
            Assert.Contains(formatter, configuration.Formatters);
        }

        [Fact]
        public void GetODataFormatters_ReturnsFormatter_IfSet()
        {
            // Arrange
            HttpConfiguration configuration = new HttpConfiguration();
            ODataMediaTypeFormatter formatter1 = CreateODataFormatter();
            ODataMediaTypeFormatter formatter2 = CreateODataFormatter();
            configuration.Formatters.Add(formatter1);
            configuration.Formatters.Add(formatter2);

            // Act
            IEnumerable<MediaTypeFormatter> result = configuration.GetODataFormatters();

            // Assert
            IEnumerable<MediaTypeFormatter> expectedFormatters = new MediaTypeFormatter[]
            {
                formatter1, formatter2
            };

            Assert.True(expectedFormatters.SequenceEqual(result));
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
            Assert.Equal(100, ((QueryableAttribute)filters[0].Instance).PageSize);
        }

        private static ODataMediaTypeFormatter CreateODataFormatter()
        {
            return new ODataMediaTypeFormatter(EdmCoreModel.Instance, new ODataPayloadKind[0]);
        }
    }
}
