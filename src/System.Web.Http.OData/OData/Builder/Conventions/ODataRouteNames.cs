// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Web.Http.OData.Builder.Conventions
{
    public static class ODataRouteNames
    {
        public const string Metadata = "OData.$metadata";
        public const string ServiceDocument = "OData.servicedoc";
        public const string PropertyNavigation = "OData.PropertyNavigation";
        public const string Link = "OData.Link";
        public const string GetById = "OData.GetById";
        public const string Default = "OData.Default";
        public const string DefaultWithParentheses = "OData.DefaultWithParentheses";
    }
}
