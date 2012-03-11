namespace WebMatrix.Data.Test.Mocks
{
    public abstract class MockDbFileHandler : IDbFileHandler
    {
        IConnectionConfiguration IDbFileHandler.GetConnectionConfiguration(string fileName)
        {
            return GetConnectionConfiguration(fileName);
        }

        public abstract MockConnectionConfiguration GetConnectionConfiguration(string fileName);
    }
}
