// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Web.Http.Tracing;
using Microsoft.TestCommon;

namespace System.Web.Http.Test
{
    public class HttpConfigurationTracingExtensionsTest
    {
        [Fact]
        public void EnableSystemDiagnosticsTracing_Adds_TraceWriter()
        {
            HttpConfiguration config = new HttpConfiguration();
            SystemDiagnosticsTraceWriter returnedTraceWriter = config.EnableSystemDiagnosticsTracing();
            ITraceWriter setTraceWriter = config.Services.GetService(typeof(ITraceWriter)) as ITraceWriter;

            Assert.ReferenceEquals(returnedTraceWriter, setTraceWriter);
        }

        [Fact]
        public void EnableSystemDiagnosticsTracing_ThrowsIfNull()
        {
            Assert.ThrowsArgumentNull(() => HttpConfigurationTracingExtensions.EnableSystemDiagnosticsTracing(null), "configuration");
        }
    }
}
