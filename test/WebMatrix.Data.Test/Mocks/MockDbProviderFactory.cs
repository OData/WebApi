// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Data.Common;

namespace WebMatrix.Data.Test.Mocks
{
    // Needs to be public for Moq to work
    public abstract class MockDbProviderFactory : IDbProviderFactory
    {
        public abstract DbConnection CreateConnection(string connectionString);
    }
}
