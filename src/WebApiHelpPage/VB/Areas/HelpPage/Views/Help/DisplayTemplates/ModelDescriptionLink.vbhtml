@Imports ROOT_PROJECT_NAMESPACE.Areas.HelpPage.ModelDescriptions
@ModelType Type
@Code
    Dim modelDescription As ModelDescription = ViewBag.modelDescription
    If TypeOf modelDescription Is ComplexTypeModelDescription Or TypeOf modelDescription Is EnumTypeModelDescription Then
        If Model Is GetType(Object) Then
            @:Object
        Else
            @Html.ActionLink(modelDescription.Name, "ResourceModel", "Help", New With {.modelName = modelDescription.Name}, Nothing)
        End If
    Else
        @Html.DisplayFor(Function(m) modelDescription)
    End If
End Code