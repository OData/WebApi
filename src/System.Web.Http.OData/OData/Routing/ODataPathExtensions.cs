// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

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

        public static IEdmEntitySet GetEntitySet(this ODataPath path)
        {
            if (path == null)
            {
                throw Error.ArgumentNull("path");
            }

            Contract.Assert(path.Segments != null);
            EntitySetPathSegment entitySetSegment = (EntitySetPathSegment)path.Segments.FirstOrDefault(
                s => s is EntitySetPathSegment);

            if (entitySetSegment == null)
            {
                return null;
            }

            return entitySetSegment.EntitySet;
        }
    }
}
