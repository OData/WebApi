// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Text.RegularExpressions;
using System.Web.Mvc;
using Microsoft.Web.Http.Data.Test;
using Microsoft.Web.UnitTestUtil;
using Xunit;
using Assert = Microsoft.TestCommon.AssertEx;

namespace Microsoft.Web.Http.Data.Helpers.Test
{
    public class UpshotExtensionsTests
    {
        private static readonly int _catalogProductsContextHash = 596152332;
        private static readonly int _northwindProductsContextHash = -453854022;
        private static readonly int _citiesContextHash = -237703404;

        [Fact]
        public void VerifyUpshotContextHelperOutput()
        {
            HtmlHelper html = MvcHelper.GetHtmlHelper(new ViewDataDictionary());
            Regex rgx = new Regex("\\s+");

            string catalogProductsContext = UpshotExtensions.UpshotContext(html, true).DataSource<CatalogController>(x => x.GetProducts(), "myUrl", "GetProducts").ToHtmlString();
            int catalogProductsContextHash = rgx.Replace(catalogProductsContext, "").GetHashCode();
            Assert.Equal(catalogProductsContextHash, _catalogProductsContextHash);

            string northwindProductsContext = UpshotExtensions.UpshotContext(html, true).DataSource<NorthwindEFTestController>(x => x.GetProducts(), "myUrl", "GetProducts").ToHtmlString();
            int northwindProductsContextHash = rgx.Replace(northwindProductsContext, "").GetHashCode();
            Assert.Equal(northwindProductsContextHash, _northwindProductsContextHash);

            string citiesContext = UpshotExtensions.UpshotContext(html, true).DataSource<CitiesController>(x => x.GetCities(), "myUrl", "GetCities").ToHtmlString();
            int citiesContextHash = rgx.Replace(citiesContext, "").GetHashCode();
            Assert.Equal(citiesContextHash, _citiesContextHash);
        }
    }
}
