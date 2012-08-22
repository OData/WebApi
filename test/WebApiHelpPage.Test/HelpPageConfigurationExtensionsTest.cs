// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Http.ModelBinding;
using Microsoft.TestCommon;
using Moq;
using ROOT_PROJECT_NAMESPACE.Areas.HelpPage;
using ROOT_PROJECT_NAMESPACE.Areas.HelpPage.Models;

namespace WebApiHelpPageWebHost.UnitTest
{
    public class HelpPageConfigurationExtensionsTest
    {
        [Theory]
        [InlineData("Get-Values")]
        [InlineData("get-values")]
        [InlineData("Get-Values_Name")]
        [InlineData("get-values_NAME")]
        [InlineData("Get-Values-id")]
        [InlineData("Get-Values-ID")]
        [InlineData("Post-Values")]
        [InlineData("POST-VALUES")]
        [InlineData("Put-Values-id")]
        [InlineData("Put-VALUES-ID")]
        [InlineData("Put-Values")]
        [InlineData("Put-VALUES")]
        [InlineData("Delete-Values-id")]
        [InlineData("Delete-VALUES-id")]
        [InlineData("Patch-Values")]
        [InlineData("Patch-VALUES")]
        [InlineData("Options-Values")]
        [InlineData("OpTions-VALUES")]
        [InlineData("Head-Values-id")]
        [InlineData("HEAD-VALUES-id")]
        public void GetHelpPageApiModel_ReturnsTheModel_WhenIdIsValid(string apiId)
        {
            HttpConfiguration config = new HttpConfiguration();
            config.Routes.MapHttpRoute("Default", "{controller}/{id}", new { id = RouteParameter.Optional });
            HelpPageApiModel model = config.GetHelpPageApiModel(apiId);
            Assert.NotNull(model);
            Assert.Same(model, config.GetHelpPageApiModel(apiId));
            Assert.Equal(apiId, model.ApiDescription.GetFriendlyId(), StringComparer.OrdinalIgnoreCase);
        }

        [Theory]
        [InlineData("foo")]
        [InlineData("bar")]
        [InlineData("@alpha")]
        [InlineData("GetValues/{id}/{name}")]
        public void GetHelpPageApiModel_ReturnsNull_WhenIdIsInvalid(string apiId)
        {
            HttpConfiguration config = new HttpConfiguration();
            config.Routes.MapHttpRoute("Default", "{controller}/{id}", new { id = RouteParameter.Optional });
            HelpPageApiModel model = config.GetHelpPageApiModel(apiId);
            Assert.Null(model);
        }

        [Theory]
        [InlineData("Get-Values")]
        [InlineData("get-values")]
        [InlineData("Get-Values-id")]
        [InlineData("Get-Values-ID")]
        public void GetHelpPageApiModel_ReturnsNull_IfNoRoute(string apiId)
        {
            HttpConfiguration config = new HttpConfiguration();
            HelpPageApiModel model = config.GetHelpPageApiModel(apiId);
            Assert.Null(model);
        }

        [Fact]
        public void GetHelpPageApiModel_HandlesException_ThrownDuringSampleGeneration()
        {
            HttpConfiguration config = new HttpConfiguration();
            config.Routes.MapHttpRoute("Default", "{controller}/{id}", new { id = RouteParameter.Optional });
            Mock<HelpPageSampleGenerator> faultyGenerator = new Mock<HelpPageSampleGenerator>();
            faultyGenerator.Setup(g => g.GetSample(It.IsAny<ApiDescription>(), It.IsAny<SampleDirection>())).Returns(() => { throw new InvalidOperationException("This is a faulty sample generator."); });
            config.SetHelpPageSampleGenerator(faultyGenerator.Object);
            HelpPageApiModel model = config.GetHelpPageApiModel("Get-Values");
            Assert.NotNull(model);
            Assert.NotEmpty(model.ErrorMessages);
            Assert.Equal("An exception has occurred while generating the sample. Exception Message: This is a faulty sample generator.", model.ErrorMessages[0]);
        }

