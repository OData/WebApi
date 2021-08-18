//-----------------------------------------------------------------------------
// <copyright file="AsyncInMemoryODataController.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing;

namespace Microsoft.Test.E2E.AspNet.OData.Common.Controllers
{
    /// <summary>
    /// Pay attention to use this class when you create test cases in parallel.
    /// If multiple test cases rely on the same <typeparamref name="TEntity"/>, these test cases maybe conflict each other.
    /// So, do not use the same <typeparamref name="TEntity"/> in mulitple test cases.
    /// </summary>
    [ModelStateErrorHandling]
    public abstract class InMemoryODataController<TEntity, TKey> : TestODataController
        where TEntity : class
    {
        private static ConcurrentDictionary<Type, ConcurrentDictionary<TKey, TEntity>> repository =
                   new ConcurrentDictionary<Type, ConcurrentDictionary<TKey, TEntity>>();

        private string idPropertyName;

        protected InMemoryODataController(string idPropertyName)
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
                return this.Request.GetPathHandler();
            }
        }

        public ConcurrentDictionary<TKey, TEntity> LocalTable
        {
            get;
            set;
        }

        protected TKey GetKey(TEntity entity)
        {
            return (TKey)typeof(TEntity).GetProperty(idPropertyName).GetValue(entity, null);
        }

        protected virtual void SetKey(TEntity entity, TKey key)
        {
            typeof(TEntity).GetProperty(idPropertyName).SetValue(entity, key, null);
        }

        protected virtual Task<ITestActionResult> CreateEntityAsync(TEntity entity)
        {
            return Task<ITestActionResult>.Factory.StartNew(() =>
                {
                    LocalTable.AddOrUpdate(
                        GetKey(entity),
                        entity,
                        (id, e) =>
                        {
                            return entity;
                        });

                    return Created<TEntity>(entity);
                });
        }

        public virtual Task<ITestActionResult> Post([FromBody]TEntity entity)
        {
            return this.CreateEntityAsync(entity);
        }

        public Task<TEntity> Get([FromODataUri] TKey key)
        {
            return Task.Factory.StartNew(() =>
            {
                return this.LocalTable[key];
            });
        }

        protected Task<TEntity> GetEntityByKeyAsync(TKey key)
        {
            return Task.Factory.StartNew(() =>
            {
                return this.LocalTable[key];
            });
        }

        [EnableQuery]
        public virtual Task<System.Collections.Generic.IEnumerable<TEntity>> Get()
        {
            return Task.Factory.StartNew(() =>
            {
                return this.LocalTable.Values.AsEnumerable();
            });
        }

        public Task Delete([FromODataUri] TKey key)
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

        protected Task<TEntity> PatchEntityAsync(TKey key, [FromBody]Delta<TEntity> patch)
        {
            return Task.Factory.StartNew(() =>
            {
                var entity = this.LocalTable[key];
                patch.Patch(entity);
                return entity;
            });
        }

        public async Task<ITestActionResult> Patch([FromODataUri]TKey key, [FromBody]Delta<TEntity> patch)
        {
            TEntity patchedEntity = await PatchEntityAsync(key, patch);
            return Updated(patchedEntity);
        }

        protected Task<TEntity> UpdateEntityAsync(TKey key, [FromBody]TEntity update)
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
