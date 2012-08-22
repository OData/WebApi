// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Text.RegularExpressions;
using System.Web.Mvc;
using Microsoft.TestCommon;
using Microsoft.Web.Http.Data.Test;
using Microsoft.Web.UnitTestUtil;

namespace Microsoft.Web.Http.Data.Helpers.Test
{
    public class UpshotExtensionsTests
    {
        private static readonly int _northwindProductsContextHash = -453854022;

        [Fact]
        public void VerifyUpshotContextHelperOutput()
        {
            HtmlHelper html = MvcHelper.GetHtmlHelper(new ViewDataDictionary());
            Regex rgx = new Regex("\\s+");

            string northwindProductsContext = UpshotExtensions.UpshotContext(html, true).DataSource<NorthwindEFTestController>(x => x.GetProducts(), "myUrl", "GetProducts").ToHtmlString();
            int northwindProductsContextHash = rgx.Replace(northwindProductsContext, "").GetHashCode();
            Assert.Equal(northwindProductsContextHash, _northwindProductsContextHash);
        }
    }
}