        [Fact]
        public void GetHelpPageApiModel_HandlesException_ThrownDuringSampleObjectSerialization()
        {
            HttpConfiguration config = new HttpConfiguration();
            config.Formatters.Clear();
            config.Formatters.Add(new JQueryMvcFormUrlEncodedFormatter());
            config.SetSampleObjects(new Dictionary<Type, object> { { typeof(string), "sample" } });
            config.Routes.MapHttpRoute("Default", "{controller}/{id}", new { id = RouteParameter.Optional });
            HelpPageApiModel model = config.GetHelpPageApiModel("Post-Values");
            Assert.NotNull(model);
            Assert.NotEmpty(model.ErrorMessages);
            Assert.Equal("Failed to generate the sample for media type 'application/x-www-form-urlencoded'. Cannot use formatter 'JQueryMvcFormUrlEncodedFormatter' to write type 'String'.", model.ErrorMessages[0]);
        }

        [Fact]
        public void SetDocumentationProvider()
        {
            HttpConfiguration config = new HttpConfiguration();
            Mock<IDocumentationProvider> docProviderMock = new Mock<IDocumentationProvider>();
            IDocumentationProvider docProvider = docProviderMock.Object;
            config.SetDocumentationProvider(docProvider);

            Assert.Same(docProvider, config.Services.GetDocumentationProvider());
        }

        [Fact]
        public void SetSampleObjects()
        {
            HttpConfiguration config = new HttpConfiguration();
            Dictionary<Type, object> sampleObjects = new Dictionary<Type, object>
            {
                {typeof(int), 21},
                {typeof(string), "sample"}
            };
            config.SetSampleObjects(sampleObjects);

            object sampleGeneratorObj;
            config.Properties.TryGetValue(typeof(HelpPageSampleGenerator), out sampleGeneratorObj);
            HelpPageSampleGenerator sampleGenerator = Assert.IsType<HelpPageSampleGenerator>(sampleGeneratorObj);
            Assert.Same(sampleObjects, sampleGenerator.SampleObjects);
        }

        [Fact]
        public void SetSampleRequest()
        {
            HttpConfiguration config = new HttpConfiguration();
            TextSample sample = new TextSample("test");
            config.SetSampleRequest(sample, new MediaTypeHeaderValue("application/xml"), "values", "get");
            object sampleGeneratorObj;
            config.Properties.TryGetValue(typeof(HelpPageSampleGenerator), out sampleGeneratorObj);
            HelpPageSampleGenerator sampleGenerator = Assert.IsType<HelpPageSampleGenerator>(sampleGeneratorObj);
            Assert.NotEmpty(sampleGenerator.ActionSamples);
            var actionSample = sampleGenerator.ActionSamples.First();
            Assert.Equal("values", actionSample.Key.ControllerName);
            Assert.Equal("get", actionSample.Key.ActionName);
            Assert.Equal(new MediaTypeHeaderValue("application/xml"), actionSample.Key.MediaType);
            Assert.Equal(SampleDirection.Request, actionSample.Key.SampleDirection);
            Assert.NotEmpty(actionSample.Key.ParameterNames);
            Assert.Equal("*", actionSample.Key.ParameterNames.First());
            Assert.Same(sample, actionSample.Value);
        }

        [Fact]
        public void SetSampleRequest_WithParameters()
        {
            HttpConfiguration config = new HttpConfiguration();
            TextSample sample = new TextSample("test");
            config.SetSampleRequest(sample, new MediaTypeHeaderValue("application/json"), "values", "post", "id", "name");
            object sampleGeneratorObj;
            config.Properties.TryGetValue(typeof(HelpPageSampleGenerator), out sampleGeneratorObj);
            HelpPageSampleGenerator sampleGenerator = Assert.IsType<HelpPageSampleGenerator>(sampleGeneratorObj);
            Assert.NotEmpty(sampleGenerator.ActionSamples);
            var actionSample = sampleGenerator.ActionSamples.First();
            Assert.Equal("values", actionSample.Key.ControllerName);
            Assert.Equal("post", actionSample.Key.ActionName);
            Assert.Equal(new MediaTypeHeaderValue("application/json"), actionSample.Key.MediaType);
            Assert.Equal(SampleDirection.Request, actionSample.Key.SampleDirection);
            Assert.Equal(2, actionSample.Key.ParameterNames.Count);
            Assert.True(actionSample.Key.ParameterNames.SetEquals(new[] { "id", "name" }));
            Assert.Same(sample, actionSample.Value);
        }

