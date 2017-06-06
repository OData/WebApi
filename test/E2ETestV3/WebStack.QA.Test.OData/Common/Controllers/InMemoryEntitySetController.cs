using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Web.Http;
using System.Web.Http.OData;
using System.Web.Http.OData.Routing;

namespace WebStack.QA.Test.OData.Common.Controllers
{
    [ModelStateErrorHandling]
    public abstract class InMemoryEntitySetController<TEntity, TKey> : EntitySetController<TEntity, TKey>
        where TEntity : class, new()
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
                return this.Configuration.GetODataPathHandler();
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

        protected override TEntity CreateEntity(TEntity entity)
        {
            LocalTable.AddOrUpdate(
                GetKey(entity),
                entity,
                (id, e) =>
                {
                    return entity;
                });

            return entity;
        }

        protected override TEntity GetEntityByKey(TKey key)
        {
            return this.LocalTable[key];
        }

        public override IQueryable<TEntity> Get()
        {
            return this.LocalTable.Values.AsQueryable();
        }

        public override void Delete([FromODataUri] TKey key)
        {
            TEntity value;
            this.LocalTable.TryRemove(key, out value);
        }

        public virtual void Delete()
        {
            this.LocalTable.Clear();
        }

        protected override TEntity PatchEntity(TKey key, System.Web.Http.OData.Delta<TEntity> patch)
        {
            var entity = GetEntityByKey(key);
            patch.Patch(entity);
            return entity;
        }

        protected override TEntity UpdateEntity(TKey key, TEntity update)
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
        }
    }
}
