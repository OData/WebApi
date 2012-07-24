// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace WebMatrix.Data
{
    internal interface IConfigurationManager
    {
        IDictionary<string, string> AppSettings { get; }
        IConnectionConfiguration GetConnection(string name);
    }
}
