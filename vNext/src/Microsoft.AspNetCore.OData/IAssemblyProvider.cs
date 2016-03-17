using System.Collections.Generic;
using System.Reflection;

namespace Microsoft.AspNetCore.OData
{
    public interface IAssemblyProvider
    {
        IEnumerable<Assembly> CandidateAssemblies { get;}
    }
}