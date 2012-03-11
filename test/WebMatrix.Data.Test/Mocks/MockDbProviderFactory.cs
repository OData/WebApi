using System.Data.Common;

namespace WebMatrix.Data.Test.Mocks
{
    // Needs to be public for Moq to work
    public abstract class MockDbProviderFactory : IDbProviderFactory
    {
        public abstract DbConnection CreateConnection(string connectionString);
    }
}
