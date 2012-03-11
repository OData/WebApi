<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>
<%= Html.TextArea("", ViewData.TemplateInfo.FormattedModelValue.ToString(), 0, 0, new { @class = "text-box multi-line" }) %>