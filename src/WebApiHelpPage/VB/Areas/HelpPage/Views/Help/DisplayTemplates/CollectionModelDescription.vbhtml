@Imports ROOT_PROJECT_NAMESPACE.Areas.HelpPage.ModelDescriptions
@ModelType CollectionModelDescription
Collection of @Html.DisplayFor(Function(m) Model.ElementDescription.ModelType, "ModelDescriptionLink", New With { .modelDescription = Model.ElementDescription })