// These changes remove Intellisense "Function expected" or "Object expected" messages due to
// code that relies on running in the browser. Simply include a reference in
// before QUnit or jQuery like so: /// <reference path="{path}/IntellisenseFix.js" />
// There is no need to include this in the actual html page

//QUnit fixes
(function () {
    var test = document.getElementById("");
    if (test && !test.style) {
        var oldGet = document.getElementById;
        document.getElementById = function (id) {
            var el = oldGet(id);
            el.style = el.getAttribute("style");
            return el;
        };
    }

    if (window.location && !window.location.search) {
        window.location.search = "";
    }
})();

//jQuery fixes 1.6.1
(function () {
    if (!document.documentElement.childNodes[0]) {
        document.documentElement.childNodes = [{ nodeType: null}];
    }

    if (!location.href) {
        location.href = "";
    }
})();