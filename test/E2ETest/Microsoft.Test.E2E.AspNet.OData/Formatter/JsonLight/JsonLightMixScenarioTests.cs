﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.OData.Client;
using Microsoft.Test.E2E.AspNet.OData.Common;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.Formatter.JsonLight
{
    public class JsonLightMixScenarioTests : MixScenarioTestsOData
    {
        public JsonLightMixScenarioTests(WebHostTestFixture fixture)
            :base(fixture)
        {
        }

        public string AcceptHeader { get; set; }

        public override DataServiceContext WriterClient(Uri serviceRoot, ODataProtocolVersion protocolVersion)
        {
            var ctx = base.WriterClient(serviceRoot, protocolVersion);
            new JsonLightConfigurator(ctx, AcceptHeader).Configure();
            return ctx;
        }

        public override DataServiceContext ReaderClient(Uri serviceRoot, ODataProtocolVersion protocolVersion)
        {
            var ctx = base.ReaderClient(serviceRoot, protocolVersion);
            new JsonLightConfigurator(ctx, AcceptHeader).Configure();
            return ctx;
        }

        protected override void UpdateConfiguration(WebRouteConfiguration configuration)
        {
            configuration.EnableODataSupport(GetEdmModel(configuration), "odata");
        }

        [Theory]
        [InlineData("application/json;odata.metadata=minimal;odata.streaming=true")]
        [InlineData("application/json;odata.metadata=minimal;odata.streaming=false")]
        [InlineData("application/json;odata.metadata=minimal")]
        [InlineData("application/json;odata.metadata=full;odata.streaming=true")]
        [InlineData("application/json;odata.metadata=full;odata.streaming=false")]
        [InlineData("application/json;odata.metadata=full")]
        [InlineData("application/json;odata.streaming=true")]
        [InlineData("application/json;odata.streaming=false")]
        [InlineData("application/json")]
        public Task ODataCRUDShouldWorkJsonLight(string acceptHeader)
        {
            AcceptHeader = acceptHeader;
            return ODataCRUDShouldWorkAsync();
        }
    }
}
