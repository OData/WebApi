//-----------------------------------------------------------------------------
// <copyright file="CaseInsensitiveResolver.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.OData.UriParser;

namespace Microsoft.Test.E2E.AspNet.OData.UriParserExtension
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
