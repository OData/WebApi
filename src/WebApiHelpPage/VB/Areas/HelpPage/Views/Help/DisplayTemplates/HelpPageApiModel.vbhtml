@Imports System.Web.Http
@Imports System.Web.Http.Description
@Imports ROOT_PROJECT_NAMESPACE.Areas.HelpPage.Models
@ModelType HelpPageApiModel

@Code
    Dim description As ApiDescription = Model.ApiDescription
    Dim hasParameters As Boolean = description.ParameterDescriptions.Count > 0
    Dim hasRequestSamples As Boolean = Model.SampleRequests.Count > 0
    Dim hasResponseSamples As Boolean = Model.SampleResponses.Count > 0
End Code
<h1>@description.HttpMethod.Method @description.RelativePath</h1>
<div>
    @If Not description.Documentation Is Nothing Then
        @<p>@description.Documentation</p>
    Else
        @<p>No documentation available.</p>
    End If

    @If hasParameters Or hasRequestSamples Then
        @<h2>Request Information</h2>
        If hasParameters Then
            @<h3>Parameters</h3>
            @Html.DisplayFor(Function(apiModel) apiModel.ApiDescription.ParameterDescriptions, "Parameters")
        End If
        If hasRequestSamples Then
            @<h3>Request body formats</h3>
            @Html.DisplayFor(Function(apiModel) apiModel.SampleRequests, "Samples")
        End If
    End If

    @If hasResponseSamples Then
        @<h2>Response Information</h2>
        If Not description.ResponseDescription.Documentation Is Nothing Then
            @<p>@description.ResponseDescription.Documentation</p>
        Else
            @<p>No documentation available.</p>
        End If
        @<h3>Response body formats</h3>
        @Html.DisplayFor(Function(apiModel) apiModel.SampleResponses, "Samples")
    End If
</div>