using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.AspNet.OData
{
    /// <summary>
    /// Class to handle delegates for Create and Delete
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1053:StaticHolderTypesShouldNotHaveConstructors")]
    public class PatchMethodHandler
    {
        /// <summary>
        /// GetOrCreate delegate to which the pointer to GetOrCreate Method can be assigned to
        /// </summary>
        /// <param name="keyValues">Key value pair for the entity keys</param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
        public delegate object GetOrCreate(IDictionary<string, object> keyValues);

        /// <summary>
        /// Delete delegate to which the pointer to Delete Method can be assigned to
        /// </summary>
        /// <param name="original">Original object to be deleted</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
        public delegate void Delete(object original);

    }
}
