using System.Security.Principal;

namespace System.Web.Helpers.AntiXsrf
{
    // Can extract unique identifers for a claims-based identity
    internal interface IClaimUidExtractor
    {
        BinaryBlob ExtractClaimUid(IIdentity identity);
    }
}
