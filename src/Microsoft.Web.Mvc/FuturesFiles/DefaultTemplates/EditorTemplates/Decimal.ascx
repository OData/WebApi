<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>
<script runat="server">
    private object FormattedValue {
        get {
            if (ViewData.TemplateInfo.FormattedModelValue == ViewData.ModelMetadata.Model) {
                return String.Format(System.Globalization.CultureInfo.CurrentCulture, "{0:0.00}", ViewData.ModelMetadata.Model);
            }
            return ViewData.TemplateInfo.FormattedModelValue;
        }
    }
</script>
<%= Html.TextBox("", FormattedValue, new { @class = "text-box single-line" }) %>