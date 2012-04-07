// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Objects;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Controllers;
using Microsoft.Web.Http.Data.EntityFramework.Metadata;

namespace Microsoft.Web.Http.Data.EntityFramework
{
    [DbMetadataProvider]
    public abstract class DbDataController<TContext> : DataController
        where TContext : DbContext, new()
    {
        private TContext _dbContext;
        private ObjectContext _refreshContext;

        /// <summary>
        /// Protected constructor for the abstract class.
        /// </summary>
        protected DbDataController()
        {
        }

        /// <summary>
        /// Gets the <see cref="ObjectContext"/> used for retrieving store values
        /// </summary>
        private ObjectContext RefreshContext
        {
            get
            {
                if (_refreshContext == null)
                {
                    DbContext dbContext = CreateDbContext();
                    _refreshContext = (dbContext as IObjectContextAdapter).ObjectContext;
                }
                return _refreshContext;
            }
        }

        /// <summary>
        /// Gets the <see cref="DbContext"/>
        /// </summary>
        protected TContext DbContext
        {
            get
            {
                if (_dbContext == null)
                {
                    _dbContext = CreateDbContext();
                }
                return _dbContext;
            }
        }

        /// <summary>
        /// Initializes the <see cref="DbDataController{T}"/>.
        /// </summary>
        /// <param name="controllerContext">The <see cref="HttpControllerContext"/> for this <see cref="DataController"/>
        /// instance. Overrides must call the base method.</param>
        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);

            ObjectContext objectContext = ((IObjectContextAdapter)DbContext).ObjectContext;
            // We turn this off, since our deserializer isn't going to create
            // the EF proxy types anyways. Proxies only really work if the entities
            // are queried on the server.
            objectContext.ContextOptions.ProxyCreationEnabled = false;

            // Turn off DbContext validation.
            DbContext.Configuration.ValidateOnSaveEnabled = false;

            // Turn off AutoDetectChanges.
            DbContext.Configuration.AutoDetectChangesEnabled = false;

            DbContext.Configuration.LazyLoadingEnabled = false;
        }

        /// <summary>
        /// Returns the DbContext object.
        /// </summary>
        /// <returns>The created DbContext object.</returns>
        protected virtual TContext CreateDbContext()
        {
            return new TContext();
        }

        /// <summary>
        /// This method is called to finalize changes after all the operations in the specified changeset
        /// have been invoked. All changes are committed to the DbContext, and any resulting optimistic
        /// concurrency errors are processed.
        /// </summary>
        /// <returns><c>True</c> if the <see cref="ChangeSet"/> was persisted successfully, <c>false</c> otherwise.</returns>
        protected override bool PersistChangeSet()
        {
            return InvokeSaveChanges(true);
        }

        /// <summary>
        /// This method is called to finalize changes after all the operations in the specified changeset
        /// have been invoked. All changes are committed to the DbContext.
        /// <remarks>If the submit fails due to concurrency conflicts <see cref="ResolveConflicts"/> will be called.
        /// If <see cref="ResolveConflicts"/> returns true a single resubmit will be attempted.
        /// </remarks>
        /// </summary>
        /// <param name="conflicts">The list of concurrency conflicts that occurred</param>
        /// <returns>Returns <c>true</c> if the <see cref="ChangeSet"/> was persisted successfully, <c>false</c> otherwise.</returns>
        protected virtual bool ResolveConflicts(IEnumerable<DbEntityEntry> conflicts)
        {
            return false;
        }

        /// <summary>
        /// See <see cref="IDisposable"/>.
        /// </summary>
        /// <param name="disposing">A <see cref="Boolean"/> indicating whether or not the instance is currently disposing.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (DbContext != null)
                {
                    DbContext.Dispose();
                }
                if (_refreshContext != null)
                {
                    _refreshContext.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Called by PersistChangeSet method to save the changes to the database.
        /// </summary>
        /// <param name="retryOnConflict">Flag indicating whether to retry after resolving conflicts.</param>
        /// <returns><c>true</c> if saved successfully and <c>false</c> otherwise.</returns>
        private bool InvokeSaveChanges(bool retryOnConflict)
        {
            try
            {
                DbContext.SaveChanges();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                // Map the operations that could have caused a conflict to an entity.
                Dictionary<DbEntityEntry, ChangeSetEntry> operationConflictMap = new Dictionary<DbEntityEntry, ChangeSetEntry>();
                foreach (DbEntityEntry conflict in ex.Entries)
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
                if (retryOnConflict && ResolveConflicts(ex.Entries))
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
        /// Updates each entry in the ChangeSet with its corresponding conflict info.
        /// </summary>
        /// <param name="operationConflictMap">Map of conflicts to their corresponding operations entries.</param>
        private void SetChangeSetConflicts(Dictionary<DbEntityEntry, ChangeSetEntry> operationConflictMap)
        {
            object storeValue;
            EntityKey refreshEntityKey;

            ObjectContext objectContext = ((IObjectContextAdapter)DbContext).ObjectContext;
            ObjectStateManager objectStateManager = objectContext.ObjectStateManager;
            if (objectStateManager == null)
            {
                throw Error.InvalidOperation(Resource.ObjectStateManagerNotFoundException, DbContext.GetType().Name);
            }

            foreach (var conflictEntry in operationConflictMap)
            {
                DbEntityEntry entityEntry = conflictEntry.Key;
                ObjectStateEntry stateEntry = objectStateManager.GetObjectStateEntry(entityEntry.Entity);

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

        /// <summary>
        /// Insert an entity into the <see cref="DbContext" />, ensuring its <see cref="EntityState" /> is <see cref="EntityState.Added" />
        /// </summary>
        /// <param name="entity">The entity to be inserted</param>
        protected virtual void InsertEntity(object entity)
        {
            DbEntityEntry dbEntityEntry = DbContext.Entry(entity);
            if (dbEntityEntry.State != EntityState.Detached)
            {
                dbEntityEntry.State = EntityState.Added;
            }
            else
            {
                DbContext.Set(entity.GetType()).Add(entity);
            }
        }

        /// <summary>
        /// Update an entity in the <see cref="DbContext" />, ensuring it is treated as a modified entity
        /// </summary>
        /// <param name="entity">The entity to be updated</param>
        protected virtual void UpdateEntity(object entity)
        {
            object original = ChangeSet.GetOriginal(entity);
            DbSet dbSet = DbContext.Set(entity.GetType());
            if (original == null)
            {
                dbSet.AttachAsModified(entity, DbContext);
            }
            else
            {
                dbSet.AttachAsModified(entity, original, DbContext);
            }
        }

        /// <summary>
        /// Delete an entity from the <see cref="DbContext" />, ensuring that its <see cref="EntityState" /> is <see cref="EntityState.Deleted" />
        /// </summary>
        /// <param name="entity">The entity to be deleted</param>
        protected virtual void DeleteEntity(object entity)
        {
            DbEntityEntry entityEntry = DbContext.Entry(entity);
            if (entityEntry.State != EntityState.Deleted)
            {
                entityEntry.State = EntityState.Deleted;
            }
            else
            {
                DbContext.Set(entity.GetType()).Attach(entity);
                DbContext.Set(entity.GetType()).Remove(entity);
            }
        }
    }
}
