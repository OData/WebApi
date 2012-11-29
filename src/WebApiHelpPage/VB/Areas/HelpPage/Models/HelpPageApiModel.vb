Imports System.Collections.Generic
Imports System.Collections.ObjectModel
Imports System.Net.Http.Headers
Imports System.Web.Http.Description

Namespace Areas.HelpPage.Models
    ''' <summary>
    ''' The model that represents an API displayed on the help page.
    ''' </summary>
    Public Class HelpPageApiModel
        Private _sampleRequests As IDictionary(Of MediaTypeHeaderValue, Object)
        Private _sampleResponses As IDictionary(Of MediaTypeHeaderValue, Object)
        Private _errorMessages As Collection(Of String)
        Private _apiDescription As ApiDescription

        '''<summary>
        ''' Initializes a new instance of the <see cref="HelpPageApiModel"/> class.
        ''' </summary>
        Public Sub New()
            SampleRequests = New Dictionary(Of MediaTypeHeaderValue, Object)
            SampleResponses = New Dictionary(Of MediaTypeHeaderValue, Object)
            ErrorMessages = New Collection(Of String)
        End Sub

        ''' <summary>
        ''' Gets or sets the <see cref="ApiDescription"/> that describes the API.
        ''' </summary>
        Public Property ApiDescription As ApiDescription
            Get
                Return _apiDescription
            End Get
            Set(value As ApiDescription)
                _apiDescription = value
            End Set
        End Property

        ''' <summary>
        ''' Gets the sample requests associated with the API.
        ''' </summary>
        Public Property SampleRequests As IDictionary(Of MediaTypeHeaderValue, Object)
            Get
                Return _sampleRequests
            End Get
            Private Set(value As IDictionary(Of MediaTypeHeaderValue, Object))
                _sampleRequests = value
            End Set
        End Property

        ''' <summary>
        ''' Gets the sample responses associated with the API.
        ''' </summary>
        Public Property SampleResponses As IDictionary(Of MediaTypeHeaderValue, Object)
            Get
                Return _sampleResponses
            End Get
            Private Set(value As IDictionary(Of MediaTypeHeaderValue, Object))
                _sampleResponses = value
            End Set
        End Property

        ''' <summary>
        ''' Gets the error messages associated with this model.
        ''' </summary>
        Public Property ErrorMessages As Collection(Of String)
            Get
                Return _errorMessages
            End Get
            Private Set(value As Collection(Of String))
                _errorMessages = value
            End Set
        End Property
    End Class
End Namespace