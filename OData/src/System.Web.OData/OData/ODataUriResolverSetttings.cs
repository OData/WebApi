// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.OData.Core.UriParser.Metadata;

namespace System.Web.OData
{
    internal class ODataUriResolverSetttings
    {
        public bool CaseInsensitive { get; set; }

        public bool UnqualifiedNameCall { get; set; }

        public bool EnumPrefixFree { get; set; }

        public ODataUriResolver CreateResolver()
        {
            ODataUriResolver resolver;
            if (UnqualifiedNameCall && EnumPrefixFree)
            {
                resolver = new UnqualifiedCallAndEnumPrefixFreeResolver();
            }
            else if (UnqualifiedNameCall)
            {
                resolver = new UnqualifiedODataUriResolver();
            }
            else if (EnumPrefixFree)
            {
                resolver = new StringAsEnumResolver();
            }
            else
            {
                resolver = new ODataUriResolver();
            }

            resolver.EnableCaseInsensitive = CaseInsensitive;
            return resolver;
        }
    }
}
