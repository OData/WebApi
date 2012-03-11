<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>
<script runat="server">
    bool ShouldShow(ModelMetadata metadata) {
        return metadata.ShowForEdit
            && metadata.ModelType != typeof(System.Data.EntityState)
            && !metadata.IsComplexType
            && !ViewData.TemplateInfo.Visited(metadata);
    }
</script>
<% if (ViewData.TemplateInfo.TemplateDepth > 1) { %>
    <% if (Model == null) { %>
        <%= ViewData.ModelMetadata.NullDisplayText %>
    <% } else { %>
        <%= ViewData.ModelMetadata.SimpleDisplayText %>
    <% } %>
<% } else { %>    
    <% foreach (var prop in ViewData.ModelMetadata.Properties.Where(pm => ShouldShow(pm))) { %>
        <% if (prop.HideSurroundingHtml) { %>
            <%= Html.Editor(prop.PropertyName) %>
        <% } else { %>
            <% if (!String.IsNullOrEmpty(Html.Label(prop.PropertyName).ToHtmlString())) { %>
                <div class="editor-label"><%= Html.Label(prop.PropertyName) %></div>
            <% } %>
            <div class="editor-field"><%= Html.Editor(prop.PropertyName) %> <%= Html.ValidationMessage(prop.PropertyName, "*") %></div>
        <% } %>
    <% } %>
<% } %>