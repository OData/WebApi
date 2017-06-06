// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.OData.UriParser;

namespace System.Web.OData.Test.TestCommon
{
    /// <summary>
    /// Add this class for ODL's issue #695, should remove it and use UnqualifiedODataUriResolver after the issue fix.
    /// </summary>
    public class UnqualifiedCaseInsensitiveResolver : UnqualifiedODataUriResolver
    {
        private bool _enableCaseInsensitive;

        public override bool EnableCaseInsensitive
        {
            get { return true; }
            set { _enableCaseInsensitive = value; }
        }
    }
}
