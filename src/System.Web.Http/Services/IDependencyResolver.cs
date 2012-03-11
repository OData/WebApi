using System.Collections.Generic;

namespace System.Web.Http.Services
{
    public interface IDependencyResolver
    {
        /// <summary>
        /// Try to get a service of the given type.
        /// </summary>
        /// <param name="serviceType">Type of service to request.</param>
        /// <returns>an instance of the service, or null if the service is not found</returns>
        object GetService(Type serviceType);

        /// <summary>
        /// Try to get a list of services of the given type.
        /// </summary>
        /// <param name="serviceType">Type of services to request.</param>
        /// <returns>an enumeration (possibly empty) of the service. 
        /// Return an empty enumeration is the service is not found (don't return null)</returns>
        IEnumerable<object> GetServices(Type serviceType);
    }
}
