<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>
<%= Html.Encode(ViewData.TemplateInfo.FormattedModelValue) %>