// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Http;

namespace Microsoft.Web.Http.Data.Metadata
{
    /// <summary>
    /// Custom TypeDescriptionProvider conditionally registered for Types exposed by a <see cref="DataController"/>.
    /// </summary>
    internal class DataControllerTypeDescriptionProvider : TypeDescriptionProvider
    {
        private readonly MetadataProvider _metadataProvider;
        private readonly Type _type;
        private ICustomTypeDescriptor _customTypeDescriptor;

        public DataControllerTypeDescriptionProvider(Type type, MetadataProvider metadataProvider)
            : base(TypeDescriptor.GetProvider(type))
        {
            if (metadataProvider == null)
            {
                throw Error.ArgumentNull("metadataProvider");
            }

            _type = type;
            _metadataProvider = metadataProvider;
        }

        public override ICustomTypeDescriptor GetTypeDescriptor(Type objectType, object instance)
        {
            if (objectType == null && instance != null)
            {
                objectType = instance.GetType();
            }

            if (_type != objectType)
            {
                // In inheritance scenarios, we might be called to provide a descriptor
                // for a derived Type. In that case, we just return base.
                return base.GetTypeDescriptor(objectType, instance);
            }

            if (_customTypeDescriptor == null)
            {
                // CLR, buddy class type descriptors
                _customTypeDescriptor = base.GetTypeDescriptor(objectType, instance);

                // EF, any other custom type descriptors provided through MetadataProviders.
                _customTypeDescriptor = _metadataProvider.GetTypeDescriptor(objectType, _customTypeDescriptor);

                // initialize FK members AFTER our type descriptors have chained
                HashSet<string> foreignKeyMembers = GetForeignKeyMembers();

                // if any FK member of any association is also part of the primary key, then the key cannot be marked
                // Editable(false)
                bool keyIsEditable = false;
                foreach (PropertyDescriptor pd in _customTypeDescriptor.GetProperties())
                {
                    if (pd.Attributes[typeof(KeyAttribute)] != null &&
                        foreignKeyMembers.Contains(pd.Name))
                    {
                        keyIsEditable = true;
                        break;
                    }
                }

                if (DataControllerTypeDescriptor.ShouldRegister(_customTypeDescriptor, keyIsEditable, foreignKeyMembers))
                {
                    // Extend the chain with one more descriptor.
                    _customTypeDescriptor = new DataControllerTypeDescriptor(_customTypeDescriptor, keyIsEditable, foreignKeyMembers);
                }
            }

            return _customTypeDescriptor;
        }

        /// <summary>
        /// Returns the set of all foreign key members for the entity.
        /// </summary>
        /// <returns>The set of foreign keys.</returns>
        private HashSet<string> GetForeignKeyMembers()
        {
            HashSet<string> foreignKeyMembers = new HashSet<string>();
            foreach (PropertyDescriptor pd in _customTypeDescriptor.GetProperties())
            {
                AssociationAttribute assoc = (AssociationAttribute)pd.Attributes[typeof(AssociationAttribute)];
                if (assoc != null && assoc.IsForeignKey)
                {
                    foreach (string foreignKeyMember in assoc.ThisKeyMembers)
                    {
                        foreignKeyMembers.Add(foreignKeyMember);
                    }
                }
            }

            return foreignKeyMembers;
        }
    }
}
