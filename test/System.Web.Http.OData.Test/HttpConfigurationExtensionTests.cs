// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Web.Http.OData.Builder;
using System.Web.Http.OData.TestCommon.Models;
using Microsoft.Data.Edm;
using Microsoft.TestCommon;

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
    }
}
