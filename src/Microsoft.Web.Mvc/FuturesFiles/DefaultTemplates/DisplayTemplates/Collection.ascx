<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>
<%
    if (Model != null) {
        string oldPrefix = ViewData.TemplateInfo.HtmlFieldPrefix;
        int index = 0;

        ViewData.TemplateInfo.HtmlFieldPrefix = String.Empty;
        
        foreach (object item in (IEnumerable)Model) {
            string fieldName = String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}[{1}]", oldPrefix, index++);
            ViewContext.Writer.Write(Html.DisplayFor(m => item, null, fieldName));
        }
        
        ViewData.TemplateInfo.HtmlFieldPrefix = oldPrefix;
    }
%>