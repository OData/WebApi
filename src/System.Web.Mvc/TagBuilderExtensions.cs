using System.Diagnostics;

namespace System.Web.Mvc
{
    internal static class TagBuilderExtensions
    {
        internal static MvcHtmlString ToMvcHtmlString(this TagBuilder tagBuilder, TagRenderMode renderMode)
        {
            Debug.Assert(tagBuilder != null);
            return new MvcHtmlString(tagBuilder.ToString(renderMode));
        }
    }
}
