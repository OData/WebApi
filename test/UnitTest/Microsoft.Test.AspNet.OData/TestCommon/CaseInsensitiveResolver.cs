// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.OData.UriParser;

namespace Microsoft.Test.AspNet.OData.TestCommon
{
    /// <summary>
    /// Add this class for ODL's issue #695, should remove it and use ODataUriResolver after the issue fix.
    /// </summary>
    public class CaseInsensitiveResolver : ODataUriResolver
    {
        private bool _enableCaseInsensitive;

        public override bool EnableCaseInsensitive
        {
            get { return true; }
            set { _enableCaseInsensitive = value; }
        }
    }
}
