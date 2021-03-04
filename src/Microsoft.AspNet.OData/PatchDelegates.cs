using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.AspNet.OData
{
    /// <summary>
    /// 
    /// </summary>
    public class PatchDelegates
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="keyValues"></param>
        /// <returns></returns>
        public delegate object GetOrCreateDelegate(IDictionary<string, object> keyValues);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="original"></param>
        public delegate void DeleteDelegate(object original);


    }
}
