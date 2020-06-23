using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.AspNet.OData.Authorization
{
    /// <summary>
    /// 
    /// </summary>
    public interface IODataAuthorizeData : IAuthorizeData
    {
        /// <summary>
        /// 
        /// </summary>
        IEnumerable<string> AllowedScopes { get; }

        /// <summary>
        /// 
        /// </summary>
        string Scheme { get; set; }
    }
}
