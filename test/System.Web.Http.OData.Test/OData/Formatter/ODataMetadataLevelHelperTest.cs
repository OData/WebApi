// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.TestCommon;

namespace System.Web.Http.OData.Formatter
{
    public class ODataMetadataLevelHelperTest : EnumHelperTestBase<ODataMetadataLevel>
    {
        public ODataMetadataLevelHelperTest()
            : base(ODataMetadataLevelHelper.IsDefined, ODataMetadataLevelHelper.Validate, (ODataMetadataLevel)999)
        {
        }
    }
}
