// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Web.Http;

namespace Microsoft.Web.Http.Data.Metadata
{
    /// <summary>
    /// Attribute applied to a <see cref="DataController"/> type to specify the <see cref="MetadataProvider"/>
    /// for the type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class MetadataProviderAttribute : Attribute
    {
        private Type _providerType;

        /// <summary>
        /// Initializes a new instance of the MetadataProviderAttribute class
        /// </summary>
        /// <param name="providerType">The <see cref="MetadataProvider"/> type</param>
        public MetadataProviderAttribute(Type providerType)
        {
            if (providerType == null)
            {
                throw Error.ArgumentNull("providerType");
            }

            _providerType = providerType;
        }

        /// <summary>
        /// Gets the <see cref="MetadataProvider"/> type
        /// </summary>
        public Type ProviderType
        {
            get { return _providerType; }
        }

        /// <summary>
        /// Gets a unique identifier for this attribute.
        /// </summary>
        public override object TypeId
        {
            get { return this; }
        }

        /// <summary>
        /// This method creates an instance of the <see cref="MetadataProvider"/>. Subclasses can override this
        /// method to provide their own construction logic.
        /// </summary>
        /// <param name="controllerType">The <see cref="DataController"/> type to create a metadata provider for.</param>
        /// <param name="parent">The parent provider. May be null.</param>
        /// <returns>The metadata provider</returns>
        public virtual MetadataProvider CreateProvider(Type controllerType, MetadataProvider parent)
        {
            if (controllerType == null)
            {
                throw Error.ArgumentNull("controllerType");
            }

            if (!typeof(DataController).IsAssignableFrom(controllerType))
            {
                throw Error.Argument("controllerType", Resource.InvalidType, controllerType.FullName, typeof(DataController).FullName);
            }

            if (!typeof(MetadataProvider).IsAssignableFrom(_providerType))
            {
                throw Error.InvalidOperation(Resource.InvalidType, _providerType.FullName, typeof(MetadataProvider).FullName);
            }

            // Verify the type has a .ctor(MetadataProvider).
            if (_providerType.GetConstructor(new Type[] { typeof(MetadataProvider) }) == null)
            {
                throw Error.InvalidOperation(Resource.MetadataProviderAttribute_MissingConstructor, _providerType.FullName);
            }

            return (MetadataProvider)Activator.CreateInstance(_providerType, parent);
        }
    }
}
