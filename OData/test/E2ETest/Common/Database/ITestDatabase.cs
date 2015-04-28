namespace WebStack.QA.Common.Database
{
    /// <summary>
    /// Base type for DB instance
    /// </summary>
    public interface ITestDatabase
    {
        string ConnectionString { get; set; }
        int? Timeout { get; set; }

        void Create();
        void Delete();
        bool Exist();
    }
}
