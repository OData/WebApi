@Imports ROOT_PROJECT_NAMESPACE.Areas.HelpPage.ModelDescriptions
@ModelType IList(Of PropertyDescription)

<ul>
    @For Each propertyDescription As PropertyDescription In Model
        Dim modelDescription As ModelDescription = propertyDescription.TypeDescription
        @<li>
            <b>@propertyDescription.Name</b>
            @If modelDescription IsNot Nothing Then
                @:(@Html.DisplayFor(Function(m) modelDescription.ModelType, "ModelDescriptionLink", New With { .modelDescription = modelDescription }))
            End If
            @If propertyDescription.Documentation IsNot Nothing Then
                @<p>@propertyDescription.Documentation</p>
            End If
            @If propertyDescription.Annotations.Count > 0 Then
                @<ul>
                @For Each annotation As PropertyAnnotation in propertyDescription.Annotations
                    @<li>@annotation.Documentation</li>
                Next
                </ul>
            End If
        </li>
    Next
</ul>