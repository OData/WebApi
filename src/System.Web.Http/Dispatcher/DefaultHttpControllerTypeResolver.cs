// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Web.Http.Controllers;

namespace System.Web.Http.Dispatcher
{
    /// <summary>
    /// Provides an implementation of <see cref="IHttpControllerTypeResolver"/> with no external dependencies.
    /// </summary>
    public class DefaultHttpControllerTypeResolver : IHttpControllerTypeResolver
    {
        private readonly Predicate<Type> _isControllerTypePredicate;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultHttpControllerTypeResolver"/> with a default
        /// filter for detecting controller types.
        /// </summary>
        public DefaultHttpControllerTypeResolver()
            : this(IsControllerType)
        {
        }

        /// <summary>
        /// Creates a new <see cref="DefaultHttpControllerTypeResolver"/> instance using a predicate to filter controller types.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        public DefaultHttpControllerTypeResolver(Predicate<Type> predicate)
        {
            if (predicate == null)
            {
                throw Error.ArgumentNull("predicate");
            }

            _isControllerTypePredicate = predicate;
        }

        protected Predicate<Type> IsControllerTypePredicate
        {
            get { return _isControllerTypePredicate; }
        }

        internal static bool IsControllerType(Type t)
        {
            Contract.Assert(t != null);
            return
                t != null &&
                t.IsClass &&
                t.IsVisible &&
                !t.IsAbstract &&
                typeof(IHttpController).IsAssignableFrom(t) &&
                HasValidControllerName(t);
        }

        /// <summary>
        /// Returns a list of controllers available for the application.
        /// </summary>
        /// <returns>An <see cref="ICollection{Type}"/> of controllers.</returns>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Catching all exceptions in this case is the right to do.")]
        public virtual ICollection<Type> GetControllerTypes(IAssembliesResolver assembliesResolver)
        {
            if (assembliesResolver == null)
            {
                throw Error.ArgumentNull("assembliesResolver");
            }

            List<Type> result = new List<Type>();

            // Go through all assemblies referenced by the application and search for types matching a predicate
            ICollection<Assembly> assemblies = assembliesResolver.GetAssemblies();
            foreach (Assembly assembly in assemblies)
            {
                Type[] exportedTypes = null;
                if (assembly == null || assembly.IsDynamic)
                {
                    // can't call GetExportedTypes on a dynamic assembly
                    continue;
                }

                try
                {
                    exportedTypes = assembly.GetExportedTypes();
                }
                catch (ReflectionTypeLoadException ex)
                {
                    exportedTypes = ex.Types;
                }
                catch
                {
                    // We deliberately ignore all exceptions when building the cache. If 
                    // a controller type is not found then we will respond later with a 404.
                    // However, until then we don't know whether an exception at all will
                    // have an impact on finding a controller.
                    continue;
                }

                if (exportedTypes != null)
                {
                    result.AddRange(exportedTypes.Where(x => IsControllerTypePredicate(x)));
                }
            }

            return result;
        }

        /// <summary>
        /// We match if type name ends with "Controller" and that is not the only part of the 
        /// name (i.e it can't be just "Controller"). The reason is that the route name has to 
        /// be a non-empty prefix of the controller type name.
        /// </summary>
        internal static bool HasValidControllerName(Type controllerType)
        {
            Contract.Assert(controllerType != null);
            string controllerSuffix = DefaultHttpControllerSelector.ControllerSuffix;
            return controllerType.Name.Length > controllerSuffix.Length && controllerType.Name.EndsWith(controllerSuffix, StringComparison.OrdinalIgnoreCase);
        }
    }
}
