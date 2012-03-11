using System.Linq.Expressions;

namespace System.Web.Mvc.ExpressionUtil.Test
{
    // Represents an ExpressionFingerprint that is of the wrong type.
    internal sealed class DummyExpressionFingerprint : ExpressionFingerprint
    {
        public DummyExpressionFingerprint(ExpressionType nodeType, Type type)
            : base(nodeType, type)
        {
        }
    }
}
