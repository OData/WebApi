Imports System
Imports System.Globalization
Imports System.Linq
Imports System.Reflection
Imports System.Web.Http.Controllers
Imports System.Web.Http.Description
Imports System.Xml.XPath

Namespace Areas.HelpPage
    ''' <summary>
    ''' A custom <see cref="IDocumentationProvider"/> that reads the API documentation from an XML documentation file.
    ''' </summary>
    Public Class XmlDocumentationProvider
        Implements IDocumentationProvider

        Private _documentNavigator As XPathNavigator
        Private Const MethodExpression As String = "/doc/members/member[@name='M:{0}']"
        Private Const ParameterExpression As String = "param[@name='{0}']"

        ''' <summary>
        ''' Initializes a new instance of the <see cref="XmlDocumentationProvider"/> class.
        ''' </summary>
        ''' <param name="documentPath">The physical path to XML document.</param>
        Public Sub New(documentPath As String)
            If (documentPath Is Nothing) Then
                Throw New ArgumentNullException("documentPath")
            End If
            Dim xpath As New XPathDocument(documentPath)
            _documentNavigator = xpath.CreateNavigator()
        End Sub

        Public Function GetDocumentation(actionDescriptor As HttpActionDescriptor) As String Implements IDocumentationProvider.GetDocumentation
            Dim methodNode As XPathNavigator = GetMethodNode(actionDescriptor)
            If (Not methodNode Is Nothing) Then
                Dim summaryNode As XPathNavigator = methodNode.SelectSingleNode("summary")
                If (Not summaryNode Is Nothing) Then
                    Return summaryNode.Value.Trim()
                End If
            End If

            Return Nothing
        End Function

        Public Function GetDocumentation(parameterDescriptor As HttpParameterDescriptor) As String Implements IDocumentationProvider.GetDocumentation
            Dim reflectedParameterDescriptor As ReflectedHttpParameterDescriptor = TryCast(parameterDescriptor, ReflectedHttpParameterDescriptor)
            If (Not reflectedParameterDescriptor Is Nothing) Then
                Dim methodNode As XPathNavigator = GetMethodNode(reflectedParameterDescriptor.ActionDescriptor)
                If (Not methodNode Is Nothing) Then
                    Dim parameterName As String = reflectedParameterDescriptor.ParameterInfo.Name
                    Dim parameterNode As XPathNavigator = methodNode.SelectSingleNode(String.Format(CultureInfo.InvariantCulture, ParameterExpression, parameterName))
                    If (Not parameterNode Is Nothing) Then
                        Return parameterNode.Value.Trim()
                    End If
                End If
            End If

            Return Nothing
        End Function

        Private Function GetMethodNode(actionDescriptor As HttpActionDescriptor) As XPathNavigator
            Dim reflectedActionDescriptor As ReflectedHttpActionDescriptor = TryCast(actionDescriptor, ReflectedHttpActionDescriptor)
            If (Not reflectedActionDescriptor Is Nothing) Then
                Dim selectExpression As String = String.Format(CultureInfo.InvariantCulture, MethodExpression, GetMemberName(reflectedActionDescriptor.MethodInfo))
                Return _documentNavigator.SelectSingleNode(selectExpression)
            End If

            Return Nothing
        End Function

        Private Shared Function GetMemberName(method As MethodInfo) As String
            Dim name As String = String.Format(CultureInfo.InvariantCulture, "{0}.{1}", method.DeclaringType.FullName, method.Name)
            Dim parameters() As ParameterInfo = method.GetParameters()
            If (parameters.Length <> 0) Then
                Dim parameterTypeNames() As String = parameters.Select(Function(param) GetTypeName(param.ParameterType)).ToArray()
                name += String.Format(CultureInfo.InvariantCulture, "({0})", String.Join(",", parameterTypeNames))
            End If

            Return name
        End Function

        Private Shared Function GetTypeName(type As Type) As String
            If (type.IsGenericType) Then
                ' Format the generic type name to something like: Generic{System.Int32,System.String}
                Dim genericType As Type = type.GetGenericTypeDefinition()
                Dim genericArguments() As Type = type.GetGenericArguments()
                Dim typeName As String = genericType.FullName

                ' Trim the generic parameter counts from the name
                typeName = typeName.Substring(0, typeName.IndexOf("`"c))
                Dim argumentTypeNames() As String = genericArguments.Select(Function(t) GetTypeName(t)).ToArray()
                Return String.Format(CultureInfo.InvariantCulture, "{0}{{{1}}}", typeName, String.Join(",", argumentTypeNames))
            End If

            Return type.FullName
        End Function
    End Class
End Namespace