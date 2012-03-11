<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>
<% if (!ViewData.ModelMetadata.HideSurroundingHtml) { %>
    <%= Html.Encode(ViewData.TemplateInfo.FormattedModelValue) %>
<% } %>