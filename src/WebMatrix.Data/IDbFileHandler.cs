namespace WebMatrix.Data
{
    internal interface IDbFileHandler
    {
        IConnectionConfiguration GetConnectionConfiguration(string fileName);
    }
}
