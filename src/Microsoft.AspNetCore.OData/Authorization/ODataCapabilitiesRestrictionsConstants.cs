using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.AspNet.OData.Authorization
{
    /// <summary>
    /// 
    /// </summary>
    public static class ODataCapabilityRestrictionsConstants
    {
        /// <summary>
        /// 
        /// </summary>
        public static readonly string CapabilitiesNamespace = "Org.OData.Capabilities.V1";
        /// <summary>
        /// 
        /// </summary>
        public static readonly string ReadRestrictions = $"{CapabilitiesNamespace}.ReadRestrictions";
        /// <summary>
        /// 
        /// </summary>
        public static readonly string ReadByKeyRestrictions = $"{CapabilitiesNamespace}.ReadByKeyRestrictions";
        /// <summary>
        /// 
        /// </summary>
        public static readonly string InsertRestrictions = $"{CapabilitiesNamespace}.InsertRestrictions";
        /// <summary>
        /// 
        /// </summary>
        public static readonly string UpdateRestrictions = $"{CapabilitiesNamespace}.UpdateRestrictions";
        /// <summary>
        /// 
        /// </summary>
        public static readonly string DeleteRestrictions = $"{CapabilitiesNamespace}.DeleteRestrictions";
    }
}
