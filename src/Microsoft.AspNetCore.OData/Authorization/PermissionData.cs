using System.Collections.Generic;

namespace Microsoft.AspNet.OData.Authorization
{
    /// <summary>
    /// Represents permission restrictions extracted from an OData model.
    /// </summary>
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
