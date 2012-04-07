// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Objects;
using System.Linq;
using System.Web.Http.Controllers;
using Microsoft.Web.Http.Data.EntityFramework.Metadata;

namespace Microsoft.Web.Http.Data.EntityFramework
{
    /// <summary>
    /// Base class for DataControllers operating on LINQ To Entities data models
    /// </summary>
    /// <typeparam name="TContext">The Type of the LINQ To Entities ObjectContext</typeparam>
    [LinqToEntitiesMetadataProvider]
    public abstract class LinqToEntitiesDataController<TContext> : DataController where TContext : ObjectContext, new()
    {
        private TContext _objectContext;
        private TContext _refreshContext;

        /// <summary>
        /// Protected constructor because this is an abstract class
        /// </summary>
        protected LinqToEntitiesDataController()
        {
        }

        /// <summary>
        /// Gets the <see cref="ObjectContext"/>
        /// </summary>
        protected internal TContext ObjectContext
        {
            get
            {
                if (_objectContext == null)
                {
                    _objectContext = CreateObjectContext();
                }
                return _objectContext;
            }
        }

        /// <summary>
        /// Gets the <see cref="ObjectContext"/> used by retrieving store values
        /// </summary>
        private ObjectContext RefreshContext
        {
            get
            {
                if (_refreshContext == null)
                {
                    _refreshContext = CreateObjectContext();
                }
                return _refreshContext;
            }
        }

        /// <summary>
        /// Initializes this <see cref="DataController"/>.
        /// </summary>
        /// <param name="controllerContext">The <see cref="HttpControllerContext"/> for this <see cref="DataController"/>
        /// instance. Overrides must call the base method.</param>
        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);

            // TODO: should we be turning this off categorically? Can we do this only
            // for queries?
            ObjectContext.ContextOptions.LazyLoadingEnabled = false;

