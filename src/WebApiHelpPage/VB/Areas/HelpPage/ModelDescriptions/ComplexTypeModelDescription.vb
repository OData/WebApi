Imports System.Collections.Generic

Namespace Areas.HelpPage.ModelDescriptions
    Public Class ComplexTypeModelDescription
        Inherits ModelDescription
        Private _properties As IList(Of PropertyDescription)

        Public Sub New()
            Properties = New List(Of PropertyDescription)()
        End Sub

        Public Property Properties() As IList(Of PropertyDescription)
            Get
                Return _properties
            End Get
            Private Set(value As IList(Of PropertyDescription))
                _properties = value
            End Set
        End Property
    End Class
End Namespace