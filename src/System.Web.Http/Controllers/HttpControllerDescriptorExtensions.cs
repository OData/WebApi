// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.ComponentModel;
using System.Web.Http.Controllers;
using System.Web.Http.Dependencies;

namespace System.Web.Http
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class HttpControllerDescriptorExtensions
    {
        /// <summary>
        /// Helper during initialization to update the specific service to a given type. 
        /// This should only be used during initialization and before the controller is instantiated.
        /// This will check the dependency container for the type, else instantiate the type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="controllerDescriptor">controller descriptor service to act on </param>
        /// <param name="service">fallback instance to use if the type is not in the DI container</param>
        public static void ReplaceService<T>(this HttpControllerDescriptor controllerDescriptor, T service) where T : class
        {
            Type serviceType = typeof(T);
            ReplaceService(controllerDescriptor, serviceType, service);
        }

        /// <summary>
        /// Helper during initialization to update the specific service to a given type. 
        /// This should only be used during initialization and before the controller is instantiated.
        /// This will check the dependency container for the type, else instantiate the type.
        /// </summary>
        /// <param name="controllerDescriptor">controller descriptor service to act on </param>
        /// <param name="serviceType">service type cto check. </param>
        /// <param name="service">fallback instance to use if the type is not in the DI container</param>
        public static void ReplaceService(this HttpControllerDescriptor controllerDescriptor, Type serviceType, object service)
        {
            if (controllerDescriptor == null)
            {
                throw Error.ArgumentNull("controllerDescriptor");
            }
            if (serviceType == null)
            {
                throw Error.ArgumentNull("serviceType");
            }
            if (service == null)
            {
                throw Error.ArgumentNull("service");
            }

            // Lookup the derived type in the DI container. This is being called to specialize a service, so we 
            // look for the derived type instead of the general service type.
            Type serviceDerivedType = service.GetType();
            IDependencyResolver dr = controllerDescriptor.Configuration.DependencyResolver;
            object value = dr.GetService(serviceDerivedType) ?? service;

            controllerDescriptor.ControllerServices.Replace(serviceType, value);
        }
    }
}
