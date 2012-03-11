<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>
<script runat="server">
    private List<SelectListItem> TriStateValues {
        get {
            return new List<SelectListItem> {
                new SelectListItem { Text = "Not Set", Value = String.Empty, Selected = !Value.HasValue },
                new SelectListItem { Text = "True", Value = "true", Selected = Value.HasValue && Value.Value },
                new SelectListItem { Text = "False", Value = "false", Selected = Value.HasValue && !Value.Value },
            };
        }
    }
    private bool? Value {
        get {
            if (ViewData.Model == null) {
                return null;
            }
            return Convert.ToBoolean(ViewData.Model, System.Globalization.CultureInfo.InvariantCulture);
        }
    }
</script>
<% if (ViewData.ModelMetadata.IsNullableValueType) { %>
    <%= Html.DropDownList("", TriStateValues, new { @class = "list-box tri-state" }) %>
<% } else { %>
    <%= Html.CheckBox("", Value ?? false, new { @class = "check-box" }) %>
<% } %>