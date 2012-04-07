// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using System.ComponentModel;
using System.Web.Http;

namespace Microsoft.Web.Http.Data.Metadata
{
    /// <summary>
    /// A <see cref="MetadataProvider"/> is used to provide the metadata description for
    /// types exposed by a <see cref="DataController"/>.
    /// </summary>
    public abstract class MetadataProvider
    {
        private MetadataProvider _parentProvider;
        private Func<Type, bool> _isEntityTypeFunc;

        /// <summary>
        /// Protected Constructor
        /// </summary>
        /// <param name="parent">The existing parent provider. May be null.</param>
        protected MetadataProvider(MetadataProvider parent)
        {
            _parentProvider = parent;
        }

        /// <summary>
        /// Gets the parent provider.
        /// </summary>
        internal MetadataProvider ParentProvider
        {
            get { return _parentProvider; }
        }

        /// <summary>
        /// Gets the <see cref="TypeDescriptor"/> for the specified Type, using the specified parent descriptor
        /// as the base. Overrides should call base to ensure the <see cref="TypeDescriptor"/>s are chained properly.
        /// </summary>
        /// <param name="type">The Type to return a descriptor for.</param>
        /// <param name="parent">The parent descriptor.</param>
        /// <returns>The <see cref="TypeDescriptor"/> for the specified Type.</returns>
        public virtual ICustomTypeDescriptor GetTypeDescriptor(Type type, ICustomTypeDescriptor parent)
        {
            if (type == null)
            {
                throw Error.ArgumentNull("type");
            }
            if (parent == null)
            {
                throw Error.ArgumentNull("parent");
            }

            if (_parentProvider != null)
            {
                return _parentProvider.GetTypeDescriptor(type, parent);
            }

            return parent;
        }

        /// <summary>
        /// Determines if the specified <see cref="Type"/> should be considered an entity <see cref="Type"/>.
        /// The base implementation returns <c>false</c>.
        /// </summary>
        /// <remarks>Effectively, the return from this method is this provider's vote as to whether the specified
        /// Type is an entity. The votes from this provider and all other providers in the chain are used
        /// by <see cref="IsEntityType"/> to make it's determination.</remarks>
        /// <param name="type">The <see cref="Type"/> to check.</param>
        /// <returns>Returns <c>true</c> if the <see cref="Type"/> should be considered an entity,
        /// <c>false</c> otherwise.</returns>
        public virtual bool LookUpIsEntityType(Type type)
        {
            if (type == null)
            {
                throw Error.ArgumentNull("type");
            }

            return false;
        }

        /// <summary>
        /// Determines if the specified <see cref="Type"/> is an entity <see cref="Type"/> by consulting
        /// the <see cref="LookUpIsEntityType"/> method of all <see cref="MetadataProvider"/>s
        /// in the provider chain for the <see cref="DataController"/>.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> to check.</param>
        /// <returns>Returns <c>true</c> if the <see cref="Type"/> is an entity, <c>false</c> otherwise.</returns>
        protected internal bool IsEntityType(Type type)
        {
            if (type == null)
            {
                throw Error.ArgumentNull("type");
            }

            if (_isEntityTypeFunc != null)
            {
                return _isEntityTypeFunc(type);
            }

            return false;
        }

        /// <summary>
        /// Sets the internal entity lookup function for this provider. The function consults
        /// the entire provider chain to make its determination.
        /// </summary>
        /// <param name="isEntityTypeFunc">The entity function.</param>
        internal void SetIsEntityTypeFunc(Func<Type, bool> isEntityTypeFunc)
        {
            _isEntityTypeFunc = isEntityTypeFunc;
        }
    }
}
