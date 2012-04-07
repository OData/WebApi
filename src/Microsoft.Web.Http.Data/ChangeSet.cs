// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace Microsoft.Web.Http.Data
{
    /// <summary>
    /// Represents a set of changes to be processed by a <see cref="DataController"/>.
    /// </summary>
    public sealed class ChangeSet
    {
        private IEnumerable<ChangeSetEntry> _changeSetEntries;

        /// <summary>
        /// Initializes a new instance of the ChangeSet class
        /// </summary>
        /// <param name="changeSetEntries">The set of <see cref="ChangeSetEntry"/> items this <see cref="ChangeSet"/> represents.</param>
        /// <exception cref="ArgumentNullException">if <paramref name="changeSetEntries"/> is null.</exception>
        public ChangeSet(IEnumerable<ChangeSetEntry> changeSetEntries)
        {
            if (changeSetEntries == null)
            {
                throw Error.ArgumentNull("changeSetEntries");
            }

            // ensure the changeset is valid
            ValidateChangeSetEntries(changeSetEntries);

            _changeSetEntries = changeSetEntries;
        }

        /// <summary>
        /// Gets the set of <see cref="ChangeSetEntry"/> items this <see cref="ChangeSet"/> represents.
        /// </summary>
        public ReadOnlyCollection<ChangeSetEntry> ChangeSetEntries
        {
            get { return _changeSetEntries.ToList().AsReadOnly(); }
        }

        /// <summary>
        /// Gets a value indicating whether any of the <see cref="ChangeSetEntry"/> items has an error.
        /// </summary>
        public bool HasError
        {
            get { return _changeSetEntries.Any(op => op.HasConflict || (op.ValidationErrors != null && op.ValidationErrors.Any())); }
        }

        /// <summary>
        /// Returns the original unmodified entity for the provided <paramref name="clientEntity"/>.
        /// </summary>
        /// <remarks>
        /// Note that only members marked with <see cref="RoundtripOriginalAttribute"/> will be set
        /// in the returned instance.
        /// </remarks>
        /// <typeparam name="TEntity">The entity type.</typeparam>
        /// <param name="clientEntity">The client modified entity.</param>
        /// <returns>The original unmodified entity for the provided <paramref name="clientEntity"/>.</returns>
        /// <exception cref="ArgumentNullException">if <paramref name="clientEntity"/> is null.</exception>
        /// <exception cref="ArgumentException">if <paramref name="clientEntity"/> is not in the change set.</exception>
        public TEntity GetOriginal<TEntity>(TEntity clientEntity) where TEntity : class
        {
            if (clientEntity == null)
            {
                throw Error.ArgumentNull("clientEntity");
            }

            ChangeSetEntry entry = _changeSetEntries.FirstOrDefault(p => Object.ReferenceEquals(p.Entity, clientEntity));
            if (entry == null)
            {
                throw Error.Argument(Resource.ChangeSet_ChangeSetEntryNotFound);
            }

            if (entry.Operation == ChangeOperation.Insert)
            {
                throw Error.InvalidOperation(Resource.ChangeSet_OriginalNotValidForInsert);
            }

            return (TEntity)entry.OriginalEntity;
        }

        /// <summary>
        /// Validates that the specified entries are well formed.
        /// </summary>
        /// <param name="changeSetEntries">The changeset entries to validate.</param>
        private static void ValidateChangeSetEntries(IEnumerable<ChangeSetEntry> changeSetEntries)
        {
            HashSet<int> idSet = new HashSet<int>();
            HashSet<object> entitySet = new HashSet<object>();
            foreach (ChangeSetEntry entry in changeSetEntries)
            {
                // ensure Entity is not null
                if (entry.Entity == null)
                {
                    throw Error.InvalidOperation(Resource.InvalidChangeSet, Resource.InvalidChangeSet_NullEntity);
                }

                // ensure unique client IDs
                if (idSet.Contains(entry.Id))
                {
                    throw Error.InvalidOperation(Resource.InvalidChangeSet, Resource.InvalidChangeSet_DuplicateId);
                }
                idSet.Add(entry.Id);

                // ensure unique entity instances - there can only be a single entry
                // for a given entity instance
                if (entitySet.Contains(entry.Entity))
                {
                    throw Error.InvalidOperation(Resource.InvalidChangeSet, Resource.InvalidChangeSet_DuplicateEntity);
                }
                entitySet.Add(entry.Entity);

                // entities must be of the same type
                if (entry.OriginalEntity != null && !(entry.Entity.GetType() == entry.OriginalEntity.GetType()))
                {
                    throw Error.InvalidOperation(Resource.InvalidChangeSet, Resource.InvalidChangeSet_MustBeSameType);
                }

                if (entry.Operation == ChangeOperation.Insert && entry.OriginalEntity != null)
                {
                    throw Error.InvalidOperation(Resource.InvalidChangeSet, Resource.InvalidChangeSet_InsertsCantHaveOriginal);
                }
            }

            // now that we have the full Id space, we can validate associations
            foreach (ChangeSetEntry entry in changeSetEntries)
            {
                if (entry.Associations != null)
                {
                    ValidateAssociationMap(entry.Entity.GetType(), idSet, entry.Associations);
                }

                if (entry.OriginalAssociations != null)
                {
                    ValidateAssociationMap(entry.Entity.GetType(), idSet, entry.OriginalAssociations);
                }
            }
        }

        /// <summary>
        /// Validates the specified association map.
        /// </summary>
        /// <param name="entityType">The entity type the association is on.</param>
        /// <param name="idSet">The set of all unique Ids in the changeset.</param>
        /// <param name="associationMap">The association map to validate.</param>
        private static void ValidateAssociationMap(Type entityType, HashSet<int> idSet, IDictionary<string, int[]> associationMap)
        {
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(entityType);

            foreach (var associationItem in associationMap)
            {
                // ensure that the member is an association member
                string associationMemberName = associationItem.Key;
                PropertyDescriptor associationMember = properties[associationMemberName];
                if (associationMember == null || associationMember.Attributes[typeof(AssociationAttribute)] == null)
                {
                    throw Error.InvalidOperation(Resource.InvalidChangeSet,
                                                 String.Format(CultureInfo.CurrentCulture, Resource.InvalidChangeSet_InvalidAssociationMember, entityType, associationMemberName));
                }

                // ensure that the id collection is not null
                if (associationItem.Value == null)
                {
                    throw Error.InvalidOperation(Resource.InvalidChangeSet,
                                                 String.Format(CultureInfo.CurrentCulture, Resource.InvalidChangeSet_AssociatedIdsCannotBeNull, entityType, associationMemberName));
                }
                // ensure that each Id specified is in the changeset
                foreach (int id in associationItem.Value)
                {
                    if (!idSet.Contains(id))
                    {
                        throw Error.InvalidOperation(Resource.InvalidChangeSet,
                                                     String.Format(CultureInfo.CurrentCulture, Resource.InvalidChangeSet_AssociatedIdNotInChangeset, id, entityType, associationMemberName));
                    }
                }
            }
        }

        /// <summary>
        /// Reestablish associations based on Id lists by adding the referenced entities
        /// to their association members
        /// </summary>
        internal void SetEntityAssociations()
        {
            // create a unique map from Id to entity instances, and update operations
            // so Ids map to the same instances, since during deserialization reference
            // identity is not maintained.
            var entityIdMap = _changeSetEntries.ToDictionary(p => p.Id, p => new { Entity = p.Entity, OriginalEntity = p.OriginalEntity });
            foreach (ChangeSetEntry changeSetEntry in _changeSetEntries)
            {
                object entity = entityIdMap[changeSetEntry.Id].Entity;
                if (changeSetEntry.Entity != entity)
                {
                    changeSetEntry.Entity = entity;
                }

                object original = entityIdMap[changeSetEntry.Id].OriginalEntity;
                if (original != null && changeSetEntry.OriginalEntity != original)
                {
                    changeSetEntry.OriginalEntity = original;
                }
            }

            // for all entities with associations, reestablish the associations by mapping the Ids
            // to entity instances and adding them to the association members
            HashSet<int> visited = new HashSet<int>();
            foreach (var entityGroup in _changeSetEntries.Where(p => (p.Associations != null && p.Associations.Count > 0) || (p.OriginalAssociations != null && p.OriginalAssociations.Count > 0)).GroupBy(p => p.Entity.GetType()))
            {
                Dictionary<string, PropertyDescriptor> associationMemberMap = TypeDescriptor.GetProperties(entityGroup.Key).Cast<PropertyDescriptor>().Where(p => p.Attributes[typeof(AssociationAttribute)] != null).ToDictionary(p => p.Name);
                foreach (ChangeSetEntry changeSetEntry in entityGroup)
                {
                    if (visited.Contains(changeSetEntry.Id))
                    {
                        continue;
                    }
                    visited.Add(changeSetEntry.Id);

                    // set current associations
                    if (changeSetEntry.Associations != null)
                    {
                        foreach (var associationItem in changeSetEntry.Associations)
                        {
                            PropertyDescriptor assocMember = associationMemberMap[associationItem.Key];
                            IEnumerable<object> children = associationItem.Value.Select(p => entityIdMap[p].Entity);
                            SetAssociationMember(changeSetEntry.Entity, assocMember, children);
                        }
                    }
                }
            }
        }

        internal bool Validate(HttpActionContext actionContext)
        {
            // Validate all entries except those with type None or Delete (since we don't want to validate
            // entites we're going to delete).
            bool success = true;
            IEnumerable<ChangeSetEntry> entriesToValidate = ChangeSetEntries.Where(
                p => (p.ActionDescriptor != null && p.Operation != ChangeOperation.None && p.Operation != ChangeOperation.Delete)
                     || (p.EntityActions != null && p.EntityActions.Any()));

            foreach (ChangeSetEntry entry in entriesToValidate)
            {
                // TODO: optimize by determining whether a type actually requires any validation?
                // TODO: support for method level / parameter validation?

                List<ValidationResultInfo> validationErrors = new List<ValidationResultInfo>();
                if (!DataControllerValidation.ValidateObject(entry.Entity, validationErrors, actionContext))
                {
                    entry.ValidationErrors = validationErrors.Distinct(EqualityComparer<ValidationResultInfo>.Default).ToList();
                    success = false;
                }

                // clear after each validate call, since we've already
                // copied over the errors
                actionContext.ModelState.Clear();
            }

            return success;
        }

        /// <summary>
        /// Adds the specified associated entities to the specified association member for the specified entity.
        /// </summary>
        /// <param name="entity">The entity</param>
        /// <param name="associationProperty">The association member (singleton or collection)</param>
        /// <param name="associatedEntities">Collection of associated entities</param>
        private static void SetAssociationMember(object entity, PropertyDescriptor associationProperty, IEnumerable<object> associatedEntities)
        {
            if (associatedEntities.Count() == 0)
            {
                return;
            }

            object associationValue = associationProperty.GetValue(entity);
            if (typeof(IEnumerable).IsAssignableFrom(associationProperty.PropertyType))
            {
                if (associationValue == null)
                {
                    throw Error.InvalidOperation(Resource.DataController_AssociationCollectionPropertyIsNull, associationProperty.ComponentType.Name, associationProperty.Name);
                }

                IList list = associationValue as IList;
                IEnumerable<object> associationSequence = null;
                MethodInfo addMethod = null;
                if (list == null)
                {
                    // not an IList, so we have to use reflection
                    Type associatedEntityType = TypeUtility.GetElementType(associationValue.GetType());
                    addMethod = associationValue.GetType().GetMethod("Add", BindingFlags.Public | BindingFlags.Instance, null, new Type[] { associatedEntityType }, null);
                    if (addMethod == null)
                    {
                        throw Error.InvalidOperation(Resource.DataController_InvalidCollectionMember, associationProperty.Name);
                    }
                    associationSequence = ((IEnumerable)associationValue).Cast<object>();
                }

                foreach (object associatedEntity in associatedEntities)
                {
                    // add the entity to the collection if it's not already there
                    if (list != null)
                    {
                        if (!list.Contains(associatedEntity))
                        {
                            list.Add(associatedEntity);
                        }
                    }
                    else
                    {
                        if (!associationSequence.Contains(associatedEntity))
                        {
                            addMethod.Invoke(associationValue, new object[] { associatedEntity });
                        }
                    }
                }
            }
            else
            {
                // set the reference if it's not already set
                object associatedEntity = associatedEntities.Single();
                object currentValue = associationProperty.GetValue(entity);
                if (!Object.Equals(currentValue, associatedEntity))
                {
                    associationProperty.SetValue(entity, associatedEntity);
                }
            }
        }
    }
}
