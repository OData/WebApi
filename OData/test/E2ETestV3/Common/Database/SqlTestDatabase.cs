using System;
using System.Data.SqlClient;

namespace WebStack.QA.Common.Database
{
    /// <summary>
    /// SQL server based DB instance
    /// eg. SQL express, LocalDB, Sql Azure
    /// </summary>
    public class SqlTestDatabase : ITestDatabase
    {
        public SqlTestDatabase(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentException("connectionString");
            }

            this.ConnectionString = connectionString;
            this.Timeout = 1000;
        }

        public string ConnectionString { get; set; }

        public int? Timeout { get; set; }

        public void Create()
        {
            SqlConnectionStringBuilder sqlConnectionStringBuilder = new SqlConnectionStringBuilder(this.ConnectionString);

            if (string.IsNullOrEmpty(sqlConnectionStringBuilder.InitialCatalog))
            {
                throw new InvalidOperationException("Missing InitialCatalog in connection string");
            }

            string databaseName = sqlConnectionStringBuilder.InitialCatalog;
            string createDatabaseScript = CreateDatabaseScript(databaseName);

            using (var conn = CreateMasterConnection(this.ConnectionString))
            {
                using (SqlCommand sqlCommand = new SqlCommand(createDatabaseScript, conn))
                {
                    if (this.Timeout.HasValue)
                    {
                        sqlCommand.CommandTimeout = this.Timeout.Value;
                    }

                    sqlCommand.ExecuteNonQuery();
                }
            }
        }

        public bool Exist()
        {
            SqlConnectionStringBuilder sqlConnectionStringBuilder = new SqlConnectionStringBuilder(this.ConnectionString);

            if (string.IsNullOrEmpty(sqlConnectionStringBuilder.InitialCatalog))
            {
                throw new InvalidOperationException("Missing InitialCatalog in connection string");
            }

            return CheckDatabaseExists(this.ConnectionString, Timeout, sqlConnectionStringBuilder.InitialCatalog);
        }

        public void Delete()
        {
            SqlConnectionStringBuilder sqlConnectionStringBuilder = new SqlConnectionStringBuilder(this.ConnectionString);
            if (string.IsNullOrEmpty(sqlConnectionStringBuilder.InitialCatalog))
            {
                throw new InvalidOperationException("Missing InitialCatalog in connection string");
            }

            string databaseName = sqlConnectionStringBuilder.InitialCatalog;
            SqlConnection.ClearAllPools();
            string dropDatabaseScript = DropDatabaseScript(databaseName);

            using (SqlConnection masterConn = CreateMasterConnection(this.ConnectionString))
            {
                ClearAllConnections(masterConn, Timeout, databaseName);
                using (SqlCommand sqlCommand = new SqlCommand(dropDatabaseScript, masterConn))
                {
                    if (Timeout.HasValue)
                    {
                        sqlCommand.CommandTimeout = Timeout.Value;
                    }

                    sqlCommand.ExecuteNonQuery();
                }
            }
        }

        private void ClearAllConnections(SqlConnection conn, int? commandTimeout, string databaseName)
        {
            var clearConnectionsScript = ClearConnectionsScript(databaseName);
            using (SqlCommand sqlCommand = new SqlCommand(clearConnectionsScript, conn))
            {
                if (commandTimeout.HasValue)
                {
                    sqlCommand.CommandTimeout = commandTimeout.Value;
                }

                try
                {
                    sqlCommand.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        private bool CheckDatabaseExists(string connectionString, int? commandTimeout, string databaseName)
        {
            using (SqlConnection masterConn = CreateMasterConnection(connectionString))
            {
                string commandText = CreateDatabaseExistsScript(databaseName);
                using (SqlCommand sqlCommand = new SqlCommand(commandText, masterConn))
                {
                    if (commandTimeout.HasValue)
                    {
                        sqlCommand.CommandTimeout = commandTimeout.Value;
                    }

                    int num = (int)sqlCommand.ExecuteScalar();
                    return (num > 0);
                }
            }
        }

        private string ClearConnectionsScript(string databaseName)
        {
            return string.Format("alter database [{0}] set single_user with rollback immediate", databaseName);
        }

        private string DropDatabaseScript(string databaseName)
        {
            return string.Format("DROP DATABASE [{0}]", databaseName);
        }

        private string CreateDatabaseExistsScript(string databaseName)
        {
            return string.Format("SELECT COUNT(*) FROM sys.databases WHERE [name]=N'{0}'", databaseName);
        }

        private string CreateDatabaseScript(string databaseName)
        {
            return string.Format("CREATE DATABASE [{0}]", databaseName);
        }

        private SqlConnection CreateMasterConnection(string connectionString)
        {
            SqlConnectionStringBuilder sqlConnectionStringBuilder = new SqlConnectionStringBuilder(connectionString)
            {
                InitialCatalog = "master",
                AttachDBFilename = string.Empty
            };
            return CreateConnection(sqlConnectionStringBuilder.ConnectionString);
        }

        private SqlConnection CreateConnection(string connectionString)
        {
            SqlConnection conn = new SqlConnection(connectionString);
            conn.Open();
            return conn;
        }
    }
}
