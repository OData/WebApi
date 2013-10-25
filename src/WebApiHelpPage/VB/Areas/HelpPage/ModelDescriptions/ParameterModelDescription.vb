Imports System.Web.Http.Description

Namespace Areas.HelpPage.ModelDescriptions
    Public Class ParameterModelDescription
        Private _modelDescription As ModelDescription
        Private _parameterDescription As ApiParameterDescription

        Public Property ModelDescription() As ModelDescription
            Get
                Return _modelDescription
            End Get
            Set(value As ModelDescription)
                _modelDescription = value
            End Set
        End Property

        Public Property ParameterDescription() As ApiParameterDescription
            Get
                Return _parameterDescription
            End Get
            Set(value As ApiParameterDescription)
                _parameterDescription = value
            End Set
        End Property
    End Class
End Namespace