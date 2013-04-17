// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.TestCommon;

namespace System.Web.WebPages.Test
{
    public class LabelHelperTest
    {
        [Fact]
        public void LabelWithAttributesFromAnonymousObject_WithUnderscoreInName_TransformsUnderscoresToDashs()
        {
            HtmlHelperTest.AssertHelperTransformsAttributesUnderscoresToDashs((helper, attributes) =>
                helper.Label("foo", attributes));

            HtmlHelperTest.AssertHelperTransformsAttributesUnderscoresToDashs((helper, attributes) =>
                helper.Label("foo", "bar", attributes));
        }
    }
}
