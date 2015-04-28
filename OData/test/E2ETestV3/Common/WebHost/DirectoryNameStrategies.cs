using System;

namespace WebStack.QA.Common.WebHost
{
    public interface IDirectoryNameStrategy
    {
        string GetName();
    }

    public class GuidDirectoryNameStrategy : IDirectoryNameStrategy
    {
        public string GetName()
        {
            return Guid.NewGuid().ToString("N");
        }
    }

    public class IncrementalDirectoryNameStrategy : IDirectoryNameStrategy
    {
        private int _counter;
        private string _prefix;

        public IncrementalDirectoryNameStrategy(string prefix)
        {
            _prefix = prefix;
            _counter = 0;
        }

        public string GetName()
        {
            return _prefix + "_" + _counter++;
        }
    }

    public class FixedDirectoryNameStrategy : IDirectoryNameStrategy
    {
        private string _name;

        public FixedDirectoryNameStrategy(string name)
        {
            _name = name;
        }

        public string GetName()
        {
            return _name;
        }
    }
}
