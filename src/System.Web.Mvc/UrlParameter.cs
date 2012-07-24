// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;

namespace System.Web.Mvc
{
    public sealed class UrlParameter
    {
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "This type is immutable.")]
        public static readonly UrlParameter Optional = new UrlParameter();

        // singleton constructor
        private UrlParameter()
        {
        }

        public override string ToString()
        {
            return String.Empty;
        }
    }
}
