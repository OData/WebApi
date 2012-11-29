@Imports System.Collections.ObjectModel
@Imports System.Web.Http.Description
@Imports System.Threading
@ModelType Collection(Of ApiParameterDescription)

<table class="help-page-table">
    <thead>
    <tr><th>Name</th><th>Description</th><th>Additional information</th></tr>
    </thead>
    <tbody>
    @For Each parameter As ApiParameterDescription In Model
        Dim parameterDocumentation As String = If(Not parameter.Documentation Is Nothing,
                                                  parameter.Documentation,
                                                  "No documentation available.")

        ' Don't show CancellationToken because it's a special parameter
        If (Not GetType(CancellationToken).IsAssignableFrom(parameter.ParameterDescriptor.ParameterType)) Then
            @<tr>
                <td class="parameter-name"><b>@parameter.Name</b></td>
                <td class="parameter-documentation"><pre>@parameterDocumentation</pre></td>
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