Imports System.Collections.Generic

Namespace Areas.HelpPage.ModelDescriptions
    Public Class EnumTypeModelDescription
        Inherits ModelDescription
        Private _values As IList(Of EnumValueDescription)

        Public Sub New()
            Values = New List(Of EnumValueDescription)()
        End Sub

        Public Property Values() As IList(Of EnumValueDescription)
            Get
                Return _values
            End Get
            Private Set(value As IList(Of EnumValueDescription))
                _values = value
            End Set
        End Property
    End Class
End Namespace