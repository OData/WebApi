// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Diagnostics.Contracts;
using System.Linq;
using Microsoft.Data.Edm;

namespace System.Web.Http.OData.Routing
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
