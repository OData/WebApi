@Imports ROOT_PROJECT_NAMESPACE.Areas.HelpPage.ModelDescriptions
@ModelType ComplexTypeModelDescription
<div>
    @If Model.Documentation IsNot Nothing Then
        @<p>@Model.Documentation</p>
    End If
    @If Model.Properties.Count > 0 Then
        @<b>Parameters</b>
        @Html.DisplayFor(Function(m) Model.Properties, "PropertyDescriptions")
    End If
</div>