using System.Data.Common;

namespace WebMatrix.Data
{
    internal interface IDbProviderFactory
    {
        DbConnection CreateConnection(string connectionString);
    }
}
