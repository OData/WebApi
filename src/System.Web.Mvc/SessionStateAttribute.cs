using System.Web.SessionState;

namespace System.Web.Mvc
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public sealed class SessionStateAttribute : Attribute
    {
        public SessionStateAttribute(SessionStateBehavior behavior)
        {
            Behavior = behavior;
        }

        public SessionStateBehavior Behavior { get; private set; }
    }
}
