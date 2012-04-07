// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using System.Data.Metadata.Edm;
using System.Globalization;
using System.Linq;
using System.Web.Http;

namespace Microsoft.Web.Http.Data.EntityFramework.Metadata
{
    /// <summary>
    /// Metadata context for LINQ To Entities controllers
    /// </summary>
    internal class LinqToEntitiesTypeDescriptionContext : TypeDescriptionContextBase
    {
        private readonly Type _contextType;
        private readonly bool _isDbContext;
        private ConcurrentDictionary<string, AssociationInfo> _associationMap = new ConcurrentDictionary<string, AssociationInfo>();
        private MetadataWorkspace _metadataWorkspace;

        /// <summary>
        /// Constructor that accepts a LINQ To Entities context type
        /// </summary>
        /// <param name="contextType">The ObjectContext Type</param>
        /// <param name="isDbContext">Set to <c>true</c> if context is a database context.</param>
        public LinqToEntitiesTypeDescriptionContext(Type contextType, bool isDbContext)
        {
            if (contextType == null)
            {
                throw Error.ArgumentNull("contextType");
            }
            _contextType = contextType;
            _isDbContext = isDbContext;
        }

        /// <summary>
        /// Gets the MetadataWorkspace for the context
        /// </summary>
        public MetadataWorkspace MetadataWorkspace
        {
            get
            {
                if (_metadataWorkspace == null)
                {
                    // we only support embedded mappings
                    _metadataWorkspace = MetadataWorkspaceUtilities.CreateMetadataWorkspace(_contextType, _isDbContext);
                }
                return _metadataWorkspace;
            }
        }

        /// <summary>
        /// Returns the <see cref="StructuralType"/> that corresponds to the given CLR type
        /// </summary>
        /// <param name="clrType">The CLR type</param>
        /// <returns>The StructuralType that corresponds to the given CLR type</returns>
        public StructuralType GetEdmType(Type clrType)
        {
            return ObjectContextUtilities.GetEdmType(MetadataWorkspace, clrType);
        }

        /// <summary>
        /// Returns the association information for the specified navigation property.
        /// </summary>
        /// <param name="navigationProperty">The navigation property to return association information for</param>
        /// <returns>The association info</returns>
        internal AssociationInfo GetAssociationInfo(NavigationProperty navigationProperty)
        {
            return _associationMap.GetOrAdd(navigationProperty.RelationshipType.FullName, associationName =>
            {
                AssociationType associationType = (AssociationType)navigationProperty.RelationshipType;

                if (!associationType.ReferentialConstraints.Any())
                {
                    // We only support EF models where FK info is part of the model.
                    throw Error.NotSupported(Resource.LinqToEntitiesProvider_UnableToRetrieveAssociationInfo, associationName);
                }

                string toRoleName = associationType.ReferentialConstraints[0].ToRole.Name;
                AssociationInfo associationInfo = new AssociationInfo()
                {
                    FKRole = toRoleName,
                    Name = GetAssociationName(navigationProperty, toRoleName),
                    ThisKey = associationType.ReferentialConstraints[0].ToProperties.Select(p => p.Name).ToArray(),
                    OtherKey = associationType.ReferentialConstraints[0].FromProperties.Select(p => p.Name).ToArray(),
                    IsRequired = associationType.RelationshipEndMembers[0].RelationshipMultiplicity == RelationshipMultiplicity.One
                };

                return associationInfo;
            });
        }

        /// <summary>
        /// Creates an AssociationAttribute for the specified navigation property
        /// </summary>
        /// <param name="navigationProperty">The navigation property that corresponds to the association (it identifies the end points)</param>
        /// <returns>A new AssociationAttribute that describes the given navigation property association</returns>
        internal AssociationAttribute CreateAssociationAttribute(NavigationProperty navigationProperty)
        {
            AssociationInfo assocInfo = GetAssociationInfo(navigationProperty);
            bool isForeignKey = navigationProperty.FromEndMember.Name == assocInfo.FKRole;
            string thisKey;
            string otherKey;
            if (isForeignKey)
            {
                thisKey = String.Join(",", assocInfo.ThisKey);
                otherKey = String.Join(",", assocInfo.OtherKey);
            }
            else
            {
                otherKey = String.Join(",", assocInfo.ThisKey);
                thisKey = String.Join(",", assocInfo.OtherKey);
            }

            AssociationAttribute assocAttrib = new AssociationAttribute(assocInfo.Name, thisKey, otherKey);
            assocAttrib.IsForeignKey = isForeignKey;
            return assocAttrib;
        }

        /// <summary>
        /// Returns a unique association name for the specified navigation property.
        /// </summary>
        /// <param name="navigationProperty">The navigation property</param>
        /// <param name="foreignKeyRoleName">The foreign key role name for the property's association</param>
        /// <returns>A unique association name for the specified navigation property.</returns>
        private string GetAssociationName(NavigationProperty navigationProperty, string foreignKeyRoleName)
        {
            RelationshipEndMember fromMember = navigationProperty.FromEndMember;
            RelationshipEndMember toMember = navigationProperty.ToEndMember;

            RefType toRefType = toMember.TypeUsage.EdmType as RefType;
            EntityType toEntityType = toRefType.ElementType as EntityType;

            RefType fromRefType = fromMember.TypeUsage.EdmType as RefType;
            EntityType fromEntityType = fromRefType.ElementType as EntityType;

            bool isForeignKey = navigationProperty.FromEndMember.Name == foreignKeyRoleName;
            string fromTypeName = isForeignKey ? fromEntityType.Name : toEntityType.Name;
            string toTypeName = isForeignKey ? toEntityType.Name : fromEntityType.Name;

            // names are always formatted non-FK side type name followed by FK side type name
            string associationName = String.Format(CultureInfo.InvariantCulture, "{0}_{1}", toTypeName, fromTypeName);
            associationName = MakeUniqueName(associationName, _associationMap.Values.Select(p => p.Name));

            return associationName;
        }
    }
}
