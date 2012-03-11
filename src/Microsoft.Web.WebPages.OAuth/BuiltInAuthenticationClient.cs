using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Web.WebPages.OAuth
{
    /// <summary>
    /// Represents built in OAuth clients.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "OAuth", Justification = "OAuth is a brand name.")]
    public enum BuiltInOAuthClient
    {
        Twitter,
        Facebook,
        LinkedIn,
        WindowsLive
    }
}
