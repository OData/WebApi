// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using Moq;
using Xunit;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Web.Http.Filters
{
    public class ConfigurationFilterProviderTest
    {
        private readonly ConfigurationFilterProvider provider = new ConfigurationFilterProvider();

        [Fact]
        public void GetFilters_IfContextParameterIsNull_ThrowsException()
        {
            Assert.ThrowsArgumentNull(() =>
            {
                provider.GetFilters(configuration: null, actionDescriptor: null);
            }, "configuration");
        }

        [Fact]
        public void GetFilters_ReturnsFiltersFromConfiguration()
        {
            var config = new HttpConfiguration();
            IFilter filter1 = new Mock<IFilter>().Object;
            config.Filters.Add(filter1);

            var result = provider.GetFilters(config, actionDescriptor: null);

            Assert.True(result.All(f => f.Scope == FilterScope.Global));
            Assert.Same(filter1, result.ToArray()[0].Instance);
        }
    }
}
