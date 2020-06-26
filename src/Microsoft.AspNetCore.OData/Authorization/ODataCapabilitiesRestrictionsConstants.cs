namespace Microsoft.AspNet.OData.Authorization
{
    internal static class ODataCapabilityRestrictionsConstants
    {
        public static readonly string CapabilitiesNamespace = "Org.OData.Capabilities.V1";
        public static readonly string ReadRestrictions = $"{CapabilitiesNamespace}.ReadRestrictions";
        public static readonly string ReadByKeyRestrictions = $"{CapabilitiesNamespace}.ReadByKeyRestrictions";
        public static readonly string InsertRestrictions = $"{CapabilitiesNamespace}.InsertRestrictions";
        public static readonly string UpdateRestrictions = $"{CapabilitiesNamespace}.UpdateRestrictions";
        public static readonly string DeleteRestrictions = $"{CapabilitiesNamespace}.DeleteRestrictions";
        public static readonly string OperationRestrictions = $"{CapabilitiesNamespace}.OperationRestrictions";
        public static readonly string NavigationRestrictions = $"{CapabilitiesNamespace}.NavigationRestrictions";
    }
}
