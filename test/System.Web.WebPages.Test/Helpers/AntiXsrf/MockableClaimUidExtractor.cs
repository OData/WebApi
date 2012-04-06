using System.Security.Principal;

namespace System.Web.Helpers.AntiXsrf.Test
{
    // An IClaimUidExtractor that can be passed to MoQ
    public abstract class MockableClaimUidExtractor : IClaimUidExtractor
    {
        public abstract object ExtractClaimUid(IIdentity identity);

        BinaryBlob IClaimUidExtractor.ExtractClaimUid(IIdentity identity)
        {
            return (BinaryBlob)ExtractClaimUid(identity);
        }
    }
}
