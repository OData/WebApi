// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Web.Http.Controllers;

namespace System.Web.Http.ExceptionHandling
{
    /// <summary>Creates exception services to call logging and handling from catch blocks.</summary>
    public static class ExceptionServices
    {
        private static object _lock = new object();

        /// <summary>Gets an exception logger that calls all registered logger services.</summary>
        /// <param name="configuration">The configuration.</param>
        /// <returns>A composite logger.</returns>
        public static IExceptionLogger GetLogger(HttpConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }

            ServicesContainer services = configuration.Services;
            Contract.Assert(services != null);
            return GetLogger(services);
        }

        internal static IExceptionLogger GetLogger(ServicesContainer services)
        {
            Contract.Assert(services != null);

            CompositeExceptionLogger cached = services.GetCompositeExceptionLogger();

            if (cached != null)
            {
                return cached;
            }

            lock (_lock)
            {
                CompositeExceptionLogger cachedAfterLock = services.GetCompositeExceptionLogger();

                if (cachedAfterLock != null)
                {
                    return cachedAfterLock;
                }

                IEnumerable<IExceptionLogger> loggers = services.GetExceptionLoggers();
                Contract.Assert(loggers != null);

                CompositeExceptionLogger composite = new CompositeExceptionLogger(loggers);
                services.Replace(typeof(CompositeExceptionLogger), composite);
                return composite;
            }
        }

        /// <summary>
        /// Gets an exception handler that calls the registered handler service, if any, and ensures exceptions do not
        /// accidentally propagate to the host.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <returns>
        /// An exception handler that calls any registered handler and ensures exceptions do not accidentally propagate
        /// to the host.
        /// </returns>
        public static IExceptionHandler GetHandler(HttpConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }

            ServicesContainer services = configuration.Services;
            Contract.Assert(services != null);
            return GetHandler(services);
        }

        internal static IExceptionHandler GetHandler(ServicesContainer services)
        {
            Contract.Assert(services != null);

            LastChanceExceptionHandler cached = services.GetLastChanceExceptionHandler();

            if (cached != null)
            {
                return cached;
            }

            lock (_lock)
            {
                LastChanceExceptionHandler cachedAfterLock = services.GetLastChanceExceptionHandler();

                if (cachedAfterLock != null)
                {
                    return cachedAfterLock;
                }

                IExceptionHandler innerHandler = services.GetExceptionHandler() ?? new EmptyExceptionHandler();
                LastChanceExceptionHandler lastChance = new LastChanceExceptionHandler(innerHandler);
                services.Replace(typeof(LastChanceExceptionHandler), lastChance);
                return lastChance;
            }
        }
    }
}
