/// <reference path="IntelliSense\References.js" />
///#RESTORE (function (global, $, upshot, undefined)
{
    var obs = upshot.observability;

    var metadata = {};

    upshot.metadata = function (entityType) {
        if (arguments.length === 0) {
            return $.extend({}, metadata);
        } else if (typeof entityType === "string") {
            if (arguments.length === 1) {
                return metadata[entityType];
            } else {
                if (!metadata[entityType]) {
                    metadata[entityType] = arguments[1];
                }
                // ...else assume the new metadata is the same as that previously registered for entityType.
            }
        } else {
            $.each(entityType, function (entityType, metadata) {
                upshot.metadata(entityType, metadata);
            });
        }
    }

    upshot.metadata.getProperties = function (entity, entityType, includeAssocations) {
        var props = [];
        if (entityType) {
            var metadata = upshot.metadata(entityType);
            if (metadata && metadata.fields) {
                // if metadata is present, we'll loop through the fields
                var fields = metadata.fields;
                for (var prop in fields) {
                    if (includeAssocations || !fields[prop].association) {
                        props.push({ name: prop, type: fields[prop].type, association: fields[prop].association });
                    }
                }
                return props;
            }
        }
        // otherwise we'll use the observability layer to infer the properties
        for (var prop in entity) {
            // TODO: determine if we want to allow the case where hasOwnProperty returns false (hierarchies, etc.)
            if (entity.hasOwnProperty(prop) && obs.isProperty(entity, prop) && (prop.indexOf("jQuery") !== 0)) {
                props.push({ name: prop });
            }
        }
        return props;
    }

    upshot.metadata.getPropertyType = function (entityType, property) {
        if (entityType) {
            var metadata = upshot.metadata(entityType);
            if (metadata && metadata.fields && metadata.fields[property]) {
                return metadata.fields[property].type;
            }
        }
        return null;
    }

    upshot.metadata.isEntityType = function (type) {
        if (type) {
            var metadata = upshot.metadata(type);
            if (metadata && metadata.key) {
                return true;
            }
        }
        return false;
    }

    var types = {};

    upshot.registerType = function (type, keyFunction) {
        /// <summary>
        /// Registers a type string for later access with a key.  This facility is convenient to avoid duplicate type string literals throughout your application scripts.  The key is expected to be returned by 'keyFunction', allowing the call to 'registerType' to precede the line of JavaScript declaring the key.  Typically, the returned key will be a constructor function for a JavaScript class corresponding to 'type'.
        /// </summary>
        /// <param name="keyFunction" type="Function">
        /// &#10;A function returning the key by which the type string will later be retrieved.
        /// </param>
        /// <returns type="String"/>

        if (upshot.isObject(type)) {
            // Allow for registrations that cover multiple types like:
            //   upshot.registerType({ "BarType": function () { return Bar; }, "FooType": function () { return Foo; } });
            $.each(type, function (type, key) {
                upshot.registerType(type, key);
            });
        } else {
            // Allow for single-type registrations:
            //   upshot.registerType("BarType", function () { return Bar; });
            var keyFunctions = types[type] || (types[type] = []);
            if ($.inArray(keyFunction, keyFunctions) < 0) {
                keyFunctions.push(keyFunction);
            }
        }
        return upshot;
    }

    upshot.type = function (key) {
        /// <summary>
        /// Returns the type string registered for a particular key.
        /// </summary>
        /// <param name="key">
        /// &#10;The key under which the desired type string is registered.
        /// </param>
        /// <returns type="String"/>

        var result;
        for (var type in types) {
            if (types.hasOwnProperty(type)) {
                var keyFunctions = types[type];
                for (var i = 0; i < keyFunctions.length; i++) {
                    if (keyFunctions[i]() === key) {
                        return type;
                    }
                }
            }
        }

        throw "No type string registered for key '" + key + "'.";
    }
}
///#RESTORE )(this, jQuery, upshot);