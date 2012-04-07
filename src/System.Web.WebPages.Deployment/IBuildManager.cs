// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.IO;

namespace System.Web.WebPages.Deployment
{
    internal interface IBuildManager
    {
        Stream CreateCachedFile(string fileName);

        Stream ReadCachedFile(string fileName);
    }
}
