// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.Metadata.Edm;
using System.Data.Objects;
using System.Linq;
using System.Web.Http;

namespace Microsoft.Web.Http.Data.EntityFramework
{
    /// <summary>
    /// Internal utility functions for dealing with EF types and metadata
    /// </summary>
    internal static class ObjectContextUtilities
    {
        /// <summary>
        /// Retrieves the <see cref="StructuralType"/> corresponding to the given CLR type (where the
        /// type is an entity or complex type).
        /// </summary>
        /// <remarks>
        /// If no mapping exists for <paramref name="clrType"/>, but one does exist for one of its base 
        /// types, we will return the mapping for the base type.
        /// </remarks>
        /// <param name="workspace">The <see cref="MetadataWorkspace"/></param>
        /// <param name="clrType">The CLR type</param>
        /// <returns>The <see cref="StructuralType"/> corresponding to that CLR type, or <c>null</c> if the Type
        /// is not mapped.</returns>
        public static StructuralType GetEdmType(MetadataWorkspace workspace, Type clrType)
        {
            if (workspace == null)
            {
                throw Error.ArgumentNull("workspace");
            }
            if (clrType == null)
            {
                throw Error.ArgumentNull("clrType");
            }

            if (clrType.IsPrimitive || clrType == typeof(object))
            {
                // want to avoid loading searching system assemblies for
                // types we know aren't entity or complex types
                return null;
            }

            // We first locate the EdmType in "OSpace", which matches the name and namespace of the CLR type
            EdmType edmType = null;
            do
            {
                if (!workspace.TryGetType(clrType.Name, clrType.Namespace, DataSpace.OSpace, out edmType))
                {
                    // If EF could not find this type, it could be because it is not loaded into
                    // its current workspace.  In this case, we explicitly load the assembly containing 
                    // the CLR type and try again.
                    workspace.LoadFromAssembly(clrType.Assembly);
                    workspace.TryGetType(clrType.Name, clrType.Namespace, DataSpace.OSpace, out edmType);
                }
            }
            while (edmType == null && (clrType = clrType.BaseType) != typeof(object) && clrType != null);

            // Next we locate the StructuralType from the EdmType.
            // This 2-step process is necessary when the types CLR namespace does not match Edm namespace.
            // Look at the EdmEntityTypeAttribute on the generated entity classes to see this Edm namespace.
            StructuralType structuralType = null;
            if (edmType != null &&
                (edmType.BuiltInTypeKind == BuiltInTypeKind.EntityType || edmType.BuiltInTypeKind == BuiltInTypeKind.ComplexType))
            {
                workspace.TryGetEdmSpaceType((StructuralType)edmType, out structuralType);
            }

            return structuralType;
        }

        /// <summary>
        /// Method used to return the current <see cref="EntityState"/> of the specified
        /// entity.
        /// </summary>
        /// <param name="context">The <see cref="ObjectContext"/></param>
        /// <param name="entity">The entity to return the <see cref="EntityState"/> for</param>
        /// <returns>The current <see cref="EntityState"/> of the specified entity</returns>
        public static EntityState GetEntityState(ObjectContext context, object entity)
        {
            if (context == null)
            {
                throw Error.ArgumentNull("context");
            }
            if (entity == null)
            {
                throw Error.ArgumentNull("entity");
            }

            ObjectStateEntry stateEntry = null;
            if (!context.ObjectStateManager.TryGetObjectStateEntry(entity, out stateEntry))
            {
                return EntityState.Detached;
            }
            return stateEntry.State;
        }

        /// <summary>
        /// Determines if the specified EdmMember is a concurrency timestamp.
        /// </summary>
        /// <remarks>Since EF doesn't expose "timestamp" as a first class
        /// concept, we use the below criteria to infer this for ourselves.
        /// </remarks>
        /// <param name="member">The member to check.</param>
        /// <returns>True or false.</returns>
        public static bool IsConcurrencyTimestamp(EdmMember member)
        {
            Facet facet = member.TypeUsage.Facets.SingleOrDefault(p => p.Name == "ConcurrencyMode");
            if (facet == null || facet.Value == null || (ConcurrencyMode)facet.Value != ConcurrencyMode.Fixed)
            {
                return false;
            }

            facet = member.TypeUsage.Facets.SingleOrDefault(p => p.Name == "FixedLength");
            if (facet == null || facet.Value == null || !((bool)facet.Value))
            {
                return false;
            }

            facet = member.TypeUsage.Facets.SingleOrDefault(p => p.Name == "MaxLength");
            if (facet == null || facet.Value == null || (int)facet.Value != 8)
            {
                return false;
            }

            MetadataProperty md = ObjectContextUtilities.GetStoreGeneratedPattern(member);
            if (md == null || facet.Value == null || (string)md.Value != "Computed")
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Gets the <see cref="StoreGeneratedPattern"/> property value from the edm member.
        /// </summary>
        /// <param name="member">The EdmMember from which to get the StoreGeneratedPattern value.</param>
        /// <returns>The <see cref="StoreGeneratedPattern"/> value.</returns>
        public static MetadataProperty GetStoreGeneratedPattern(EdmMember member)
        {
            MetadataProperty md;
            member.MetadataProperties.TryGetValue("http://schemas.microsoft.com/ado/2009/02/edm/annotation:StoreGeneratedPattern", ignoreCase: true, item: out md);
            return md;
        }

        public static ObjectStateEntry AttachAsModifiedInternal<TEntity>(TEntity current, TEntity original, ObjectContext objectContext)
        {
            ObjectStateEntry stateEntry = objectContext.ObjectStateManager.GetObjectStateEntry(current);
            stateEntry.ApplyOriginalValues(original);

            // For any members that don't have RoundtripOriginal applied, EF can't determine modification
            // state by doing value comparisons. To avoid losing updates in these cases, we must explicitly
            // mark such members as modified.
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(typeof(TEntity));
            AttributeCollection attributes = TypeDescriptor.GetAttributes(typeof(TEntity));
            bool isRoundtripType = attributes[typeof(RoundtripOriginalAttribute)] != null;
            foreach (var fieldMetadata in stateEntry.CurrentValues.DataRecordInfo.FieldMetadata)
            {
                string memberName = stateEntry.CurrentValues.GetName(fieldMetadata.Ordinal);
                PropertyDescriptor property = properties[memberName];
                // TODO: below we need to replace ExcludeAttribute logic with corresponding
                // DataContractMember/IgnoreDataMember logic
                if (property != null &&
                    (property.Attributes[typeof(RoundtripOriginalAttribute)] == null && !isRoundtripType)
                    /* && property.Attributes[typeof(ExcludeAttribute)] == null */)
                {
                    stateEntry.SetModifiedProperty(memberName);
                }
            }

            return stateEntry;
        }
    }
}
