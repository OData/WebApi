Imports System.Collections.Generic

Namespace Areas.HelpPage.ModelDescriptions
    Public Class PropertyDescription
        Private _annotations As IList(Of PropertyAnnotation)
        Private _documentation As String
        Private _name As String
        Private _typeDescription As ModelDescription

        Public Sub New()
            Annotations = New List(Of PropertyAnnotation)()
        End Sub

        Public Property Annotations() As IList(Of PropertyAnnotation)
            Get
                Return _annotations
            End Get
            Private Set(value As IList(Of PropertyAnnotation))
                _annotations = value
            End Set
        End Property

        Public Property Documentation() As String
            Get
                Return _documentation
            End Get
            Set(value As String)
                _documentation = value
            End Set
        End Property

        Public Property Name() As String
            Get
                Return _name
            End Get
            Set(value As String)
                _name = value
            End Set
        End Property

        Public Property TypeDescription() As ModelDescription
            Get
                Return _typeDescription
            End Get
            Set(value As ModelDescription)
                _typeDescription = value
            End Set
        End Property
    End Class
End Namespace