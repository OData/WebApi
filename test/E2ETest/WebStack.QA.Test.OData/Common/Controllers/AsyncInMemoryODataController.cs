﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing;

namespace WebStack.QA.Test.OData.Common.Controllers
{
    [ModelStateErrorHandling]
    public abstract class InMemoryODataController<TEntity, TKey> : ODataController
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

        protected virtual Task<TEntity> CreateEntityAsync(TEntity entity)
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

        public virtual async Task<IHttpActionResult> Post(TEntity entity)
        {
            TEntity createdEntity = await CreateEntityAsync(entity);
            return Created<TEntity>(createdEntity);
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

        protected Task<TEntity> PatchEntityAsync(TKey key, Delta<TEntity> patch)
        {
            return Task.Factory.StartNew(() =>
            {
                var entity = this.LocalTable[key];
                patch.Patch(entity);
                return entity;
            });
        }

        public async Task<IHttpActionResult> Patch([FromODataUri]TKey key, Delta<TEntity> patch)
        {
            TEntity patchedEntity = await PatchEntityAsync(key, patch);
            return Updated(patchedEntity);
        }

        protected Task<TEntity> UpdateEntityAsync(TKey key, TEntity update)
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
