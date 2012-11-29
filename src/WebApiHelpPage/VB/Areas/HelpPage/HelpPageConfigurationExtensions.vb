Imports System
Imports System.Collections.Generic
Imports System.Collections.ObjectModel
Imports System.Diagnostics.CodeAnalysis
Imports System.Globalization
Imports System.Linq
Imports System.Net.Http.Headers
Imports System.Runtime.CompilerServices
Imports System.Web.Http
Imports System.Web.Http.Description
Imports ROOT_PROJECT_NAMESPACE.Areas.HelpPage.Models

Namespace Areas.HelpPage
    Public Module HelpPageConfigurationExtensions
        Private Const ApiModelPrefix As String = "MS_HelpPageApiModel_"

        ''' <summary>
        ''' Sets the documentation provider for help page.
        ''' </summary>
        ''' <param name="config">The <see cref="HttpConfiguration"/>.</param>
        ''' <param name="documentationProvider">The documentation provider.</param>
        <Extension()>
        Public Sub SetDocumentationProvider(ByVal config As HttpConfiguration, documentationProvider As IDocumentationProvider)
            config.Services.Replace(GetType(IDocumentationProvider), documentationProvider)
        End Sub

        ''' <summary>
        ''' Sets the objects that will be used by the formatters to produce sample requests/responses.
        ''' </summary>
        ''' <param name="config">The <see cref="HttpConfiguration"/>.</param>
        ''' <param name="sampleObjects">The sample objects.</param>
        <Extension()>
        Public Sub SetSampleObjects(ByVal config As HttpConfiguration, sampleObjects As IDictionary(Of Type, Object))
            config.GetHelpPageSampleGenerator().SampleObjects = sampleObjects
        End Sub

        ''' <summary>
        ''' Sets the sample request directly for the specified media type and action.
        ''' </summary>
        ''' <param name="config">The <see cref="HttpConfiguration"/>.</param>
        ''' <param name="sample">The sample request.</param>
        ''' <param name="mediaType">The media type.</param>
        ''' <param name="controllerName">Name of the controller.</param>
        ''' <param name="actionName">Name of the action.</param>
        <Extension()>
        Public Sub SetSampleRequest(ByVal config As HttpConfiguration, sample As Object, mediaType As MediaTypeHeaderValue, controllerName As String, actionName As String)
            config.GetHelpPageSampleGenerator().ActionSamples.Add(New HelpPageSampleKey(mediaType, SampleDirection.Request, controllerName, actionName, New String() {"*"}), sample)
        End Sub

        ''' <summary>
        ''' Sets the sample request directly for the specified media type and action with parameters.
        ''' </summary>
        ''' <param name="config">The <see cref="HttpConfiguration"/>.</param>
        ''' <param name="sample">The sample request.</param>
        ''' <param name="mediaType">The media type.</param>
        ''' <param name="controllerName">Name of the controller.</param>
        ''' <param name="actionName">Name of the action.</param>
        ''' <param name="parameterNames">The parameter names.</param>
        <Extension()>
        Public Sub SetSampleRequest(ByVal config As HttpConfiguration, sample As Object, mediaType As MediaTypeHeaderValue, controllerName As String, actionName As String, ByVal ParamArray parameterNames() As String)
            config.GetHelpPageSampleGenerator().ActionSamples.Add(New HelpPageSampleKey(mediaType, SampleDirection.Request, controllerName, actionName, parameterNames), sample)
        End Sub

        ''' <summary>
        ''' Sets the sample request directly for the specified media type of the action.
        ''' </summary>
        ''' <param name="config">The <see cref="HttpConfiguration"/>.</param>
        ''' <param name="sample">The sample response.</param>
        ''' <param name="mediaType">The media type.</param>
        ''' <param name="controllerName">Name of the controller.</param>
        ''' <param name="actionName">Name of the action.</param>
        <Extension()>
        Public Sub SetSampleResponse(ByVal config As HttpConfiguration, sample As Object, mediaType As MediaTypeHeaderValue, controllerName As String, actionName As String)
            config.GetHelpPageSampleGenerator().ActionSamples.Add(New HelpPageSampleKey(mediaType, SampleDirection.Response, controllerName, actionName, New String() {"*"}), sample)
        End Sub

        ''' <summary>
        ''' Sets the sample response directly for the specified media type of the action with specific parameters.
        ''' </summary>
        ''' <param name="config">The <see cref="HttpConfiguration"/>.</param>
        ''' <param name="sample">The sample response.</param>
        ''' <param name="mediaType">The media type.</param>
        ''' <param name="controllerName">Name of the controller.</param>
        ''' <param name="actionName">Name of the action.</param>
        ''' <param name="parameterNames">The parameter names.</param>
        <Extension()>
        Public Sub SetSampleResponse(ByVal config As HttpConfiguration, sample As Object, mediaType As MediaTypeHeaderValue, controllerName As String, actionName As String, ByVal ParamArray parameterNames() As String)
            config.GetHelpPageSampleGenerator().ActionSamples.Add(New HelpPageSampleKey(mediaType, SampleDirection.Response, controllerName, actionName, parameterNames), sample)
        End Sub

        ''' <summary>
        ''' Sets the sample directly for all actions with the specified type and media type.
        ''' </summary>
        ''' <param name="config">The <see cref="HttpConfiguration"/>.</param>
        ''' <param name="sample">The sample.</param>
        ''' <param name="mediaType">The media type.</param>
        ''' <param name="type">The parameter type or return type of an action.</param>
        <Extension()>
        Public Sub SetSampleForType(ByVal config As HttpConfiguration, sample As Object, mediaType As MediaTypeHeaderValue, type As Type)
            config.GetHelpPageSampleGenerator().ActionSamples.Add(New HelpPageSampleKey(mediaType, type), sample)
        End Sub

        ''' <summary>
        ''' Specifies the actual type of <see cref="System.Net.Http.ObjectContent(Of T)"/> passed to the <see cref="System.Net.Http.HttpRequestMessage"/> in an action. 
        ''' The help page will use this information to produce more accurate request samples.
        ''' </summary>
        ''' <param name="config">The <see cref="HttpConfiguration"/>.</param>
        ''' <param name="type">The type.</param>
        ''' <param name="controllerName">Name of the controller.</param>
        ''' <param name="actionName">Name of the action.</param>
        <Extension()>
        Public Sub SetActualRequestType(ByVal config As HttpConfiguration, type As Type, controllerName As String, actionName As String)
            config.GetHelpPageSampleGenerator().ActualHttpMessageTypes.Add(New HelpPageSampleKey(SampleDirection.Request, controllerName, actionName, New String() {"*"}), type)
        End Sub

        ''' <summary>
        ''' Specifies the actual type of <see cref="System.Net.Http.ObjectContent(Of T)"/> passed to the <see cref="System.Net.Http.HttpRequestMessage"/> in an action. 
        ''' The help page will use this information to produce more accurate request samples.
        ''' </summary>
        ''' <param name="config">The <see cref="HttpConfiguration"/>.</param>
        ''' <param name="type">The type.</param>
        ''' <param name="controllerName">Name of the controller.</param>
        ''' <param name="actionName">Name of the action.</param>
        ''' <param name="parameterNames">The parameter names.</param>
        <Extension()>
        Public Sub SetActualRequestType(ByVal config As HttpConfiguration, type As Type, controllerName As String, actionName As String, ByVal ParamArray parameterNames() As String)
            config.GetHelpPageSampleGenerator().ActualHttpMessageTypes.Add(New HelpPageSampleKey(SampleDirection.Request, controllerName, actionName, parameterNames), type)
        End Sub

        ''' <summary>
        ''' Specifies the actual type of <see cref="System.Net.Http.ObjectContent(Of T)"/> returned as part of the <see cref="System.Net.Http.HttpRequestMessage"/> in an action. 
        ''' The help page will use this information to produce more accurate response samples.
        ''' </summary>
        ''' <param name="config">The <see cref="HttpConfiguration"/>.</param>
        ''' <param name="type">The type.</param>
        ''' <param name="controllerName">Name of the controller.</param>
        ''' <param name="actionName">Name of the action.</param>
        <Extension()>
        Public Sub SetActualResponseType(ByVal config As HttpConfiguration, type As Type, controllerName As String, actionName As String)
            config.GetHelpPageSampleGenerator().ActualHttpMessageTypes.Add(New HelpPageSampleKey(SampleDirection.Response, controllerName, actionName, New String() {"*"}), type)
        End Sub

        ''' <summary>
        ''' Specifies the actual type of <see cref="System.Net.Http.ObjectContent(Of T)"/> returned as part of the <see cref="System.Net.Http.HttpRequestMessage"/> in an action. 
        ''' The help page will use this information to produce more accurate response samples.
        ''' </summary>
        ''' <param name="config">The <see cref="HttpConfiguration"/>.</param>
        ''' <param name="type">The type.</param>
        ''' <param name="controllerName">Name of the controller.</param>
        ''' <param name="actionName">Name of the action.</param>
        ''' <param name="parameterNames">The parameter names.</param>
        <Extension()>
        Public Sub SetActualResponseType(ByVal config As HttpConfiguration, type As Type, controllerName As String, actionName As String, ByVal ParamArray parameterNames() As String)
            config.GetHelpPageSampleGenerator().ActualHttpMessageTypes.Add(New HelpPageSampleKey(SampleDirection.Response, controllerName, actionName, parameterNames), type)
        End Sub

        ''' <summary>
        ''' Gets the help page sample generator.
        ''' </summary>
        ''' <param name="config">The <see cref="HttpConfiguration"/>.</param>
        ''' <returns>The help page sample generator.</returns>
        <Extension()>
        Public Function GetHelpPageSampleGenerator(ByVal config As HttpConfiguration) As HelpPageSampleGenerator
            Return DirectCast(config.Properties.GetOrAdd(
                GetType(HelpPageSampleGenerator),
                Function(k) New HelpPageSampleGenerator()), HelpPageSampleGenerator)
        End Function

        ''' <summary>
        ''' Sets the help page sample generator.
        ''' </summary>
        ''' <param name="config">The <see cref="HttpConfiguration"/>.</param>
        ''' <param name="sampleGenerator">The help page sample generator.</param>
        <Extension()>
        Public Sub SetHelpPageSampleGenerator(ByVal config As HttpConfiguration, sampleGenerator As HelpPageSampleGenerator)
            config.Properties.AddOrUpdate(
                GetType(HelpPageSampleGenerator),
                Function(k) sampleGenerator,
                Function(k, o) sampleGenerator)
        End Sub

        ''' <summary>
        ''' Gets the model that represents an API displayed on the help page. The model is initialized on the first call and cached for subsequent calls.
        ''' </summary>
        ''' <param name="config">The <see cref="HttpConfiguration"/>.</param>
        ''' <param name="apiDescriptionId">The <see cref="ApiDescription"/> ID.</param>
        ''' <returns>
        ''' An <see cref="HelpPageApiModel"/>
        ''' </returns>

        <Extension()>
        Public Function GetHelpPageApiModel(ByVal config As HttpConfiguration, apiDescriptionId As String) As HelpPageApiModel
            Dim model As New Object

            Dim modelId As String = ApiModelPrefix + apiDescriptionId
            If (Not config.Properties.TryGetValue(modelId, model)) Then
                Dim apiDescriptions As Collection(Of ApiDescription) = config.Services.GetApiExplorer().ApiDescriptions
                Dim ApiDescription As ApiDescription = apiDescriptions.FirstOrDefault(Function(api) String.Equals(api.GetFriendlyId(), apiDescriptionId, StringComparison.OrdinalIgnoreCase))
                If (Not ApiDescription Is Nothing) Then
                    Dim sampleGenerator As HelpPageSampleGenerator = config.GetHelpPageSampleGenerator()
                    model = GenerateApiModel(ApiDescription, sampleGenerator)
                    config.Properties.TryAdd(modelId, model)
                End If
            End If
            Return DirectCast(model, HelpPageApiModel)
        End Function

        <SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification:="The exception is recorded as ErrorMessages.")>
        Public Function GenerateApiModel(apiDescription As ApiDescription, sampleGenerator As HelpPageSampleGenerator) As HelpPageApiModel
            Dim apiModel As New HelpPageApiModel()
            apiModel.ApiDescription = apiDescription

            Try
                For Each item In sampleGenerator.GetSampleRequests(apiDescription)
                    apiModel.SampleRequests.Add(item.Key, item.Value)
                    LogInvalidSampleAsError(apiModel, item.Value)
                Next

                For Each item In sampleGenerator.GetSampleResponses(apiDescription)
                    apiModel.SampleResponses.Add(item.Key, item.Value)
                    LogInvalidSampleAsError(apiModel, item.Value)
                Next
            Catch e As Exception
                apiModel.ErrorMessages.Add(String.Format(CultureInfo.CurrentCulture, "An exception has occurred while generating the sample. Exception Message: {0}", e.Message))
            End Try

            Return apiModel
        End Function

        Private Sub LogInvalidSampleAsError(apiModel As HelpPageApiModel, sample As Object)
            Dim invalidSample As InvalidSample = TryCast(sample, InvalidSample)
            If (Not invalidSample Is Nothing) Then
                apiModel.ErrorMessages.Add(invalidSample.ErrorMessage)
            End If
        End Sub
    End Module
End Namespace