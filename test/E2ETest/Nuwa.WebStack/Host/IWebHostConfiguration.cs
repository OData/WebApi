namespace Nuwa.WebStack.Host
{
    /// <summary>
    /// Interface IWebHostConfiguration defines an actionable 
    /// structure for web host situation.
    /// </summary>
    public interface IWebHostConfiguration
    {
        void Initialize(WebHostContext context);
        void TearDown(WebHostContext context);
    }
}
