// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Microsoft.Web.Http.Data.Metadata
{
    /// <summary>
    /// Custom TypeDescriptor for Types exposed by a <see cref="DataController"/>.
    /// </summary>
    internal class DataControllerTypeDescriptor : CustomTypeDescriptor
    {
        private readonly HashSet<string> _foreignKeyMembers;
        private readonly bool _keyIsEditable;
        private PropertyDescriptorCollection _properties;

        public DataControllerTypeDescriptor(ICustomTypeDescriptor parent, bool keyIsEditable, HashSet<string> foreignKeyMembers)
            : base(parent)
        {
            _keyIsEditable = keyIsEditable;
            _foreignKeyMembers = foreignKeyMembers;
        }

        public override PropertyDescriptorCollection GetProperties()
        {
            if (_properties == null)
            {
                // Get properties from our parent
                PropertyDescriptorCollection originalCollection = base.GetProperties();

                // Set _properties to avoid a stack overflow when CreateProjectionProperties 
                // ends up recursively calling TypeDescriptor.GetProperties on a type.
                _properties = originalCollection;

                bool customDescriptorsCreated = false;
                List<PropertyDescriptor> tempPropertyDescriptors = new List<PropertyDescriptor>();

                // for every property exposed by our parent, see if we have additional metadata to add,
                // and if we do we need to add a wrapper PropertyDescriptor to add the new attributes
                foreach (PropertyDescriptor propDescriptor in _properties)
                {
                    Attribute[] newMetadata = GetAdditionalAttributes(propDescriptor);
                    if (newMetadata.Length > 0)
                    {
                        tempPropertyDescriptors.Add(new DataControllerPropertyDescriptor(propDescriptor, newMetadata));
                        customDescriptorsCreated = true;
                    }
                    else
                    {
                        tempPropertyDescriptors.Add(propDescriptor);
                    }
                }

                if (customDescriptorsCreated)
                {
                    _properties = new PropertyDescriptorCollection(tempPropertyDescriptors.ToArray(), true);
                }
            }

            return _properties;
        }

        /// <summary>
        /// Return an array of new attributes for the specified PropertyDescriptor. If no
        /// attributes need to be added, return an empty array.
        /// </summary>
        /// <param name="pd">The property to add attributes for.</param>
        /// <returns>The collection of new attributes.</returns>
        private Attribute[] GetAdditionalAttributes(PropertyDescriptor pd)
        {
            List<Attribute> additionalAttributes = new List<Attribute>();

            if (ShouldAddRoundTripAttribute(pd, _foreignKeyMembers.Contains(pd.Name)))
            {
                additionalAttributes.Add(new RoundtripOriginalAttribute());
            }

            bool allowInitialValue;
            if (ShouldAddEditableFalseAttribute(pd, _keyIsEditable, out allowInitialValue))
            {
                additionalAttributes.Add(new EditableAttribute(false) { AllowInitialValue = allowInitialValue });
            }

            return additionalAttributes.ToArray();
        }

        /// <summary>
        /// Determines whether a type uses any features requiring the
        /// <see cref="DataControllerTypeDescriptor"/> to be registered. We do this
        /// check as an optimization so we're not adding additional TDPs to the
        /// chain when they're not necessary.
        /// </summary>
        /// <param name="descriptor">The descriptor for the type to check.</param>
        /// <param name="keyIsEditable">Indicates whether the key for this Type is editable.</param>
        /// <param name="foreignKeyMembers">The set of foreign key members for the Type.</param>
        /// <returns>Returns <c>true</c> if the type uses any features requiring the
        /// <see cref="DataControllerTypeDescriptionProvider"/> to be registered.</returns>
        internal static bool ShouldRegister(ICustomTypeDescriptor descriptor, bool keyIsEditable, HashSet<string> foreignKeyMembers)
        {
            foreach (PropertyDescriptor pd in descriptor.GetProperties())
            {
                // If there are any attributes that should be inferred for this member, then
                // we will register the descriptor
                if (ShouldInferAttributes(pd, keyIsEditable, foreignKeyMembers))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Returns true if the specified member requires a RoundTripOriginalAttribute
        /// and one isn't already present.
        /// </summary>
        /// <param name="pd">The member to check.</param>
        /// <param name="isFkMember">True if the member is a foreign key, false otherwise.</param>
        /// <returns>True if RoundTripOriginalAttribute should be added, false otherwise.</returns>
        private static bool ShouldAddRoundTripAttribute(PropertyDescriptor pd, bool isFkMember)
        {
            if (pd.Attributes[typeof(RoundtripOriginalAttribute)] != null || pd.Attributes[typeof(AssociationAttribute)] != null)
            {
                // already has the attribute or is an association 
                return false;
            }

            if (isFkMember || pd.Attributes[typeof(ConcurrencyCheckAttribute)] != null ||
                pd.Attributes[typeof(TimestampAttribute)] != null || pd.Attributes[typeof(KeyAttribute)] != null)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Returns <c>true</c> if the specified member requires an <see cref="EditableAttribute"/>
        /// to make the member read-only and one isn't already present.
        /// </summary>
        /// <param name="pd">The member to check.</param>
        /// <param name="keyIsEditable">Indicates whether the key for this Type is editable.</param>
        /// <param name="allowInitialValue">
        /// The default that should be used for <see cref="EditableAttribute.AllowInitialValue"/> if the attribute
        /// should be added to the member.
        /// </param>
        /// <returns><c>true</c> if <see cref="EditableAttribute"/> should be added, <c>false</c> otherwise.</returns>
        private static bool ShouldAddEditableFalseAttribute(PropertyDescriptor pd, bool keyIsEditable, out bool allowInitialValue)
        {
            allowInitialValue = false;

            if (pd.Attributes[typeof(EditableAttribute)] != null)
            {
                // already has the attribute
                return false;
            }

            bool hasKeyAttribute = (pd.Attributes[typeof(KeyAttribute)] != null);
            if (hasKeyAttribute && keyIsEditable)
            {
                return false;
            }

            if (hasKeyAttribute || pd.Attributes[typeof(TimestampAttribute)] != null)
            {
                // If we're inferring EditableAttribute because of a KeyAttribute
                // we want to allow initial value for the member.
                allowInitialValue = hasKeyAttribute;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Determines if there are any attributes that can be inferred for the specified member.
        /// </summary>
        /// <param name="pd">The member to check.</param>
        /// <param name="keyIsEditable">Indicates whether the key for this Type is editable.</param>
        /// <param name="foreignKeyMembers">Collection of foreign key members for the Type.</param>
        /// <returns><c>true</c> if there are attributes to be inferred, <c>false</c> otherwise.</returns>
        private static bool ShouldInferAttributes(PropertyDescriptor pd, bool keyIsEditable, IEnumerable<string> foreignKeyMembers)
        {
            bool allowInitialValue;

            return ShouldAddEditableFalseAttribute(pd, keyIsEditable, out allowInitialValue) ||
                   ShouldAddRoundTripAttribute(pd, foreignKeyMembers.Contains(pd.Name));
        }
    }

    /// <summary>
    /// PropertyDescriptor wrapper.
    /// </summary>
    internal class DataControllerPropertyDescriptor : PropertyDescriptor
    {
        private PropertyDescriptor _base;

        public DataControllerPropertyDescriptor(PropertyDescriptor pd, Attribute[] attribs)
            : base(pd, attribs)
        {
            _base = pd;
        }

        public override Type ComponentType
        {
            get { return _base.ComponentType; }
        }

        public override bool IsReadOnly
        {
            get { return _base.IsReadOnly; }
        }

        public override Type PropertyType
        {
            get { return _base.PropertyType; }
        }

        public override object GetValue(object component)
        {
            return _base.GetValue(component);
        }

        public override void SetValue(object component, object value)
        {
            _base.SetValue(component, value);
        }

        public override bool ShouldSerializeValue(object component)
        {
            return _base.ShouldSerializeValue(component);
        }

        public override bool CanResetValue(object component)
        {
            return _base.CanResetValue(component);
        }

        public override void ResetValue(object component)
        {
            _base.ResetValue(component);
        }
    }
}
