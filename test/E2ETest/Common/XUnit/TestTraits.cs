using Xunit;

namespace WebStack.QA.Common.XUnitTest
{
    public class BVTAttribute : TraitAttribute
    {
        public BVTAttribute()
            : base("Category", "BVT")
        {
        }
    }

    public class IDWAttribute : TraitAttribute
    {
        public IDWAttribute()
            : base("Category", "IDW")
        {
        }
    }

    /// <summary>
    /// SanityAttribute those tests simply verify if the test framework is working.
    /// 
    /// Very limited test cases should be marked, since they're not supposed to measure
    /// product's quality, but simply answer the question whether the test framework
    /// is working.
    /// </summary>
    public class SanityAttribute : TraitAttribute
    {
        public SanityAttribute()
            : base("Category", "Sanity")
        {
        }
    }
}