        [Fact]
        public void SetSampleResponse()
        {
            HttpConfiguration config = new HttpConfiguration();
            TextSample sample = new TextSample("test");
            config.SetSampleResponse(sample, new MediaTypeHeaderValue("application/xml"), "values", "get");
            object sampleGeneratorObj;
            config.Properties.TryGetValue(typeof(HelpPageSampleGenerator), out sampleGeneratorObj);
            HelpPageSampleGenerator sampleGenerator = Assert.IsType<HelpPageSampleGenerator>(sampleGeneratorObj);
            Assert.NotEmpty(sampleGenerator.ActionSamples);
            var actionSample = sampleGenerator.ActionSamples.First();
            Assert.Equal("values", actionSample.Key.ControllerName);
            Assert.Equal("get", actionSample.Key.ActionName);
            Assert.Equal(new MediaTypeHeaderValue("application/xml"), actionSample.Key.MediaType);
            Assert.Equal(SampleDirection.Response, actionSample.Key.SampleDirection);
            Assert.NotEmpty(actionSample.Key.ParameterNames);
            Assert.Equal("*", actionSample.Key.ParameterNames.First());
            Assert.Same(sample, actionSample.Value);
        }

        [Fact]
        public void SetSampleResponse_WithParameters()
        {
            HttpConfiguration config = new HttpConfiguration();
            TextSample sample = new TextSample("test");
            config.SetSampleResponse(sample, new MediaTypeHeaderValue("application/json"), "values", "post", "id", "name");
            object sampleGeneratorObj;
            config.Properties.TryGetValue(typeof(HelpPageSampleGenerator), out sampleGeneratorObj);
            HelpPageSampleGenerator sampleGenerator = Assert.IsType<HelpPageSampleGenerator>(sampleGeneratorObj);
            Assert.NotEmpty(sampleGenerator.ActionSamples);
            var actionSample = sampleGenerator.ActionSamples.First();
            Assert.Equal("values", actionSample.Key.ControllerName);
            Assert.Equal("post", actionSample.Key.ActionName);
            Assert.Equal(new MediaTypeHeaderValue("application/json"), actionSample.Key.MediaType);
            Assert.Equal(SampleDirection.Response, actionSample.Key.SampleDirection);
            Assert.Equal(2, actionSample.Key.ParameterNames.Count);
            Assert.True(actionSample.Key.ParameterNames.SetEquals(new[] { "id", "name" }));
            Assert.Same(sample, actionSample.Value);
        }

        [Fact]
        public void SetSampleForType()
        {
            HttpConfiguration config = new HttpConfiguration();
            ImageSample sample = new ImageSample("http://localhost/test.png");
            config.SetSampleForType(sample, new MediaTypeHeaderValue("image/png"), typeof(string));
            object sampleGeneratorObj;
            config.Properties.TryGetValue(typeof(HelpPageSampleGenerator), out sampleGeneratorObj);
            HelpPageSampleGenerator sampleGenerator = Assert.IsType<HelpPageSampleGenerator>(sampleGeneratorObj);
            Assert.NotEmpty(sampleGenerator.ActionSamples);
            var actionSample = sampleGenerator.ActionSamples.First();
            Assert.Equal(String.Empty, actionSample.Key.ControllerName);
            Assert.Equal(String.Empty, actionSample.Key.ActionName);
            Assert.Equal(new MediaTypeHeaderValue("image/png"), actionSample.Key.MediaType);
            Assert.Null(actionSample.Key.SampleDirection);
            Assert.Empty(actionSample.Key.ParameterNames);
            Assert.Same(sample, actionSample.Value);
        }

        [Fact]
        public void SetActualRequestType()
        {
            HttpConfiguration config = new HttpConfiguration();
            config.SetActualRequestType(typeof(string), "c", "a");
            object sampleGeneratorObj;
            config.Properties.TryGetValue(typeof(HelpPageSampleGenerator), out sampleGeneratorObj);
            HelpPageSampleGenerator sampleGenerator = Assert.IsType<HelpPageSampleGenerator>(sampleGeneratorObj);
            Assert.NotEmpty(sampleGenerator.ActualHttpMessageTypes);
            var actualType = sampleGenerator.ActualHttpMessageTypes.First();
            Assert.Equal("c", actualType.Key.ControllerName);
            Assert.Equal("a", actualType.Key.ActionName);
            Assert.Null(actualType.Key.MediaType);
            Assert.Equal(SampleDirection.Request, actualType.Key.SampleDirection);
            Assert.NotEmpty(actualType.Key.ParameterNames);
            Assert.Equal("*", actualType.Key.ParameterNames.First());
            Assert.Equal(typeof(string), actualType.Value);
        }

