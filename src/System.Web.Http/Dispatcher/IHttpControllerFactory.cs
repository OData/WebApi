using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Web.Http.Controllers;

namespace System.Web.Http.Dispatcher
{
    /// <summary>
    /// Defines the methods that are required for an <see cref="IHttpController"/> factory.
    /// </summary>
    public interface IHttpControllerFactory
    {
        /// <summary>
        /// Creates the <see cref="IHttpController"/> using the specified context and controller name.
        /// </summary>
        /// <param name="controllerContext">The controller context.</param>
        /// <param name="controllerName">Name of the controller.</param>
        /// <returns>An <see cref="IHttpController"/> instance.</returns>
        IHttpController CreateController(HttpControllerContext controllerContext, string controllerName);

        /// <summary>
        /// Releases an <see cref="IHttpController"/> instance.
        /// </summary>
        /// <param name="controllerContext">The controller context.</param>
        /// <param name="controller">The controller.</param>
        void ReleaseController(HttpControllerContext controllerContext, IHttpController controller);

        /// <summary>
        /// Returns a map, keyed by controller string, of all <see cref="HttpControllerDescriptor"/> that the factory can produce. 
        /// This is primarily called by <see cref="System.Web.Http.Description.IApiExplorer"/> to discover all the possible controllers in the system.
        /// </summary>
        /// <returns>A map of all <see cref="HttpControllerDescriptor"/> that the factory can produce, or null if the factory does not have a well-defined mapping of <see cref="HttpControllerDescriptor"/>.</returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "This is better handled as a method.")]
        IDictionary<string, HttpControllerDescriptor> GetControllerMapping();
    }
}
