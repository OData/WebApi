using System;
using WebStack.QA.Common.Database;
using Xunit;

namespace WebStack.QA.Common.Tests
{
    public class SqlDBTests
    {
        [Fact]
        public void CreateDeleteSqlExpress()
        {
            var connectionString = new ConnectionStringBuilder().UseLocalSqlExpress().UseRandomDBName("MyDB").ConnectionString;
            Console.WriteLine(connectionString);
            var db = new SqlTestDatabase(connectionString);
            Assert.False(db.Exist());
            db.Create();
            Assert.True(db.Exist());
            db.Delete();
            Assert.False(db.Exist());
        }

        [Fact]
        public void CreateDeleteLocalDB()
        {
            var connectionString = new ConnectionStringBuilder().UseLocalDB().UseRandomDBName("MyDB").ConnectionString;
            Console.WriteLine(connectionString);
            var db = new SqlTestDatabase(connectionString);
            Assert.False(db.Exist());
            db.Create();
            Assert.True(db.Exist());
            db.Delete();
            Assert.False(db.Exist());
        }
    }
}
