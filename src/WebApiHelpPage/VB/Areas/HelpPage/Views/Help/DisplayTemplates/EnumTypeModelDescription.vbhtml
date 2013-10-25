@Imports ROOT_PROJECT_NAMESPACE.Areas.HelpPage.ModelDescriptions
@ModelType EnumTypeModelDescription

<div>
    @If Model.Documentation IsNot Nothing Then
        @<p>@Model.Documentation</p>
    End If
    <p>Possible enumeration values:</p>
    <ul>
        @For Each value As EnumValueDescription in Model.Values
            @<li>
                @value.Name: @value.Value
                <p>@value.Documentation</p>
            </li>
        Next
    </ul>
</div>