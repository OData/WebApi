// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;

namespace System.Web.WebPages.ApplicationParts
{
    // For unit testing purpose since Assembly is not Moqable
    internal interface IResourceAssembly
    {
        string Name { get; }
        Stream GetManifestResourceStream(string name);
        IEnumerable<string> GetManifestResourceNames();
        IEnumerable<Type> GetTypes();
    }
}
