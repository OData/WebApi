// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Dynamic;
using System.Web.Http;

namespace System.Web.OData
{
    /// <summary>
    /// A class the tracks changes (i.e. the Delta) for an entity.
    /// </summary>
    [NonValidatingParameterBinding]
    public abstract class Delta : DynamicObject, IDelta 
    {
        /// <summary>
        /// Clears the Delta and resets the underlying Entity.
        /// </summary>
        public abstract void Clear();

        /// <summary>
        /// Attempts to set the Property called <paramref name="name"/> to the <paramref name="value"/> specified.
        /// <remarks>
        /// Only properties that exist on Entity can be set.
        /// If there is a type mismatch the request will fail.
        /// </remarks>
        /// </summary>
        /// <param name="name">The name of the Property</param>
        /// <param name="value">The new value of the Property</param>
        /// <returns>True if successful</returns>
        public abstract bool TrySetPropertyValue(string name, object value);

        /// <summary>
        /// Attempts to get the value of the Property called <paramref name="name"/> from the underlying Entity.
        /// <remarks>
        /// Only properties that exist on Entity can be retrieved.
        /// Both modified and unmodified properties can be retrieved.
        /// </remarks>
        /// </summary>
        /// <param name="name">The name of the Property</param>
        /// <param name="value">The value of the Property</param>
        /// <returns>True if the Property was found</returns>
        public abstract bool TryGetPropertyValue(string name, out object value);

        /// <summary>
        /// Attempts to get the <see cref="Type"/> of the Property called <paramref name="name"/> from the underlying Entity.
        /// <remarks>
        /// Only properties that exist on Entity can be retrieved.
        /// Both modified and unmodified properties can be retrieved.
        /// </remarks>
        /// </summary>
        /// <param name="name">The name of the Property</param>
        /// <param name="type">The type of the Property</param>
        /// <returns>Returns <c>true</c> if the Property was found and <c>false</c> if not.</returns>
        public abstract bool TryGetPropertyType(string name, out Type type);

        /// <summary>
        /// Overrides the DynamicObject TrySetMember method, so that only the properties
        /// of Entity can be set.
        /// </summary>
        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            if (binder == null)
            {
                throw Error.ArgumentNull("binder");
            }

            return TrySetPropertyValue(binder.Name, value);
        }

        /// <summary>
        /// Overrides the DynamicObject TryGetMember method, so that only the properties
        /// of Entity can be got.
        /// </summary>
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if (binder == null)
            {
                throw Error.ArgumentNull("binder");
            }

            return TryGetPropertyValue(binder.Name, out result);
        }

        /// <summary>
        /// Returns the Properties that have been modified through this Delta as an 
        /// enumeration of Property Names 
        /// </summary>
        public abstract IEnumerable<string> GetChangedPropertyNames();

        /// <summary>
        /// Returns the Properties that have not been modified through this Delta as an 
        /// enumeration of Property Names 
        /// </summary>
        public abstract IEnumerable<string> GetUnchangedPropertyNames();
    }
}