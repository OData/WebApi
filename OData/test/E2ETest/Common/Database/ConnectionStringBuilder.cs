using System;
using System.Data.SqlClient;

namespace WebStack.QA.Common.Database
{
    /// <summary>
    /// Fluent API to build connection string
    /// </summary>
    public class ConnectionStringBuilder
    {
        private SqlConnectionStringBuilder _builder;

        public ConnectionStringBuilder(string connectionString = null)
        {
            if (connectionString != null)
            {
                _builder = new SqlConnectionStringBuilder(connectionString);
            }
            else
            {
                _builder = new SqlConnectionStringBuilder();
            }
        }

        public string UserID
        {
            get
            {
                return _builder.UserID;
            }
            set
            {

                if (!string.IsNullOrEmpty(value))
                {
                    _builder.UserID = value;
                }
            }
        }

        public string Password
        {
            get
            {
                return _builder.Password;
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    _builder.Password = value;
                }
            }
        }

        public string DataSource
        {
            get
            {
                return _builder.DataSource;
            }
            set
            {
                _builder.DataSource = value;
            }
        }

        public bool IntegratedSecurity
        {
            get
            {
                return _builder.IntegratedSecurity;
            }
            set
            {
                _builder.IntegratedSecurity = value;
            }
        }

        public string InitialCatalog
        {
            get
            {
                return _builder.InitialCatalog;
            }
            set
            {
                _builder.InitialCatalog = value;
            }
        }

        public bool MultipleActiveResultSets
        {
            get
            {
                return _builder.MultipleActiveResultSets;
            }
            set
            {
                _builder.MultipleActiveResultSets = value;
            }
        }

        public ConnectionStringBuilder UseLocalSqlExpress()
        {
            this.IntegratedSecurity = true;
            this.DataSource = ".\\SQLEXPRESS";
            this.MultipleActiveResultSets = true;

            return this;
        }

        public ConnectionStringBuilder UseLocalSqlExpress(string userID, string password)
        {
            this.UserID = userID;
            this.Password = password;
            this.IntegratedSecurity = false;
            this.DataSource = ".\\SQLEXPRESS";
            this.MultipleActiveResultSets = true;

            return this;
        }

        public ConnectionStringBuilder UseLocalDB()
        {
            this.DataSource = "(LocalDB)\\v11.0";
            this.IntegratedSecurity = true;
            this.MultipleActiveResultSets = true;

            return this;
        }

        public ConnectionStringBuilder UseRandomDBName(string prefix = null)
        {
            if (prefix == null)
            {
                prefix = string.Empty;
            }

            this.InitialCatalog = prefix + "_" + DateTime.UtcNow.Ticks;

            return this;
        }

        public string ConnectionString
        {
            get
            {
                return _builder.ConnectionString;
            }
        }
    }
}
