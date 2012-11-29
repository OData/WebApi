@Imports System.Web.Http
@Imports System.Web.Http.Description
@Imports ROOT_PROJECT_NAMESPACE.Areas.HelpPage.Models
@ModelType HelpPageApiModel

@Code
    Dim description As ApiDescription = Model.ApiDescription
    ViewData("Title") = description.HttpMethod.Method + " " + description.RelativePath
End Code

<div id="body">
    <section class="featured">
        <div class="content-wrapper">
            <p>
                @Html.ActionLink("Help Page Home", "Index")
            </p>
        </div>
    </section>
    <section class="content-wrapper main-content clear-fix">
        @Html.DisplayFor(Function(m) Model)
    </section>
</div>

@Section Scripts
    <link type="text/css" href="~/Areas/HelpPage/HelpPage.css" rel="stylesheet" />
End Section