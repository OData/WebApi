namespace Microsoft.TestCommon.Types
{
    /// <summary>
    /// Tagging interface to assist comparing instances of these types.
    /// </summary>
    public interface INameAndIdContainer
    {
        string Name { get; set; }

        int Id { get; set; }
    }
}
