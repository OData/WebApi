// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
namespace Microsoft.Web.Helpers
{
    public enum LinkShareSite
    {
        Delicious,
        Digg,
        [Obsolete("Google Buzz is no longer supported by the Link Share helper. It will be removed entirely in the next major version.")]
        GoogleBuzz,
        Facebook,
        Reddit,
        StumbleUpon,
        Twitter,
        All
    }
}
