using System;
using Nuwa.WebStack.Host;
using WebStack.QA.Common.Database;
using WebStack.QA.Common.WebHost;

namespace Nuwa.WebStack
{
    /// <summary>
    /// This configuration set web.config transforms to change connection string to sql database.
    /// It alters DB types based on host types: IIS Express => LocalDB, IIS => SQL Express, Azure => 
    /// SQL Azure
    /// </summary>
    public class NuwaConnectionStringAttribute : Attribute, IWebHostConfiguration
    {
        private const string NuwaDBContextKey = "Nuwa.NuwaDBContext";

        public NuwaConnectionStringAttribute(string connectionStringName)
        {
            ConnectionStringName = connectionStringName;
        }

        public string ConnectionStringName { get; set; }

        public void Initialize(WebHostContext context)
        {
            ITestDatabase db;
            string connectionString;
            string prefix = context.TestType.TestTypeInfo.Type.Name;
            if (context.HostOptions is IISExpressHostOptions)
            {
                connectionString = new ConnectionStringBuilder().UseLocalDB().UseRandomDBName(prefix).ToString();
                db = new SqlTestDatabase(connectionString);
            }
            else if (context.HostOptions is IISHostOptions)
            {
                connectionString = new ConnectionStringBuilder().UseLocalSqlExpress().UseRandomDBName(prefix).ToString();
                db = new SqlTestDatabase(connectionString);
            }
            else if (context.HostOptions is AzureWebsiteHostOptions)
            {
                connectionString = NuwaGlobalConfiguration.SqlAzureConnectionString;
                db = new SqlTestDatabase(connectionString);
            }
            else
            {
                throw new NotSupportedException(context.HostOptions.GetType().Name);
            }

            if (db.Exist())
            {
                db.Delete();
            }

            context.Properties.Add(NuwaDBContextKey, db);

            context.DeploymentOptions.WebConfigTransformers.Add(new WebConfigTransformer(
                config =>
                {
                    config.ClearConnectionStrings();
                    config.AddConnectionString(ConnectionStringName, connectionString, "System.Data.SqlClient");
                }));
        }

        public void TearDown(WebHostContext context)
        {
            var db = context.Properties[NuwaDBContextKey] as ITestDatabase;
            if (db != null && db.Exist())
            {
                db.Delete();
            }
        }
    }
}
