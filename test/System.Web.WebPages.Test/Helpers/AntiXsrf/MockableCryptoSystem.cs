namespace System.Web.Helpers.AntiXsrf.Test
{
    // An ICryptoSystem that can be passed to MoQ
    public abstract class MockableCryptoSystem : ICryptoSystem
    {
        public abstract string Protect(byte[] data);
        public abstract byte[] Unprotect(string protectedData);
    }
}