            // We turn this off, since our deserializer isn't going to create
            // the EF proxy types anyways. Proxies only really work if the entities
            // are queried on the server.
            ObjectContext.ContextOptions.ProxyCreationEnabled = false;
        }

        /// <summary>
        /// Creates and returns the <see cref="ObjectContext"/> instance that will
        /// be used by this provider.
        /// </summary>
        /// <returns>The ObjectContext</returns>
        protected virtual TContext CreateObjectContext()
        {
            return new TContext();
        }

        /// <summary>
        /// See <see cref="IDisposable"/>.
        /// </summary>
        /// <param name="disposing">A <see cref="Boolean"/> indicating whether or not the instance is currently disposing.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_objectContext != null)
                {
                    _objectContext.Dispose();
                }
                if (_refreshContext != null)
                {
                    _refreshContext.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// This method is called to finalize changes after all the operations in the specified changeset
        /// have been invoked. All changes are committed to the ObjectContext, and any resulting optimistic
        /// concurrency errors are processed.
        /// </summary>
        /// <returns>True if the <see cref="ChangeSet"/> was persisted successfully, false otherwise.</returns>
        protected override bool PersistChangeSet()
        {
            return InvokeSaveChanges(true);
        }

        private bool InvokeSaveChanges(bool retryOnConflict)
        {
            try
            {
                ObjectContext.SaveChanges();
            }
            catch (OptimisticConcurrencyException ex)
            {
                // Map the operations that could have caused a conflict to an entity.
                Dictionary<ObjectStateEntry, ChangeSetEntry> operationConflictMap = new Dictionary<ObjectStateEntry, ChangeSetEntry>();
                foreach (ObjectStateEntry conflict in ex.StateEntries)
                {
                    ChangeSetEntry entry = ChangeSet.ChangeSetEntries.SingleOrDefault(p => Object.ReferenceEquals(p.Entity, conflict.Entity));
                    if (entry == null)
                    {
                        // If we're unable to find the object in our changeset, propagate
                        // the original exception
                        throw;
                    }
                    operationConflictMap.Add(conflict, entry);
                }

                SetChangeSetConflicts(operationConflictMap);

                // Call out to any user resolve code and resubmit if all conflicts
                // were resolved
                if (retryOnConflict && ResolveConflicts(ex.StateEntries))
                {
                    // clear the conflics from the entries
                    foreach (ChangeSetEntry entry in ChangeSet.ChangeSetEntries)
                    {
                        entry.StoreEntity = null;
                        entry.ConflictMembers = null;
                        entry.IsDeleteConflict = false;
                    }

                    // If all conflicts were resolved attempt a resubmit
                    return InvokeSaveChanges(retryOnConflict: false);
                }

                // if there was a conflict but no conflict information was
                // extracted to the individual entries, we need to ensure the
                // error makes it back to the client
                if (!ChangeSet.HasError)
                {
                    throw;
                }

                return false;
            }

            return true;
        }

        /// <summary>
        /// This method is called to finalize changes after all the operations in the specified changeset
        /// have been invoked. All changes are committed to the ObjectContext.
        /// <remarks>If the submit fails due to concurrency conflicts <see cref="ResolveConflicts"/> will be called.
        /// If <see cref="ResolveConflicts"/> returns true a single resubmit will be attempted.
        /// </remarks>
        /// </summary>
        /// <param name="conflicts">The list of concurrency conflicts that occurred</param>
        /// <returns>Returns <c>true</c> if the <see cref="ChangeSet"/> was persisted successfully, <c>false</c> otherwise.</returns>
        protected virtual bool ResolveConflicts(IEnumerable<ObjectStateEntry> conflicts)
        {
            return false;
        }

        /// <summary>
        /// Insert an entity into the <see cref="ObjectContext" />, ensuring its <see cref="EntityState" /> is <see cref="EntityState.Added" />
        /// </summary>
        /// <typeparam name="TEntity">The entity type</typeparam>
        /// <param name="entity">The entity to be inserted</param>
        protected virtual void InsertEntity<TEntity>(TEntity entity) where TEntity : class
        {
            ObjectStateEntry stateEntry;
            if (ObjectContext.ObjectStateManager.TryGetObjectStateEntry(entity, out stateEntry) &&
                stateEntry.State != EntityState.Added)
            {
                ObjectContext.ObjectStateManager.ChangeObjectState(entity, EntityState.Added);
            }
            else
            {
                ObjectContext.CreateObjectSet<TEntity>().AddObject(entity);
            }
        }

        /// <summary>
        /// Update an entity in the <see cref="ObjectContext" />, ensuring it is treated as a modified entity
        /// </summary>
        /// <typeparam name="TEntity">The entity type</typeparam>
        /// <param name="entity">The entity to be updated</param>
        protected virtual void UpdateEntity<TEntity>(TEntity entity) where TEntity : class
        {
            TEntity original = ChangeSet.GetOriginal(entity);
            ObjectSet<TEntity> objectSet = ObjectContext.CreateObjectSet<TEntity>();
            if (original == null)
            {
                objectSet.AttachAsModified(entity);
            }
            else
            {
                objectSet.AttachAsModified(entity, original);
            }
        }

        /// <summary>
        /// Delete an entity from the <see cref="ObjectContext" />, ensuring that its <see cref="EntityState" /> is <see cref="EntityState.Deleted" />
        /// </summary>
        /// <typeparam name="TEntity">The entity type</typeparam>
        /// <param name="entity">The entity to be deleted</param>
        protected virtual void DeleteEntity<TEntity>(TEntity entity) where TEntity : class
        {
            ObjectStateEntry stateEntry;
            if (ObjectContext.ObjectStateManager.TryGetObjectStateEntry(entity, out stateEntry) &&
                stateEntry.State != EntityState.Deleted)
            {
                ObjectContext.ObjectStateManager.ChangeObjectState(entity, EntityState.Deleted);
            }
            else
            {
                ObjectSet<TEntity> objectSet = ObjectContext.CreateObjectSet<TEntity>();
                objectSet.Attach(entity);
                objectSet.DeleteObject(entity);
            }
        }

        /// <summary>
        /// Updates each entry in the ChangeSet with its corresponding conflict info.
        /// </summary>
        /// <param name="operationConflictMap">Map of conflicts to their corresponding operations entries.</param>
        private void SetChangeSetConflicts(Dictionary<ObjectStateEntry, ChangeSetEntry> operationConflictMap)
        {
            object storeValue;
            EntityKey refreshEntityKey;

            foreach (var conflictEntry in operationConflictMap)
            {
                ObjectStateEntry stateEntry = conflictEntry.Key;

                if (stateEntry.State == EntityState.Unchanged)
                {
                    continue;
                }

                // Note: we cannot call Refresh StoreWins since this will overwrite Current entity and remove the optimistic concurrency ex.
                ChangeSetEntry operationInConflict = conflictEntry.Value;
                refreshEntityKey = RefreshContext.CreateEntityKey(stateEntry.EntitySet.Name, stateEntry.Entity);
                RefreshContext.TryGetObjectByKey(refreshEntityKey, out storeValue);
                operationInConflict.StoreEntity = storeValue;

                // StoreEntity will be null if the entity has been deleted in the store (i.e. Delete/Delete conflict)
                bool isDeleted = (operationInConflict.StoreEntity == null);
                if (isDeleted)
                {
                    operationInConflict.IsDeleteConflict = true;
                }
                else
                {
                    // Determine which members are in conflict by comparing original values to the current DB values
                    PropertyDescriptorCollection propDescriptors = TypeDescriptor.GetProperties(operationInConflict.Entity.GetType());
                    List<string> membersInConflict = new List<string>();
                    object originalValue;
                    PropertyDescriptor pd;
                    for (int i = 0; i < stateEntry.OriginalValues.FieldCount; i++)
                    {
                        originalValue = stateEntry.OriginalValues.GetValue(i);
                        if (originalValue is DBNull)
                        {
                            originalValue = null;
                        }

                        string propertyName = stateEntry.OriginalValues.GetName(i);
                        pd = propDescriptors[propertyName];
                        if (pd == null)
                        {
                            // This might happen in the case of a private model
                            // member that isn't mapped
                            continue;
                        }

                        if (!Object.Equals(originalValue, pd.GetValue(operationInConflict.StoreEntity)))
                        {
                            membersInConflict.Add(pd.Name);
                        }
                    }
                    operationInConflict.ConflictMembers = membersInConflict;
                }
            }
        }
    }
}
