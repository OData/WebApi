/// <reference path="IntelliSense\References.js" />
///#RESTORE (function (global, $, upshot, undefined)
{
    var observability = upshot.observability = upshot.observability || {};

    $.each(["track", "insert", "remove", "refresh", "isProperty", "getProperty", "setProperty", "isArray", "createCollection", "asArray", "map", "unmap", "setContextProperty"], function (index, value) {
        observability[value] = function () {
            // NOTE: observability.configuration is expected to be established by a loaded Upshot.Compat.<platform>.js.
            // TODO: Support apps and UI libraries that have no observability design.
            var config = observability.configuration;
            return config[value].apply(config, arguments);
        };
    });

    upshot.map = function (data, entityType, target) {
        // Interestingly, we don't use a "mapNested" parameter here (as Upshot proper does).
        // As a consequence, apps that call upshot.map from a map function will not pick up custom
        // map functions for nested entities/objects.  They'd need to hand-code calls to custom map
        // functions (ctors) for nested objects from the parent map function.
        return observability.configuration.map(data, entityType, null, target);
    };

}
///#RESTORE )(this, jQuery, upshot);