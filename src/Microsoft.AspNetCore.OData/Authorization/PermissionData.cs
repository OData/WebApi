using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.AspNet.OData.Authorization
{
    internal class PermissionData
    {
        public string SchemeName { get; set; }
        public IList<PermissionScopeData> Scopes { get; set; }
    }

    internal class PermissionScopeData
    {
        public string Scope { get; set; }
        public string RestrictedProperties { get; set; }
    }
}
