using System.Collections.Generic;

namespace System.Web.Mvc.ExpressionUtil
{
    internal delegate TValue Hoisted<TModel, TValue>(TModel model, List<object> capturedConstants);
}
