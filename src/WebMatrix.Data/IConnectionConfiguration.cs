namespace WebMatrix.Data
{
    internal interface IConnectionConfiguration
    {
        string ConnectionString { get; }
        IDbProviderFactory ProviderFactory { get; }
    }
}
