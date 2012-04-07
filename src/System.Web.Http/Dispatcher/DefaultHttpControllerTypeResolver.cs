// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Web.Http.Internal;

namespace System.Web.Http.Dispatcher
{
    /// <summary>
    /// Provides an implementation of <see cref="IHttpControllerTypeResolver"/> with no external dependencies.
    /// </summary>
    public class DefaultHttpControllerTypeResolver : IHttpControllerTypeResolver
    {
        private readonly Predicate<Type> _isControllerTypePredicate;

        public DefaultHttpControllerTypeResolver() : this(IsControllerType)
        {
        }

        /// <summary>
        /// Creates a new <see cref="DefaultHttpControllerTypeResolver"/> instance using a predicate to filter controller types.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        public DefaultHttpControllerTypeResolver(Predicate<Type> predicate)
        {
            Contract.Assert(predicate != null);

            _isControllerTypePredicate = predicate;
        }

        protected Predicate<Type> IsControllerTypePredicate
        {
            get { return _isControllerTypePredicate; }
        }

        private static bool IsControllerType(Type t) 
        {
            return
                t != null &&
                t.IsClass &&
                t.IsPublic &&
                t.Name.EndsWith(DefaultHttpControllerSelector.ControllerSuffix, StringComparison.OrdinalIgnoreCase) &&
                !t.IsAbstract &&
                TypeHelper.HttpControllerType.IsAssignableFrom(t);
        }

        /// <summary>
        /// Returns a list of controllers available for the application.
        /// </summary>
        /// <returns>An <see cref="ICollection{Type}"/> of controllers.</returns>
        public virtual ICollection<Type> GetControllerTypes(IAssembliesResolver assembliesResolver)
        {
            // Go through all assemblies referenced by the application and search for types matching a predicate
            IEnumerable<Type> typesSoFar = Type.EmptyTypes;

            ICollection<Assembly> assemblies = assembliesResolver.GetAssemblies();
            foreach (Assembly assembly in assemblies)
            {
                if (assembly.IsDynamic)
                {
                    // can't call GetExportedTypes on a dynamic assembly
                    continue;
                }

                typesSoFar = typesSoFar.Concat(assembly.GetExportedTypes());
            }

            return typesSoFar.Where(x => IsControllerTypePredicate(x)).ToList();
        }
    }
}
