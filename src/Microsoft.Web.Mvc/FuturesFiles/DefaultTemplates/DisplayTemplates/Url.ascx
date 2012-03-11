<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>
<a href="<%= Html.AttributeEncode(Model) %>"><%= Html.Encode(ViewData.TemplateInfo.FormattedModelValue) %></a>