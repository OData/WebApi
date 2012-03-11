using System.Diagnostics;
using System.Web.Helpers;

namespace System.Web.Mvc
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class ValidateAntiForgeryTokenAttribute : FilterAttribute, IAuthorizationFilter
    {
        private string _salt;

        public ValidateAntiForgeryTokenAttribute()
            : this(AntiForgery.Validate)
        {
        }

        internal ValidateAntiForgeryTokenAttribute(Action<HttpContextBase, string> validateAction)
        {
            Debug.Assert(validateAction != null);
            ValidateAction = validateAction;
        }

        public string Salt
        {
            get { return _salt ?? String.Empty; }
            set { _salt = value; }
        }

        internal Action<HttpContextBase, string> ValidateAction { get; private set; }

        public void OnAuthorization(AuthorizationContext filterContext)
        {
            if (filterContext == null)
            {
                throw new ArgumentNullException("filterContext");
            }

            ValidateAction(filterContext.HttpContext, Salt);
        }
    }
}
