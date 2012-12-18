// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Web.Http.OData.Formatter.Serialization
{
    internal class EntitySelfLinks
    {
        public string IdLink { get; set; }

        public Uri EditLink { get; set; }

        public Uri ReadLink { get; set; }
    }
}
