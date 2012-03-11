<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>
<%= Html.Password("", ViewData.TemplateInfo.FormattedModelValue, new { @class = "text-box single-line password" })%>