//-----------------------------------------------------------------------------
// <copyright file="ETagCurrencyTokenEfContext.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.ETags
{
    public class ETagCurrencyTokenEfContext : DbContext
    {
        public static string ConnectionString = @"Data Source=(LocalDb)\MSSQLLocalDB;Integrated Security=True;Initial Catalog=ETagCurrencyTokenEfContext1";

        public ETagCurrencyTokenEfContext()
            : base(ConnectionString)
        {
            Database.SetInitializer(new ETagCurrencyTokenEfContextInitializer());
        }

        public IDbSet<Dominio> Dominios { get; set; }

        public IDbSet<Server> Servers { get; set; }
    }

    public class ETagCurrencyTokenEfContextInitializer : DropCreateDatabaseAlways<ETagCurrencyTokenEfContext>
    {
        private static IEnumerable<Dominio> _dominios;
        private static IEnumerable<Server> _servers;

        private static IEnumerable<Dominio> Dominios
        {
            get
            {
                if (_dominios == null)
                {
                    Generate();
                }

                Assert.NotNull(_dominios);
                return _dominios;
            }
        }

        private static IEnumerable<Server> Servers
        {
            get
            {
                if (_servers == null)
                {
                    Generate();
                }

                Assert.NotNull(_servers);
                return _servers;
            }
        }

        private static void Generate()
        {
            if (_dominios != null || _servers != null)
            {
                return;
            }

            List<Server> servers = new List<Server>(2);
            Server server1 = new Server
            {
                Id = "1",
                Descrizione = "Server 1",
                Url = "http://server1",
                RECVER = null
            };
            servers.Add(server1);

            Server server2 = new Server
            {
                Id = "2",
                Descrizione = "Server 2",
                Url = "http://server2",
                RECVER = 5
            };
            servers.Add(server2);
            _servers = servers;

            List<Dominio> dominios = new List<Dominio>(2);
            Dominio do1 = new Dominio
            {
                Id = "1",
                ServerAutenticazione = server1,
                Descrizione = "Test1",
                RECVER = null,
                ServerAutenticazioneId = "1",
            };
            dominios.Add(do1);

            Dominio do2 = new Dominio
            {
                Id = "2",
                ServerAutenticazione = server2,
                Descrizione = "Test2",
                RECVER = 10,
                ServerAutenticazioneId = "2",
            };

            dominios.Add(do2);
            _dominios = dominios;
        }

        protected override void Seed(ETagCurrencyTokenEfContext context)
        {
            context.Configuration.AutoDetectChangesEnabled = false;

            Generate();
            foreach (var d in Dominios)
            {
                context.Dominios.Add(d);
            }

            foreach (var s in Servers)
            {
                context.Servers.Add(s);
            }

            context.SaveChanges();
            base.Seed(context);
        }
    }

    [Table("Domini")]
    public class Dominio
    {
        [Key]
        [StringLength(50)]
        public string Id { get; set; }

        [StringLength(200)]
        public string Descrizione { get; set; }

        public string ServerAutenticazioneId { get; set; }

        [ForeignKey("ServerAutenticazioneId")]
        public virtual Server ServerAutenticazione { get; set; }

        [ConcurrencyCheck]
        public int? RECVER { get; set; }
    }

    [Table("Servers")]
    public class Server
    {
        [Key]
        [StringLength(50)]
        public string Id { get; set; }

        [StringLength(200)]
        public string Descrizione { get; set; }

        [StringLength(2000)]
        public string Url { get; set; }

        [ConcurrencyCheck]
        public int? RECVER { get; set; }
    }
}
