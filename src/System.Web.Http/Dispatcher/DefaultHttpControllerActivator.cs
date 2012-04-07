// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Net.Http;
using System.Threading;
using System.Web.Http.Controllers;
using System.Web.Http.Internal;
using System.Web.Http.Properties;

namespace System.Web.Http.Dispatcher
{
    /// <summary>
    /// Default implementation of an <see cref="IHttpControllerActivator"/>.
    /// A different implementation can be registered via the <see cref="T:System.Web.Http.Services.DependencyResolver"/>.   
    /// We optimize for the case where we have an <see cref="Controllers.ApiControllerActionInvoker"/> 
    /// instance per <see cref="HttpControllerDescriptor"/> instance but can support cases where there are
    /// many <see cref="HttpControllerDescriptor"/> instances for one <see cref="System.Web.Http.Controllers.ApiControllerActionInvoker"/> 
    /// as well. In the latter case the lookup is slightly slower because it goes through the 
    /// <see cref="P:HttpControllerDescriptor.Properties"/> dictionary.
    /// </summary>
    public class DefaultHttpControllerActivator : IHttpControllerActivator
    {
        private Tuple<HttpControllerDescriptor, Func<IHttpController>> _fastCache;
        private object _cacheKey = new object();

        /// <summary>
        /// Creates the <see cref="IHttpController"/> specified by <paramref name="controllerType"/> using the given <paramref name="request"/>
        /// </summary>
        /// <param name="request">The request message.</param>
        /// <param name="controllerType">Type of the controller.</param>
        /// <param name="controllerDescriptor">The controller descriptor</param>
        /// <returns>An instance of type <paramref name="controllerType"/>.</returns>
        public IHttpController Create(HttpRequestMessage request, HttpControllerDescriptor controllerDescriptor, Type controllerType)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            if (controllerDescriptor == null)
            {
                throw Error.ArgumentNull("controllerDescriptor");
            }

            if (controllerType == null)
            {
                throw Error.ArgumentNull("controllerType");
            }

            try
            {
                // First check in the local fast cache and if not a match then look in the broader 
                // HttpControllerDescriptor.Properties cache
                if (_fastCache == null)
                {
                    // If dependency resolver returns controller object then keep asking it whenever we need a new instance
                    IHttpController instance = (IHttpController)request.GetDependencyScope().GetService(controllerType);
                    if (instance != null)
                    {
                        return instance;
                    }

                    // Otherwise create a delegate for creating a new instance of the type
                    Func<IHttpController> activator = TypeActivator.Create<IHttpController>(controllerType);
                    Tuple<HttpControllerDescriptor, Func<IHttpController>> cacheItem = Tuple.Create(controllerDescriptor, activator);
                    Interlocked.CompareExchange(ref _fastCache, cacheItem, null);

                    // Execute the delegate
                    return activator();
                }
                else if (_fastCache.Item1 == controllerDescriptor)
                {
                    // If the key matches and we already have the delegate for creating an instance then just execute it
                    return _fastCache.Item2();
                }
                else
                {
                    // If the key doesn't match then lookup/create delegate in the HttpControllerDescriptor.Properties for
                    // that HttpControllerDescriptor instance
                    Func<IHttpController> activator = (Func<IHttpController>)controllerDescriptor.Properties.GetOrAdd(
                        _cacheKey,
                        key => TypeActivator.Create<IHttpController>(controllerType));
                    return activator();
                }
            }
            catch (Exception ex)
            {
                throw Error.InvalidOperation(ex, SRResources.DefaultControllerFactory_ErrorCreatingController, controllerType.Name);
            }
        }
    }
}
