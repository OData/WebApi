// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Web.Http.ModelBinding
{
    /// <summary>
    /// End to end functional tests for model binding
    /// </summary>
    public abstract class ModelBindingTests : HttpServerTestBase
    {
        protected ModelBindingTests()
            : base("http://localhost/")
        {
        }

        protected override void ApplyConfiguration(HttpConfiguration configuration)
        {
            configuration.Routes.MapHttpRoute("Default", "{controller}/{action}", new { controller = "ModelBinding" });
        }
    }
}