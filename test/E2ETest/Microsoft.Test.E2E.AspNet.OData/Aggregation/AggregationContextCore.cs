//-----------------------------------------------------------------------------
// <copyright file="AggregationContextCore.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Microsoft.Test.E2E.AspNet.OData.Aggregation
{
    public class AggregationContextCoreBase : DbContext
    {
        public static readonly LoggerFactory ConsoleLoggerFactory = new LoggerFactory
            (new[] { new TraceLoggerProvider() });

        public TraceLoggerProvider Provider { get; set; } = new TraceLoggerProvider();


        public DbSet<Customer> Customers { get; set; }

        public override void Dispose()
        {
            ConsoleLoggerFactory.Dispose();
            base.Dispose();
        }
    }

    public class AggregationContextCoreInMemory : AggregationContextCoreBase
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseInMemoryDatabase("AggregationContextCore");
            base.OnConfiguring(optionsBuilder);
        }
    }


    public class AggregationContextCoreSql : AggregationContextCoreBase
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Data Source=(LocalDb)\MSSQLLocalDB;Integrated Security=True;Persist Security Info = True;Database = AggregationEFCoreTest1");
            optionsBuilder.UseLoggerFactory(ConsoleLoggerFactory);
            base.OnConfiguring(optionsBuilder);
        }
    }

    #region "SQL Logging"
    public class TraceLogger : ILogger
    {
        private readonly string categoryName;

        private Action<string> _callback;

        public TraceLogger(Action<string> callback)
        {
            this.categoryName = DbLoggerCategory.Database.Command.Name;
            this._callback = callback;
        }

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception exception,
            Func<TState, Exception, string> formatter)
        {
            this._callback?.Invoke(formatter(state, exception));
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }
    }

    public class TraceLoggerProvider : ILoggerProvider
    {
        public Microsoft.Extensions.Logging.ILogger CreateLogger(string categoryName)
        {
            return new TraceLogger(Add);
        }

        public static string CurrentSQL { get; set; }
        public void Add(string txt)
        {
            if (txt.StartsWith("Executing DbCommand"))
            {
                CurrentSQL = txt;
            }

        }

        public void Dispose() { }

        internal static void CleanCommands()
        {
            CurrentSQL = "";
        }
    }
    #endregion


}
