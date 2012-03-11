/// <reference path="../Scripts/References.js" />
/// <reference path="TestGen.js" />

// Start the tests after all scripts have been dynamically loaded
QUnit.config.autostart = false;

// Resets the database and starts test generation
var TestPri = (function ($) {
    /// <param name="$" type="jQuery" />

    function loadScripts() {
        // Scripts are now a variable that can be dynamically modified to switch to vsdoc or minified.
        var scriptsToLoad = [
            "upshot/ChangeTracking.tests.js",
            "upshot/Consistency.tests.js",
            "upshot/Core.tests.js",
            "upshot/DataContext.tests.js",
            "upshot/DataProvider.tests.js",
            "upshot/DataSource.Common.js",
            "upshot/DataSource.tests.js",
            "upshot/Delete.tests.js",
            "upshot/EntitySet.tests.js",
            "upshot/jQuery.DataView.tests.js",
            "upshot/Mapping.tests.js",
            "upshot/RecordSet.js"
        ];

        // Avoid browser caching by appending a query string to the url ?13099...
        $.each(scriptsToLoad, function (i, item) {
            scriptsToLoad[i] = item + "?" + (new Date()).getTime();
        });

        return $.getScriptByReference("upshot/Datasets.js").pipe(function () {
            return $.whenAll($.getScriptsByReference(scriptsToLoad));
        });
    }

    $(window).load(function () { loadScripts().then(QUnit.start); });  // QUnit initializes on window load.  Don't call QUnit.start until then.

    return 0;
})(jQuery);