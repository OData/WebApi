@Imports System.Collections.ObjectModel
@Imports System.Web.Http.Description
@Imports System.Threading
@Imports ROOT_PROJECT_NAMESPACE.Areas.HelpPage.ModelDescriptions
@ModelType Collection(Of ApiParameterDescription)

@Code
    Dim requestModelDescription As ParameterModelDescription = ViewBag.modelDescription
End Code

<table class="help-page-table">
    <thead>
    <tr><th>Name</th><th>Description</th><th>Additional information</th></tr>
    </thead>
    <tbody>
    @For Each parameter As ApiParameterDescription In Model
        Dim parameterDocumentation As String = If(Not parameter.Documentation Is Nothing,
                                                  parameter.Documentation,
                                                  "No documentation available.")
        Dim hasModelDescription As Boolean = requestModelDescription IsNot Nothing AndAlso requestModelDescription.ParameterDescription Is parameter

        ' Don't show CancellationToken because it's a special parameter
        If (parameter.ParameterDescriptor Is Nothing Or
            (parameter.ParameterDescriptor IsNot Nothing AndAlso
            Not GetType(CancellationToken).IsAssignableFrom(parameter.ParameterDescriptor.ParameterType))) Then
            @<tr>
                <td class="parameter-name"><b>@parameter.Name</b></td>
                <td class="parameter-documentation">
                    @If (parameter.Documentation IsNot Nothing Or Not hasModelDescription) Then
                        @<p>@parameterDocumentation</p>
                    End If
                    @If (hasModelDescription) Then
                        @Html.DisplayFor(Function(m) requestModelDescription.ModelDescription)
                    End If
                </td>
                <td class="parameter-source">
                    @Select parameter.Source
                    Case ApiParameterSource.FromBody
                            @<p>Define this parameter in the request <b>body</b>.</p>
                    Case ApiParameterSource.FromUri
                            @<p>Define this parameter in the request <b>URI</b>.</p>
                    Case Else
                            @<p>None.</p>
                    End Select
                </td>
            </tr>
        End If
    Next
    </tbody>
</table>