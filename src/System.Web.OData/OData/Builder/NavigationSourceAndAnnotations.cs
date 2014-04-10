// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.OData.Edm.Library;

namespace System.Web.OData.Builder
{
    /// <summary>
    /// This class is used in internal as a helper class to build the Edm model.
    /// This class wrappers a relationship between Edm type and the CLR type configuration.
    /// This relationship is used to builder the navigation property and the corresponding links.
    /// </summary>
    internal class NavigationSourceAndAnnotations
    {
        public EdmNavigationSource NavigationSource { get; set; }
        public NavigationSourceConfiguration Configuration { get; set; }
        public NavigationSourceLinkBuilderAnnotation LinkBuilder { get; set; }
        public NavigationSourceUrlAnnotation Url { get; set; }
    }
}