        [Fact]
        public void SetActualRequestType_WithParameters()
        {
            HttpConfiguration config = new HttpConfiguration();
            config.SetActualRequestType(typeof(string), "c", "a", "id");
            object sampleGeneratorObj;
            config.Properties.TryGetValue(typeof(HelpPageSampleGenerator), out sampleGeneratorObj);
            HelpPageSampleGenerator sampleGenerator = Assert.IsType<HelpPageSampleGenerator>(sampleGeneratorObj);
            Assert.NotEmpty(sampleGenerator.ActualHttpMessageTypes);
            var actualType = sampleGenerator.ActualHttpMessageTypes.First();
            Assert.Equal("c", actualType.Key.ControllerName);
            Assert.Equal("a", actualType.Key.ActionName);
            Assert.Null(actualType.Key.MediaType);
            Assert.Equal(SampleDirection.Request, actualType.Key.SampleDirection);
            Assert.NotEmpty(actualType.Key.ParameterNames);
            Assert.Equal("id", actualType.Key.ParameterNames.First());
            Assert.Equal(typeof(string), actualType.Value);
        }

        [Fact]
        public void SetActualResponseType()
        {
            HttpConfiguration config = new HttpConfiguration();
            config.SetActualResponseType(typeof(int), "c", "a");
            object sampleGeneratorObj;
            config.Properties.TryGetValue(typeof(HelpPageSampleGenerator), out sampleGeneratorObj);
            HelpPageSampleGenerator sampleGenerator = Assert.IsType<HelpPageSampleGenerator>(sampleGeneratorObj);
            Assert.NotEmpty(sampleGenerator.ActualHttpMessageTypes);
            var actualType = sampleGenerator.ActualHttpMessageTypes.First();
            Assert.Equal("c", actualType.Key.ControllerName);
            Assert.Equal("a", actualType.Key.ActionName);
            Assert.Null(actualType.Key.MediaType);
            Assert.Equal(SampleDirection.Response, actualType.Key.SampleDirection);
            Assert.NotEmpty(actualType.Key.ParameterNames);
            Assert.Equal("*", actualType.Key.ParameterNames.First());
            Assert.Equal(typeof(int), actualType.Value);
        }

        [Fact]
        public void SetActualResponseType_WithParameters()
        {
            HttpConfiguration config = new HttpConfiguration();
            config.SetActualResponseType(typeof(int), "c", "a", "id");
            object sampleGeneratorObj;
            config.Properties.TryGetValue(typeof(HelpPageSampleGenerator), out sampleGeneratorObj);
            HelpPageSampleGenerator sampleGenerator = Assert.IsType<HelpPageSampleGenerator>(sampleGeneratorObj);
            Assert.NotEmpty(sampleGenerator.ActualHttpMessageTypes);
            var actualType = sampleGenerator.ActualHttpMessageTypes.First();
            Assert.Equal("c", actualType.Key.ControllerName);
            Assert.Equal("a", actualType.Key.ActionName);
            Assert.Null(actualType.Key.MediaType);
            Assert.Equal(SampleDirection.Response, actualType.Key.SampleDirection);
            Assert.NotEmpty(actualType.Key.ParameterNames);
            Assert.Equal("id", actualType.Key.ParameterNames.First());
            Assert.Equal(typeof(int), actualType.Value);
        }

        [Fact]
        public void GetHelpPageSampleGenerator_ReturnsDefaultValue()
        {
            HttpConfiguration config = new HttpConfiguration();
            HelpPageSampleGenerator helpPageSampleGenerator = config.GetHelpPageSampleGenerator();
            Assert.NotNull(helpPageSampleGenerator);
            Assert.Same(helpPageSampleGenerator, config.GetHelpPageSampleGenerator());
        }

        [Fact]
        public void SetHelpPageSampleGenerator_ChangesTheDefault()
        {
            HttpConfiguration config = new HttpConfiguration();
            Mock<HelpPageSampleGenerator> helpPageSampleGenerator = new Mock<HelpPageSampleGenerator>();
            config.SetHelpPageSampleGenerator(helpPageSampleGenerator.Object);
            Assert.Same(helpPageSampleGenerator.Object, config.GetHelpPageSampleGenerator());
        }
    }
}
