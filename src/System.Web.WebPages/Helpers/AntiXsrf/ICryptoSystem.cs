namespace System.Web.Helpers.AntiXsrf
{
    // Provides an abstraction around the cryptographic subsystem for the anti-XSRF helpers.
    internal interface ICryptoSystem
    {
        string Protect(byte[] data);
        byte[] Unprotect(string protectedData);
    }
}
