//-----------------------------------------------------------------------------
// <copyright file="ODataConfigurationExtensionsTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNet.OData.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData;
using Xunit;

namespace Microsoft.AspNet.OData.Test.Extensions
{
    public class ODataConfigurationExtensionsTest
    {
        [Fact]
        public void SetDefaultODataOptions()
        {
            var defaultOptions = new ODataOptions
            {
                NullDynamicPropertyIsEnabled = false,
                EnableContinueOnErrorHeader = false,
                CompatibilityOptions = CompatibilityOptions.AllowNextLinkWithNonPositiveTopValue,
                UrlKeyDelimiter = ODataUrlKeyDelimiter.Slash
            };
            var services = new ServiceCollection();
            services.Insert(0, ServiceDescriptor.Singleton(new ODataOptions
            {
                NullDynamicPropertyIsEnabled = true,
                EnableContinueOnErrorHeader = true,
                CompatibilityOptions = CompatibilityOptions.None,
                UrlKeyDelimiter = ODataUrlKeyDelimiter.Parentheses
            }));
            ODataOptions updatedOptions;
            using (var serviceProvider = services.BuildServiceProvider())
            {
                serviceProvider.SetDefaultODataOptions(defaultOptions);
                updatedOptions = serviceProvider.GetRequiredService<ODataOptions>();
            }

            Assert.Equal(defaultOptions.NullDynamicPropertyIsEnabled, updatedOptions.NullDynamicPropertyIsEnabled);
            Assert.Equal(defaultOptions.EnableContinueOnErrorHeader, updatedOptions.EnableContinueOnErrorHeader);
            Assert.Equal(defaultOptions.CompatibilityOptions, updatedOptions.CompatibilityOptions);
            Assert.Equal(defaultOptions.UrlKeyDelimiter, updatedOptions.UrlKeyDelimiter);
        }
    }
}
