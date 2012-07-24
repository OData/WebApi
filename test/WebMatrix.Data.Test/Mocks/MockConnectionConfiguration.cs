// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace WebMatrix.Data.Test.Mocks
{
    public class MockConnectionConfiguration : IConnectionConfiguration
    {
        public MockConnectionConfiguration(string connectionString)
        {
            ConnectionString = connectionString;
        }

        public string ConnectionString { get; private set; }

        string IConnectionConfiguration.ConnectionString
        {
            get { return ConnectionString; }
        }

        IDbProviderFactory IConnectionConfiguration.ProviderFactory
        {
            get { return null; }
        }
    }
}
