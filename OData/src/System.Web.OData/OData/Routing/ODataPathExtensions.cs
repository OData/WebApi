// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Diagnostics.Contracts;
using System.Linq;
using System.Web.Http;
using Microsoft.OData.Edm;

namespace System.Web.OData.Routing
{
    internal static class ODataPathExtensions
    {
        public static IEdmNavigationProperty GetNavigationProperty(this ODataPath path)
        {
            if (path == null)
            {
                throw Error.ArgumentNull("path");
            }

            Contract.Assert(path.Segments != null);
            NavigationPathSegment navigationSegment = (NavigationPathSegment)path.Segments.FirstOrDefault(
                s => s is NavigationPathSegment);

            if (navigationSegment == null)
            {
                return null;
            }

            return navigationSegment.NavigationProperty;
        }
    }
}
