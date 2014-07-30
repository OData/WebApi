// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using System.Web.Http.Controllers;
using System.Web.Http.OData.Formatter;
using Microsoft.Data.OData;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.OData
{
    public class ODataFormattingAttributeTest
    {
        [Fact]
        public void Initialize_RegistersODataFormatters()
        {
            var config = new HttpConfiguration();
            var controllerSettings = new HttpControllerSettings(config);
            var controllerDescriptor = new HttpControllerDescriptor();
            controllerDescriptor.Configuration = config;

            new ODataFormattingAttribute().Initialize(controllerSettings, controllerDescriptor);

            Assert.NotEmpty(controllerSettings.Formatters.OfType<ODataMediaTypeFormatter>());
            Assert.Empty(controllerSettings.Formatters.Where(f => f is XmlMediaTypeFormatter));
            Assert.Empty(controllerSettings.Formatters.Where(f => f is JsonMediaTypeFormatter));
            // Formatters that aren't XmlMTF or JsonMTF are left in the formatter collection
            Assert.NotEmpty(controllerSettings.Formatters.Where(f => !(f is ODataMediaTypeFormatter)));
        }

        [Fact]
        public void Initialize_DoesNotChangeFormatters_IfODataFormatterAlreadyRegistered()
        {
            var config = new HttpConfiguration();
            var odataFormatter = new ODataMediaTypeFormatter(Enumerable.Empty<ODataPayloadKind>());
            config.Formatters.Add(odataFormatter);
            int formatterCount = config.Formatters.Count;
            var controllerSettings = new HttpControllerSettings(config);
            var controllerDescriptor = new HttpControllerDescriptor();
            controllerDescriptor.Configuration = config;

            new ODataFormattingAttribute().Initialize(controllerSettings, controllerDescriptor);

            Assert.Same(odataFormatter, controllerSettings.Formatters.OfType<ODataMediaTypeFormatter>().First());
            Assert.Equal(formatterCount, controllerSettings.Formatters.Count);
        }

        [Fact]
        public void Initialize_RegistersContentNegotiator()
        {
            var config = new HttpConfiguration();
            var controllerSettings = new HttpControllerSettings(config);
            var controllerDescriptor = new HttpControllerDescriptor();
            controllerDescriptor.Configuration = config;

            new ODataFormattingAttribute().Initialize(controllerSettings, controllerDescriptor);

            Assert.IsType<PerRequestContentNegotiator>(controllerSettings.Services.GetContentNegotiator());
        }

        [Fact]
        public void Initialize_Calls_CreateODataFormatters()
        {
            var config = new HttpConfiguration();
            var controllerSettings = new HttpControllerSettings(config);
            var controllerDescriptor = new HttpControllerDescriptor();
            controllerDescriptor.Configuration = config;

            Mock<ODataFormattingAttribute> formattingAttribute = new Mock<ODataFormattingAttribute>();
            formattingAttribute
                .Setup(f => f.CreateODataFormatters())
                .Returns(new List<ODataMediaTypeFormatter> { new TestODataMediaTypeFormatter() })
                .Verifiable();

            formattingAttribute.Object.Initialize(controllerSettings, controllerDescriptor);
            Assert.NotEmpty(controllerSettings.Formatters.OfType<TestODataMediaTypeFormatter>());
        }

        private class TestODataMediaTypeFormatter : ODataMediaTypeFormatter
        {
            public TestODataMediaTypeFormatter()
                : base(Enumerable.Empty<ODataPayloadKind>())
            {
            }
        }
    }
}
