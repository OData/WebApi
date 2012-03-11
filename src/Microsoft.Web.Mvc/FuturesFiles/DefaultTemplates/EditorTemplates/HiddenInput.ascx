<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>
<script runat="server">
    private object ModelValue {
        get {
            if (Model is System.Data.Linq.Binary) {
                return Convert.ToBase64String(((System.Data.Linq.Binary)Model).ToArray());
            }
            if (Model is byte[]) {
                return Convert.ToBase64String((byte[])Model);
            }
            return Model;
        }
    }
</script>
<% if (!ViewData.ModelMetadata.HideSurroundingHtml) { %>
    <%= Html.Encode(ViewData.TemplateInfo.FormattedModelValue) %>
<% } %>
<%= Html.Hidden("", ModelValue) %>