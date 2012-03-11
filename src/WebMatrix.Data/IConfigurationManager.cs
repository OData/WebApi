using System.Collections.Generic;

namespace WebMatrix.Data
{
    internal interface IConfigurationManager
    {
        IDictionary<string, string> AppSettings { get; }
        IConnectionConfiguration GetConnection(string name);
    }
}
