Imports System
Imports System.Collections.Generic
Imports System.Net.Http.Headers
Imports System.Web
Imports System.Web.Http

Namespace Areas.HelpPage
    ''' <summary>
    ''' Use this class to customize the Help Page.
    ''' For example you can set a custom <see cref="System.Web.Http.Description.IDocumentationProvider"/> to supply the documentation
    ''' or you can provide the samples for the requests/responses.
    ''' </summary>
    Public Module HelpPageConfig
        Public Sub Register(config As HttpConfiguration)
            '' Uncomment the following to use the documentation from XML documentation file.
            'config.SetDocumentationProvider(New XmlDocumentationProvider(HttpContext.Current.Server.MapPath("~/App_Data/XmlDocument.xml")))

            '' Uncomment the following to use "sample string" as the sample for all actions that have string as the body parameter or return type.
            '' Also, the string arrays will be used for IEnumerable(Of String). The sample objects will be serialized into different media type 
            '' formats by the available formatters.
            'config.SetSampleObjects(New Dictionary(Of Type, Object) From
            '{
            '     {GetType(String), "sample string"},
            '     {GetType(IEnumerable(Of String)), New String() {"sample 1", "sample 2"}}
            '})

            '' Uncomment the following to use "[0]=foo&[1]=bar" directly as the sample for all actions that support form URL encoded format
            '' and have IEnumerable(Of String) as the body parameter or return type.
            'config.SetSampleForType("[0]=foo&[1]=bar", New MediaTypeHeaderValue("application/x-www-form-urlencoded"), GetType(IEnumerable(Of String)))

            '' Uncomment the following to use "1234" directly as the request sample for media type "text/plain" on the controller named "Values"
            '' and action named "Put".
            'config.SetSampleRequest("1234", New MediaTypeHeaderValue("text/plain"), "Values", "Put")

            '' Uncomment the following to use the image on "../images/aspNetHome.png" directly as the response sample for media type "image/png"
            '' on the controller named "Values" and action named "Get" with parameter "id".
            'config.SetSampleResponse(New ImageSample("../images/aspNetHome.png"), New MediaTypeHeaderValue("image/png"), "Values", "Get", "id")

            '' Uncomment the following to correct the sample request when the action expects an HttpRequestMessage with ObjectContent(Of string).
            '' The sample will be generated as if the controller named "Values" and action named "Get" were having String as the body parameter.
            'config.SetActualRequestType(GetType(String), "Values", "Get")

            '' Uncomment the following to correct the sample response when the action returns an HttpResponseMessage with ObjectContent(Of String).
            '' The sample will be generated as if the controller named "Values" and action named "Post" were returning a String.
            'config.SetActualResponseType(GetType(String), "Values", "Post")
        End Sub
    End Module
End Namespace