using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http.OData;
using System.Web.Http.OData.Extensions;
using System.Web.Http.OData.Routing;

namespace WebStack.QA.Test.OData.Common.Controllers
{
    [ModelStateErrorHandling]
    public abstract class InMemoryEntitySetController<TEntity, TKey> : AsyncEntitySetController<TEntity, TKey>
        where TEntity : class
        //where TKey : struct
    {
        private static ConcurrentDictionary<Type, ConcurrentDictionary<TKey, TEntity>> repository =
            new ConcurrentDictionary<Type, ConcurrentDictionary<TKey, TEntity>>();

        private string idPropertyName;

        protected InMemoryEntitySetController(string idPropertyName)
        {
            this.idPropertyName = idPropertyName;
            LocalTable = repository.GetOrAdd(typeof(TEntity), new ConcurrentDictionary<TKey, TEntity>());
        }

        public static ConcurrentDictionary<Type, ConcurrentDictionary<TKey, TEntity>> Repository
        {
            get
            {
                return repository;
            }
        }

        public IODataPathHandler PathHandler 
        {
            get
            {
                return this.Request.ODataProperties().PathHandler;
            }            
        }

        public ConcurrentDictionary<TKey, TEntity> LocalTable
        {
            get;
            set;
        }

        protected override TKey GetKey(TEntity entity)
        {
            return (TKey)typeof(TEntity).GetProperty(idPropertyName).GetValue(entity, null);
        }

        protected virtual void SetKey(TEntity entity, TKey key)
        {
            typeof(TEntity).GetProperty(idPropertyName).SetValue(entity, key, null);
        }

        protected override System.Threading.Tasks.Task<TEntity> CreateEntityAsync(TEntity entity)
        {
            return Task.Factory.StartNew(() =>
                {
                    LocalTable.AddOrUpdate(
                        GetKey(entity),
                        entity,
                        (id, e) =>
                        {
                            return entity;
                        });

                    return entity;
                });
        }

        protected override Task<TEntity> GetEntityByKeyAsync(TKey key)
        {
            return Task.Factory.StartNew(() =>
                {
                    return this.LocalTable[key];
                });
        }

        [EnableQuery]
        public override Task<System.Collections.Generic.IEnumerable<TEntity>> Get()
        {
            return Task.Factory.StartNew(() =>
            {
                return this.LocalTable.Values.AsEnumerable();
            });
        }

        public override Task Delete([FromODataUri] TKey key)
        {
            return Task.Factory.StartNew(() =>
            {
                TEntity value;
                this.LocalTable.TryRemove(key, out value);
            });
        }

        public virtual void Delete()
        {
            this.LocalTable.Clear();
        }

        protected override Task<TEntity> PatchEntityAsync(TKey key, Delta<TEntity> patch)
        {
            return Task.Factory.StartNew(() =>
            {
                var entity = this.LocalTable[key];
                patch.Patch(entity);
                return entity;
            });
        }

        protected override Task<TEntity> UpdateEntityAsync(TKey key, TEntity update)
        {
            return Task.Factory.StartNew(() =>
            {
                SetKey(update, key);
                if (this.LocalTable.ContainsKey(key))
                {
                    this.LocalTable[key] = update;
                    return update;
                }
                else
                {
                    return null;
                }
            });
        }
    }
}
