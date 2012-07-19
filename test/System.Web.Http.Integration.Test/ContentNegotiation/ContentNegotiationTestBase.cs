// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Web.Http.ContentNegotiation
{
    public abstract class ContentNegotiationTestBase : HttpServerTestBase
    {
        protected ContentNegotiationTestBase()
            : base("http://localhost/Conneg")
        {
        }

        protected override void ApplyConfiguration(HttpConfiguration configuration)
        {
            configuration.Routes.MapHttpRoute("Default", "{controller}", new { controller = "Conneg" });
        }
    }
}
