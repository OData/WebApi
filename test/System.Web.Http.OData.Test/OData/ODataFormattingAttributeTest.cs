// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using System.Web.Http.Controllers;
using System.Web.Http.OData.Formatter;
using Microsoft.Data.Edm.Library;
using Microsoft.Data.OData;
using Microsoft.TestCommon;

namespace System.Web.Http.OData
{
    public class ODataFormattingAttributeTest
    {
        [Fact]
        public void Initialize_RegistersODataFormatters()
        {
            var config = new HttpConfiguration();
            config.SetEdmModel(EdmCoreModel.Instance);
            var controllerSettings = new HttpControllerSettings(config);
            var controllerDescriptor = new HttpControllerDescriptor();
            controllerDescriptor.Configuration = config;

            new ODataFormattingAttribute().Initialize(controllerSettings, controllerDescriptor);

            Assert.NotEmpty(controllerSettings.Formatters.OfType<ODataMediaTypeFormatter>());
        }

        [Fact]
        public void Initialize_DoesNotChangeFormatters_IfNoModelRegistered()
        {
            var config = new HttpConfiguration();
            var controllerSettings = new HttpControllerSettings(config);
            var controllerDescriptor = new HttpControllerDescriptor();
            controllerDescriptor.Configuration = config;

            new ODataFormattingAttribute().Initialize(controllerSettings, controllerDescriptor);

            Assert.Empty(controllerSettings.Formatters.OfType<ODataMediaTypeFormatter>());
        }

        [Fact]
        public void Initialize_DoesNotChangeFormatters_IfODataFormatterAlreadyRegistered()
        {
            var config = new HttpConfiguration();
            var odataFormatter = new ODataMediaTypeFormatter(EdmCoreModel.Instance, Enumerable.Empty<ODataPayloadKind>());
            config.Formatters.Add(odataFormatter);
            int formatterCount = config.Formatters.Count;
            var controllerSettings = new HttpControllerSettings(config);
            var controllerDescriptor = new HttpControllerDescriptor();
            controllerDescriptor.Configuration = config;

            new ODataFormattingAttribute().Initialize(controllerSettings, controllerDescriptor);

            Assert.Same(odataFormatter, controllerSettings.Formatters.OfType<ODataMediaTypeFormatter>().First());
            Assert.Equal(formatterCount, controllerSettings.Formatters.Count);
        }
    }
}
