using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Http.Services;

namespace System.Web.Http.Tracing
{
    /// <summary>
    /// Interface to initialize the tracing layer.
    /// </summary>
    /// <remarks>
    /// This is an extensibility interface that may be inserted into the
    /// <see cref="DependencyResolver"/> to provide a replacement for the
    /// entire tracing layer.
    /// </remarks>
    public interface ITraceManager
    {
        void Initialize(HttpConfiguration configuration);
    }
}
