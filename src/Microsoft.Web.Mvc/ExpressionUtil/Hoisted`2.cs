using System.Collections.Generic;

namespace Microsoft.Web.Mvc.ExpressionUtil
{
    internal delegate TValue Hoisted<TModel, TValue>(TModel model, List<object> capturedConstants);
}
