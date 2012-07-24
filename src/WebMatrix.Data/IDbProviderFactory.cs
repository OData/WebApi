// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Data.Common;

namespace WebMatrix.Data
{
    internal interface IDbProviderFactory
    {
        DbConnection CreateConnection(string connectionString);
    }
}
